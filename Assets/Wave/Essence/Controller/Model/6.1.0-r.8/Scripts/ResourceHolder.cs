// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;
using Wave.Native;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.Text;

namespace Wave.Essence.Controller.Model
{
	[System.Serializable]
	public class BatteryIndicator
	{
		public int level;
		public float min;
		public float max;
		public string texturePath;
		public bool textureLoaded;
		public Texture2D batteryTexture;
		public TextureInfo batteryTextureInfo;
	}

	[System.Serializable]
	public class TouchSetting
	{
		public Vector3 touchForward;
		public Vector3 touchCenter;
		public Vector3 touchRight;
		public Vector3 touchPtU;
		public Vector3 touchPtW;
		public Vector3 touchPtV;
		public float raidus;
		public float touchptHeight;
	}

	[System.Serializable]
	public class TextureInfo
	{
		public byte[] modelTextureData;
		public int width;
		public int height;
		public int stride;
		public int size;
		public int format;
	}

	[System.Serializable]
	public class ModelResource
	{
		public string renderModelName;
		public bool loadFromAsset;
		public bool mergeToOne;
		public XR_Hand hand;
		public Thread loaderThread;
		public bool isLoading;  // need lock
		public readonly object lockObj = new object();

		public uint sectionCount;
		public FBXInfo_t[] FBXInfo;
		public MeshInfo_t[] SectionInfo;
		public bool parserReady;

		public int modelTextureCount;
		public Texture2D[] modelTextures;
		public TextureInfo[] modelTextureInfos;
		public bool[] textureUpsideDowns;  // for modelTextures, if textures is loaded from asset, this need be true.

		public bool isTouchSetting;
		public TouchSetting TouchSetting;

		public bool isBatterySetting;
		public List<BatteryIndicator> batteryTextureList;

		public void Release()
		{
			lock (lockObj)
			{
				if (loaderThread != null)
				{
					Log.i("ResourceHolder", "MR Abort thread " + loaderThread.Name);
					loaderThread.Abort();
					loaderThread = null;
				}
			}
			for (int i = 0; i < batteryTextureList.Count; i++)
			{
				UnityEngine.Object.Destroy(batteryTextureList[i].batteryTexture);
				batteryTextureList[i].batteryTexture = null;
			}
			batteryTextureList.Clear();
			for (int i = 0; i < modelTextures.Length; i++)
			{
				UnityEngine.Object.Destroy(modelTextures[i]);
				modelTextures[i] = null;
			}
		}
	}

	public class ResourceHolder
	{
		private static string TAG = "ResourceHolder";

		private static ResourceHolder instance = null;
		public static ResourceHolder Instance
		{
			get
			{
				if (instance == null)
				{
					Log.i(TAG, "create ResourceHolder instance");

					instance = new ResourceHolder();
				}
				return instance;
			}
		}

		public List<ModelResource> renderModelList = new List<ModelResource>();
		public Texture2D controllerTextureInProject = null;

		public bool isRenderModelExist(string renderModel, XR_Hand hand, bool merge)
		{
			foreach (ModelResource t in renderModelList)
			{
				if ((t.renderModelName == renderModel) && (t.mergeToOne == merge) && (t.hand == hand))
				{
					return true;
				}
			}

			return false;
		}

		public ModelResource getRenderModelResource(string renderModel, XR_Hand hand, bool merge)
		{
			foreach (ModelResource t in renderModelList)
			{
				if ((t.renderModelName == renderModel) && (t.mergeToOne == merge) && (t.hand == hand))
				{
					return t;
				}
			}

			return null;
		}

		private void GetNativeControllerModel(XR_Hand hand, ModelResource targetMR, bool isOneBone)
		{
			var sb = new StringBuilder();
			string TAG_LOCAL = sb.Clear().Append(TAG).Append(GetHandName(hand)).ToString();
			Log.i(TAG_LOCAL, sb.Clear()
				.Append("GetNativeControllerModel start, IntPtrSize=").Append(IntPtr.Size)
				.Append(", hand=").Append(GetHandName(hand)).Append(", isOneBone=").Append(isOneBone));

			ModelResource mr = new ModelResource
			{
				isLoading = true,
				renderModelName = targetMR.renderModelName,
				mergeToOne = targetMR.mergeToOne,
				parserReady = false,
				hand = hand
			};
			IntPtr ctrlModel = IntPtr.Zero;
			int IntBits = IntPtr.Size;

			WVR_DeviceType deviceType = (hand == XR_Hand.Dominant) ? WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left;

			WVR_Result r = Interop.WVR_GetCurrentControllerModel(deviceType, ref ctrlModel, isOneBone);

			if (r != WVR_Result.WVR_Success)
			{
				Log.w(TAG_LOCAL, "WVR_GetCurrentControllerModel fail!");
				lock (targetMR.lockObj)
				{
					targetMR.isLoading = false;
					return;
				}
			}
			if (ctrlModel == IntPtr.Zero)
			{
				Log.w(TAG_LOCAL, "WVR_GetCurrentControllerModel return model is null");
				lock (targetMR.lockObj)
				{
					targetMR.isLoading = false;
					return;
				}
			}

			Log.i(TAG_LOCAL, sb.Clear()
				.Append("WVR_GetCurrentControllerModel, ctrlModel IntPtr=").Append(ctrlModel.ToInt64())
				.Append(", sizeof(WVR_CtrlerModel)=").Append(Marshal.SizeOf(typeof(WVR_CtrlerModel))));

			{
				WVR_CtrlerModel ctrl = (WVR_CtrlerModel)Marshal.PtrToStructure(ctrlModel, typeof(WVR_CtrlerModel));

				Log.i(TAG_LOCAL, sb.Clear()
					.Append("render model name=").Append(ctrl.name)
					.Append(", load from asset=").Append(ctrl.loadFromAsset));

				WVR_CtrlerCompInfoTable cit = ctrl.compInfos;

				int szStruct = Marshal.SizeOf(typeof(WVR_CtrlerCompInfo));

				Log.i(TAG_LOCAL, sb.Clear().Append("Controller component size=").Append(cit.size));

				mr.FBXInfo = new FBXInfo_t[cit.size];
				mr.sectionCount = cit.size;
				mr.SectionInfo = new MeshInfo_t[cit.size];
				mr.loadFromAsset = ctrl.loadFromAsset;

				for (int i = 0; i < cit.size; i++)
				{
					WVR_CtrlerCompInfo wcci;

					if (IntBits == 4)
						wcci = (WVR_CtrlerCompInfo)Marshal.PtrToStructure(new IntPtr(cit.table.ToInt32() + (szStruct * i)), typeof(WVR_CtrlerCompInfo));
					else
						wcci = (WVR_CtrlerCompInfo)Marshal.PtrToStructure(new IntPtr(cit.table.ToInt64() + (szStruct * i)), typeof(WVR_CtrlerCompInfo));

					mr.FBXInfo[i] = new FBXInfo_t();
					mr.SectionInfo[i] = new MeshInfo_t();

					mr.FBXInfo[i].meshName = Marshal.StringToHGlobalAnsi(wcci.name);
					mr.SectionInfo[i]._active = wcci.defaultDraw;

					// local matrix
					Matrix4x4 lt = TransformConverter.RigidTransform.toMatrix44(wcci.localMat, false);
					Matrix4x4 t = TransformConverter.RigidTransform.RowColumnInverse(lt);
					Log.i(TAG_LOCAL, sb.Clear()
						.Append("Controller component name=").Append(wcci.name)
						.Append(", tex index=").Append(wcci.texIndex)
						.Append(", active=").Append(mr.SectionInfo[i]._active)
						.AppendMatrix(", CtrlMatrix", t));

					mr.FBXInfo[i].matrix = TransformConverter.RigidTransform.ToWVRMatrix(t, false);

					WVR_VertexBuffer vertices = wcci.vertices;

					if (vertices.dimension == 3)
					{
						uint verticesCount = (vertices.size / vertices.dimension);

						Log.i(TAG_LOCAL, sb.Clear().Append(" vertices size=").Append(vertices.size).Append(", dimension=").Append(vertices.dimension).Append(", count=").Append(verticesCount));

						mr.SectionInfo[i]._vectice = new Vector3[verticesCount];
						float[] verticeArray = new float[vertices.size];

						Marshal.Copy(vertices.buffer, verticeArray, 0, verticeArray.Length);

						int verticeIndex = 0;
						int floatIndex = 0;

						while (verticeIndex < verticesCount)
						{
							mr.SectionInfo[i]._vectice[verticeIndex] = new Vector3();
							mr.SectionInfo[i]._vectice[verticeIndex].x = verticeArray[floatIndex++];
							mr.SectionInfo[i]._vectice[verticeIndex].y = verticeArray[floatIndex++];
							mr.SectionInfo[i]._vectice[verticeIndex].z = verticeArray[floatIndex++] * -1.0f;

							verticeIndex++;
						}
					}
					else
					{
						Log.w(TAG_LOCAL, "vertices buffer's dimension incorrect!");
					}

					// normals
					WVR_VertexBuffer normals = wcci.normals;

					if (normals.dimension == 3)
					{
						uint normalsCount = (normals.size / normals.dimension);
						Log.i(TAG_LOCAL, sb.Clear().Append(" normals size=").Append(normals.size).Append(", dimension=").Append(normals.dimension).Append(", count=").Append(normalsCount));
						mr.SectionInfo[i]._normal = new Vector3[normalsCount];
						float[] normalArray = new float[normals.size];

						Marshal.Copy(normals.buffer, normalArray, 0, normalArray.Length);

						int normalsIndex = 0;
						int floatIndex = 0;

						while (normalsIndex < normalsCount)
						{
							mr.SectionInfo[i]._normal[normalsIndex] = new Vector3();
							mr.SectionInfo[i]._normal[normalsIndex].x = normalArray[floatIndex++];
							mr.SectionInfo[i]._normal[normalsIndex].y = normalArray[floatIndex++];
							mr.SectionInfo[i]._normal[normalsIndex].z = normalArray[floatIndex++] * -1.0f;

							normalsIndex++;
						}

						Log.i(TAG_LOCAL, sb.Clear().Append(" normals size=").Append(normals.size).Append(", dimension=").Append(normals.dimension).Append(", count=").Append(normalsCount));
					}
					else
					{
						Log.w(TAG_LOCAL, "normals buffer's dimension incorrect!");
					}

					// texCoord
					WVR_VertexBuffer texCoord = wcci.texCoords;

					if (texCoord.dimension == 2)
					{
						uint uvCount = (texCoord.size / texCoord.dimension);
						Log.i(TAG_LOCAL, sb.Clear().Append(" texCoord size=").Append(texCoord.size).Append(", dimension=").Append(texCoord.dimension).Append(", count=").Append(uvCount));
						mr.SectionInfo[i]._uv = new Vector2[uvCount];
						float[] texCoordArray = new float[texCoord.size];

						Marshal.Copy(texCoord.buffer, texCoordArray, 0, texCoordArray.Length);

						int uvIndex = 0;
						int floatIndex = 0;

						while (uvIndex < uvCount)
						{
							mr.SectionInfo[i]._uv[uvIndex] = new Vector2();
							mr.SectionInfo[i]._uv[uvIndex].x = texCoordArray[floatIndex++];
							mr.SectionInfo[i]._uv[uvIndex].y = texCoordArray[floatIndex++];

							uvIndex++;
						}
					}
					else
					{
						Log.w(TAG_LOCAL, "normals buffer's dimension incorrect!");
					}

					// indices
					WVR_IndexBuffer indices = wcci.indices;
					Log.i(TAG_LOCAL, sb.Clear().Append(" indices size=").Append(indices.size));

					mr.SectionInfo[i]._indice = new int[indices.size];
					Marshal.Copy(indices.buffer, mr.SectionInfo[i]._indice, 0, mr.SectionInfo[i]._indice.Length);

					uint indiceIndex = 0;

					while (indiceIndex < indices.size)
					{
						int tmp = mr.SectionInfo[i]._indice[indiceIndex];
						mr.SectionInfo[i]._indice[indiceIndex] = mr.SectionInfo[i]._indice[indiceIndex + 2];
						mr.SectionInfo[i]._indice[indiceIndex + 2] = tmp;
						indiceIndex += 3;
					}
				}

				// Controller texture section
				WVR_CtrlerTexBitmapTable wctbt = ctrl.bitmapInfos;
				Log.i(TAG_LOCAL, sb.Clear().Append("Controller textures=").Append(wctbt.size));
				int bmStruct = Marshal.SizeOf(typeof(WVR_CtrlerTexBitmap));
				mr.modelTextureCount = (int)wctbt.size;
				mr.modelTextureInfos = new TextureInfo[wctbt.size];
				mr.modelTextures = new Texture2D[wctbt.size];
				mr.textureUpsideDowns = new bool[wctbt.size];

				for (int mt = 0; mt < wctbt.size; mt++)
				{
					TextureInfo ct = new TextureInfo();

					WVR_CtrlerTexBitmap wctb;

					if (IntBits == 4)
						wctb = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wctbt.table.ToInt32() + (bmStruct * mt)), typeof(WVR_CtrlerTexBitmap));
					else
						wctb = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wctbt.table.ToInt64() + (bmStruct * mt)), typeof(WVR_CtrlerTexBitmap));

					Log.i(TAG_LOCAL, sb.Clear()
						.Append(" [").Append(mt)
						.Append("] bitmap w=").Append(wctb.width)
						.Append(", h=").Append(wctb.height)
						.Append(", st=").Append(wctb.stride)
						.Append(", fmt=").Append(wctb.format));

					// bitmap size
					var rawImageSize = wctb.height * wctb.stride;

					ct.modelTextureData = new byte[rawImageSize];
					Marshal.Copy(wctb.bitmap, ct.modelTextureData, 0, ct.modelTextureData.Length);
					ct.width = (int)wctb.width;
					ct.height = (int)wctb.height;
					ct.stride = (int)wctb.stride;
					ct.format = (int)wctb.format;
					ct.size = (int)rawImageSize;

					mr.modelTextureInfos[mt] = ct;
				}

				// Touchpad section
				Log.d(TAG_LOCAL, "---  Get touch info from runtime  ---");
				WVR_TouchPadPlane wtpp = ctrl.touchpadPlane;

				mr.TouchSetting = new TouchSetting();
				mr.TouchSetting.touchCenter.x = wtpp.center.v0 * 100f;
				mr.TouchSetting.touchCenter.y = wtpp.center.v1 * 100f;
				mr.TouchSetting.touchCenter.z = (-1.0f * wtpp.center.v2) * 100f;

				mr.TouchSetting.raidus = wtpp.radius * 100;

				mr.TouchSetting.touchptHeight = wtpp.floatingDistance * 100;

				mr.isTouchSetting = wtpp.valid;

				mr.TouchSetting.touchPtU.x = wtpp.u.v0;
				mr.TouchSetting.touchPtU.y = wtpp.u.v1;
				mr.TouchSetting.touchPtU.z = wtpp.u.v2;

				mr.TouchSetting.touchPtV.x = wtpp.v.v0;
				mr.TouchSetting.touchPtV.y = wtpp.v.v1;
				mr.TouchSetting.touchPtV.z = wtpp.v.v2;

				mr.TouchSetting.touchPtW.x = wtpp.w.v0;
				mr.TouchSetting.touchPtW.y = wtpp.w.v1;
				mr.TouchSetting.touchPtW.z = -1.0f * wtpp.w.v2;
				Log.i(TAG_LOCAL, sb.Clear()
					.AppendVector3(" touchCenter!", mr.TouchSetting.touchCenter).AppendLine()
					.Append(" Floating distance=").Append(mr.TouchSetting.touchptHeight).AppendLine()
					.AppendVector3(" touchPtW!", mr.TouchSetting.touchPtW).AppendLine()
					.AppendVector3(" touchPtU!", mr.TouchSetting.touchPtU).AppendLine()
					.AppendVector3(" touchPtV!", mr.TouchSetting.touchPtV).AppendLine()
					.Append(" raidus=").Append(mr.TouchSetting.raidus).AppendLine()
					.Append(" isTouchSetting=").Append(mr.isTouchSetting));

				// Battery section
				Log.d(TAG_LOCAL, "---  Get battery info from runtime  ---");
				WVR_BatteryLevelTable wblt = ctrl.batteryLevels;

				List<BatteryIndicator> batteryTextureList = new List<BatteryIndicator>();
				mr.batteryTextureList = batteryTextureList;

				Log.i(TAG_LOCAL, sb.Clear().Append("Battery levels=").Append(wblt.size));

				int btStruct = Marshal.SizeOf(typeof(WVR_CtrlerTexBitmap));
				int sizeInt = Marshal.SizeOf(typeof(int));

				for (int b = 0; b < wblt.size; b++)
				{
					WVR_CtrlerTexBitmap batteryImage;
					int batteryMin = 0;
					int batteryMax = 0;

					if (IntBits == 4)
					{
						batteryImage = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wblt.texTable.ToInt32() + (btStruct * b)), typeof(WVR_CtrlerTexBitmap));
						batteryMin = (int)Marshal.PtrToStructure(new IntPtr(wblt.minTable.ToInt32() + (sizeInt * b)), typeof(int));
						batteryMax = (int)Marshal.PtrToStructure(new IntPtr(wblt.maxTable.ToInt32() + (sizeInt * b)), typeof(int));
					}
					else
					{
						batteryImage = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wblt.texTable.ToInt64() + (btStruct * b)), typeof(WVR_CtrlerTexBitmap));
						batteryMin = (int)Marshal.PtrToStructure(new IntPtr(wblt.minTable.ToInt64() + (sizeInt * b)), typeof(int));
						batteryMax = (int)Marshal.PtrToStructure(new IntPtr(wblt.maxTable.ToInt64() + (sizeInt * b)), typeof(int));
					}

					BatteryIndicator tmpBI = new BatteryIndicator();
					tmpBI.level = b;
					tmpBI.min = (float)batteryMin;
					tmpBI.max = (float)batteryMax;

					var batteryImageSize = batteryImage.height * batteryImage.stride;

					tmpBI.batteryTextureInfo = new TextureInfo();
					tmpBI.batteryTextureInfo.modelTextureData = new byte[batteryImageSize];
					Marshal.Copy(batteryImage.bitmap, tmpBI.batteryTextureInfo.modelTextureData, 0, tmpBI.batteryTextureInfo.modelTextureData.Length);
					tmpBI.batteryTextureInfo.width = (int)batteryImage.width;
					tmpBI.batteryTextureInfo.height = (int)batteryImage.height;
					tmpBI.batteryTextureInfo.stride = (int)batteryImage.stride;
					tmpBI.batteryTextureInfo.format = (int)batteryImage.format;
					tmpBI.batteryTextureInfo.size = (int)batteryImageSize;
					tmpBI.textureLoaded = true;
					Log.i(TAG_LOCAL, sb.Clear()
						.Append(" Battery Level[").Append(tmpBI.level)
						.Append("] min=").Append(tmpBI.min)
						.Append(" max=").Append(tmpBI.max)
						.Append(" loaded=")
						.Append(tmpBI.textureLoaded)
						.Append(" w=").Append(batteryImage.width)
						.Append(" h=").Append(batteryImage.height)
						.Append(" size=").Append(batteryImageSize));

					batteryTextureList.Add(tmpBI);
				}
				mr.isBatterySetting = true;

				Log.i(TAG_LOCAL, sb.Clear().Append("WVR_ReleaseControllerModel, ctrlModel IntPtr=").Append(ctrlModel.ToInt64()));
				Interop.WVR_ReleaseControllerModel(ref ctrlModel);
			}

			lock (targetMR.lockObj)
			{
				Log.i(TAG_LOCAL, "Copy data into targetMR");

				// Avoid multithread problem, copy to targetMR after finish loading.
				targetMR.modelTextures = mr.modelTextures;
				targetMR.textureUpsideDowns = mr.textureUpsideDowns;
				targetMR.modelTextureInfos = mr.modelTextureInfos;
				targetMR.modelTextureCount = mr.modelTextureCount;

				targetMR.isBatterySetting = mr.isBatterySetting;
				targetMR.batteryTextureList = mr.batteryTextureList;

				targetMR.isTouchSetting = mr.isTouchSetting;
				targetMR.TouchSetting = mr.TouchSetting;

				targetMR.sectionCount = mr.sectionCount;
				targetMR.FBXInfo = mr.FBXInfo;
				targetMR.SectionInfo = mr.SectionInfo;

				targetMR.loadFromAsset = mr.loadFromAsset;

				targetMR.parserReady = true;
				targetMR.isLoading = false;
				Log.i(TAG_LOCAL, "Call WVR_GetCurrentControllerModel end");
				targetMR.loaderThread = null;
			}
		}

		public string GetHandName(XR_Hand hand)
		{
			switch (hand)
			{
				case XR_Hand.Right:
					return "R";
				case XR_Hand.Left:
					return "L";
				default:
					return "UnknownHand";
			}
		}


		public bool addRenderModel(string renderModel, XR_Hand hand, bool merge)
		{
			ModelResource mr = getRenderModelResource(renderModel, hand, merge);
			if (mr != null)
			{
				lock (mr.lockObj)
				{
					if (mr.parserReady)
					{
						Log.i(TAG, Log.CSB.Append(GetHandName(hand)).Append(" ").Append(renderModel).Append(" is already added.  Skip it."));
						return false;
					}
					if (mr.isLoading)
					{
						Log.i(TAG, Log.CSB.Append(GetHandName(hand)).Append(" ").Append(renderModel).Append(" is not finish loading."));
						return false;
					}
					// Not finish parser, but not loading, it means the parser is failed.
					Log.i(TAG, Log.CSB.Append(GetHandName(hand)).Append(" ").Append(renderModel).Append(" is not finish loading.  Resume it."));

					// reset loading flags
					mr.isLoading = true;
					mr.parserReady = false;
				}
			}
			else
			{
				mr = new ModelResource
				{
					isLoading = true,
					renderModelName = renderModel,
					mergeToOne = merge,
					parserReady = false,
					hand = hand
				};
				renderModelList.Add(mr);
			}

			Log.i(TAG, "Initial a thread to load current controller model assets");
			mr.loaderThread = new Thread(() => GetNativeControllerModel(hand, mr, merge))
			{
				Name = "ResourceHolder" + GetHandName(hand)
			};
			mr.loaderThread.Start();

			return true;
		}

		public void Release()
		{
			Log.i(TAG, "Release");
			for (int i = 0; i < renderModelList.Count; i++)
			{
				var mr = renderModelList[i];
				if (mr == null) continue;
				mr.Release();
			}
			renderModelList.Clear();

			if (controllerTextureInProject != null)
				Resources.UnloadAsset(controllerTextureInProject);
			controllerTextureInProject = null;
			instance = null;
		}

		public void Release(string renderModel, XR_Hand hand, bool merge)
		{
			int found = -1;
			for (int i = 0; i < renderModelList.Count; i++)
			{
				var t = renderModelList[i];
				if ((t.renderModelName == renderModel) && (t.mergeToOne == merge) && (t.hand == hand))
				{
					found = i;
					break;
				}
			}
			if (found > 0)
			{
				var mr = renderModelList[found];
				mr.Release();
				renderModelList.RemoveAt(found);
			}
		}

		public void Release(ModelResource mr)
		{
			mr.Release();
			renderModelList.Remove(mr);
		}
	}
}
