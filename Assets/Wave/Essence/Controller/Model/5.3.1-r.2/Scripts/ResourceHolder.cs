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

		public uint sectionCount;
		public FBXInfo_t[] FBXInfo;
		public MeshInfo_t[] SectionInfo;
		public bool parserReady;

		public int modelTextureCount;
		public Texture2D[] modelTexture;
		public TextureInfo[] modelTextureInfo;

		public bool isTouchSetting;
		public TouchSetting TouchSetting;

		public bool isBatterySetting;
		public List<BatteryIndicator> batteryTextureList;
	}

	public class ResourceHolder
	{
		private static string TAG = "ResourceHolder";
		private Thread mthread;

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

		private void getNativeControllerModel(XR_Hand hand, ModelResource curr, bool isOneBone)
		{
			Log.i(TAG, Log.CSB
				.Append("getNativeControllerModel start, IntPtr size=").Append(IntPtr.Size)
				.Append(", isOneBone=").Append(isOneBone));

			IntPtr ctrlModel = IntPtr.Zero;
			int IntBits = IntPtr.Size;

			WVR_DeviceType deviceType = (hand == XR_Hand.Dominant) ? WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left;

			WVR_Result r = Interop.WVR_GetCurrentControllerModel(deviceType, ref ctrlModel, isOneBone);

			if (r != WVR_Result.WVR_Success)
			{
				Log.w(TAG, "WVR_GetCurrentControllerModel fail!");
				return;
			}
			if (ctrlModel == IntPtr.Zero)
			{
				Log.w(TAG, "WVR_GetCurrentControllerModel return model is null");
				return;
			}

			Log.i(TAG, Log.CSB
				.Append("WVR_GetCurrentControllerModel, ctrlModel IntPtr=").Append(ctrlModel.ToInt64())
				.Append(", sizeof(WVR_CtrlerModel)=").Append(Marshal.SizeOf(typeof(WVR_CtrlerModel))));

			{
				WVR_CtrlerModel ctrl = (WVR_CtrlerModel)Marshal.PtrToStructure(ctrlModel, typeof(WVR_CtrlerModel));

				Log.i(TAG, Log.CSB
					.Append("render model name=").Append(ctrl.name)
					.Append(", load from asset=").Append(ctrl.loadFromAsset));

				WVR_CtrlerCompInfoTable cit = ctrl.compInfos;

				int szStruct = Marshal.SizeOf(typeof(WVR_CtrlerCompInfo));

				Log.i(TAG, Log.CSB.Append("Controller component size=").Append(cit.size));

				curr.FBXInfo = new FBXInfo_t[cit.size];
				curr.sectionCount = cit.size;
				curr.SectionInfo = new MeshInfo_t[cit.size];
				curr.loadFromAsset = ctrl.loadFromAsset;

				for (int i = 0; i < cit.size; i++)
				{
					WVR_CtrlerCompInfo wcci;

					if (IntBits == 4)
						wcci = (WVR_CtrlerCompInfo)Marshal.PtrToStructure(new IntPtr(cit.table.ToInt32() + (szStruct * i)), typeof(WVR_CtrlerCompInfo));
					else
						wcci = (WVR_CtrlerCompInfo)Marshal.PtrToStructure(new IntPtr(cit.table.ToInt64() + (szStruct * i)), typeof(WVR_CtrlerCompInfo));

					curr.FBXInfo[i] = new FBXInfo_t();
					curr.SectionInfo[i] = new MeshInfo_t();

					curr.FBXInfo[i].meshName = Marshal.StringToHGlobalAnsi(wcci.name);
					curr.SectionInfo[i]._active = wcci.defaultDraw;

					// local matrix
					Matrix4x4 lt = TransformConverter.RigidTransform.toMatrix44(wcci.localMat, false);
					Matrix4x4 t = TransformConverter.RigidTransform.RowColumnInverse(lt);
					Log.i(TAG, Log.CSB
						.Append("Controller component name=").Append(wcci.name)
						.Append(", tex index=").Append(wcci.texIndex)
						.Append(", active=").Append(curr.SectionInfo[i]._active)
						.AppendMatrix(", CtrlMatrix", t));

					curr.FBXInfo[i].matrix = TransformConverter.RigidTransform.ToWVRMatrix(t, false);

					WVR_VertexBuffer vertices = wcci.vertices;

					if (vertices.dimension == 3)
					{
						uint verticesCount = (vertices.size / vertices.dimension);

						Log.i(TAG, Log.CSB.Append(" vertices size=").Append(vertices.size).Append(", dimension=").Append(vertices.dimension).Append(", count=").Append(verticesCount));

						curr.SectionInfo[i]._vectice = new Vector3[verticesCount];
						float[] verticeArray = new float[vertices.size];

						Marshal.Copy(vertices.buffer, verticeArray, 0, verticeArray.Length);

						int verticeIndex = 0;
						int floatIndex = 0;

						while (verticeIndex < verticesCount)
						{
							curr.SectionInfo[i]._vectice[verticeIndex] = new Vector3();
							curr.SectionInfo[i]._vectice[verticeIndex].x = verticeArray[floatIndex++];
							curr.SectionInfo[i]._vectice[verticeIndex].y = verticeArray[floatIndex++];
							curr.SectionInfo[i]._vectice[verticeIndex].z = verticeArray[floatIndex++] * -1.0f;

							verticeIndex++;
						}
					}
					else
					{
						Log.w(TAG, "vertices buffer's dimension incorrect!");
					}

					// normals
					WVR_VertexBuffer normals = wcci.normals;

					if (normals.dimension == 3)
					{
						uint normalsCount = (normals.size / normals.dimension);
						Log.i(TAG, Log.CSB.Append(" normals size=").Append(normals.size).Append(", dimension=").Append(normals.dimension).Append(", count=").Append(normalsCount));
						curr.SectionInfo[i]._normal = new Vector3[normalsCount];
						float[] normalArray = new float[normals.size];

						Marshal.Copy(normals.buffer, normalArray, 0, normalArray.Length);

						int normalsIndex = 0;
						int floatIndex = 0;

						while (normalsIndex < normalsCount)
						{
							curr.SectionInfo[i]._normal[normalsIndex] = new Vector3();
							curr.SectionInfo[i]._normal[normalsIndex].x = normalArray[floatIndex++];
							curr.SectionInfo[i]._normal[normalsIndex].y = normalArray[floatIndex++];
							curr.SectionInfo[i]._normal[normalsIndex].z = normalArray[floatIndex++] * -1.0f;

							normalsIndex++;
						}

						Log.i(TAG, Log.CSB.Append(" normals size=").Append(normals.size).Append(", dimension=").Append(normals.dimension).Append(", count=").Append(normalsCount));
					}
					else
					{
						Log.w(TAG, "normals buffer's dimension incorrect!");
					}

					// texCoord
					WVR_VertexBuffer texCoord = wcci.texCoords;

					if (texCoord.dimension == 2)
					{
						uint uvCount = (texCoord.size / texCoord.dimension);
						Log.i(TAG, Log.CSB.Append(" texCoord size=").Append(texCoord.size).Append(", dimension=").Append(texCoord.dimension).Append(", count=").Append(uvCount));
						curr.SectionInfo[i]._uv = new Vector2[uvCount];
						float[] texCoordArray = new float[texCoord.size];

						Marshal.Copy(texCoord.buffer, texCoordArray, 0, texCoordArray.Length);

						int uvIndex = 0;
						int floatIndex = 0;

						while (uvIndex < uvCount)
						{
							curr.SectionInfo[i]._uv[uvIndex] = new Vector2();
							curr.SectionInfo[i]._uv[uvIndex].x = texCoordArray[floatIndex++];
							curr.SectionInfo[i]._uv[uvIndex].y = texCoordArray[floatIndex++];

							uvIndex++;
						}
					}
					else
					{
						Log.w(TAG, "normals buffer's dimension incorrect!");
					}

					// indices
					WVR_IndexBuffer indices = wcci.indices;
					Log.i(TAG, Log.CSB.Append(" indices size=").Append(indices.size));

					curr.SectionInfo[i]._indice = new int[indices.size];
					Marshal.Copy(indices.buffer, curr.SectionInfo[i]._indice, 0, curr.SectionInfo[i]._indice.Length);

					uint indiceIndex = 0;

					while (indiceIndex < indices.size)
					{
						int tmp = curr.SectionInfo[i]._indice[indiceIndex];
						curr.SectionInfo[i]._indice[indiceIndex] = curr.SectionInfo[i]._indice[indiceIndex + 2];
						curr.SectionInfo[i]._indice[indiceIndex + 2] = tmp;
						indiceIndex += 3;
					}
				}

				// Controller texture section
				WVR_CtrlerTexBitmapTable wctbt = ctrl.bitmapInfos;
				Log.i(TAG, Log.CSB.Append("Controller textures=").Append(wctbt.size));
				int bmStruct = Marshal.SizeOf(typeof(WVR_CtrlerTexBitmap));
				curr.modelTextureCount = (int)wctbt.size;
				curr.modelTextureInfo = new TextureInfo[wctbt.size];
				curr.modelTexture = new Texture2D[wctbt.size];

				for (int mt = 0; mt < wctbt.size; mt++)
				{
					TextureInfo ct = new TextureInfo();

					WVR_CtrlerTexBitmap wctb;

					if (IntBits == 4)
						wctb = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wctbt.table.ToInt32() + (bmStruct * mt)), typeof(WVR_CtrlerTexBitmap));
					else
						wctb = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wctbt.table.ToInt64() + (bmStruct * mt)), typeof(WVR_CtrlerTexBitmap));

					Log.i(TAG, Log.CSB
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

					curr.modelTextureInfo[mt] = ct;
				}

				// Touchpad section
				Log.d(TAG, "---  Get touch info from runtime  ---");
				WVR_TouchPadPlane wtpp = ctrl.touchpadPlane;

				curr.TouchSetting = new TouchSetting();
				curr.TouchSetting.touchCenter.x = wtpp.center.v0 * 100f;
				curr.TouchSetting.touchCenter.y = wtpp.center.v1 * 100f;
				curr.TouchSetting.touchCenter.z = (-1.0f * wtpp.center.v2) * 100f;

				curr.TouchSetting.raidus = wtpp.radius * 100;

				curr.TouchSetting.touchptHeight = wtpp.floatingDistance * 100;

				curr.isTouchSetting = wtpp.valid;

				curr.TouchSetting.touchPtU.x = wtpp.u.v0;
				curr.TouchSetting.touchPtU.y = wtpp.u.v1;
				curr.TouchSetting.touchPtU.z = wtpp.u.v2;

				curr.TouchSetting.touchPtV.x = wtpp.v.v0;
				curr.TouchSetting.touchPtV.y = wtpp.v.v1;
				curr.TouchSetting.touchPtV.z = wtpp.v.v2;

				curr.TouchSetting.touchPtW.x = wtpp.w.v0;
				curr.TouchSetting.touchPtW.y = wtpp.w.v1;
				curr.TouchSetting.touchPtW.z = -1.0f * wtpp.w.v2;
				Log.i(TAG, Log.CSB
					.AppendVector3(" touchCenter!", curr.TouchSetting.touchCenter).AppendLine()
					.Append(" Floating distance=").Append(curr.TouchSetting.touchptHeight).AppendLine()
					.AppendVector3(" touchPtW!", curr.TouchSetting.touchPtW).AppendLine()
					.AppendVector3(" touchPtU!", curr.TouchSetting.touchPtU).AppendLine()
					.AppendVector3(" touchPtV!", curr.TouchSetting.touchPtV).AppendLine()
					.Append(" raidus=").Append(curr.TouchSetting.raidus).AppendLine()
					.Append(" isTouchSetting=").Append(curr.isTouchSetting));

				// Battery section
				Log.d(TAG, "---  Get battery info from runtime  ---");
				WVR_BatteryLevelTable wblt = ctrl.batteryLevels;

				List<BatteryIndicator> batteryTextureList = new List<BatteryIndicator>();
				curr.batteryTextureList = batteryTextureList;

				Log.i(TAG, Log.CSB.Append("Battery levels=").Append(wblt.size));

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
					Log.i(TAG, Log.CSB
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
				curr.isBatterySetting = true;

				Log.i(TAG, Log.CSB.Append("WVR_ReleaseControllerModel, ctrlModel IntPtr=").Append(ctrlModel.ToInt64()));

				Log.i(TAG, "Call WVR_ReleaseControllerModel");
				Interop.WVR_ReleaseControllerModel(ref ctrlModel);
			}


			curr.parserReady = true;
			Log.i(TAG, "Call WVR_GetCurrentControllerModel end");
		}


		public bool addRenderModel(string renderModel, XR_Hand hand, bool merge)
		{
			ModelResource newMR = getRenderModelResource(renderModel, hand, merge);
			if (newMR != null)
			{
				if (newMR.parserReady)
				{
					Log.i(TAG, Log.CSB.Append(hand).Append(" ").Append(renderModel).Append(" is already added.  Skip it."));
					return false;
				}
				Log.i(TAG, Log.CSB.Append(hand).Append(" ").Append(renderModel).Append(" is not finish loading.  Resume it."));
			}
			else
			{
				newMR = new ModelResource();
			}
			newMR.renderModelName = renderModel;
			newMR.mergeToOne = merge;
			newMR.parserReady = false;
			newMR.hand = hand;
			renderModelList.Add(newMR);

			Log.i(TAG, "Initial a thread to load current controller model assets");
			mthread = new Thread(() => getNativeControllerModel(hand, newMR, merge));
			mthread.Name = "srpResourceHolder";
			mthread.Start();

			return true;
		}

		public void Release()
		{
			Log.i(TAG, "Release");
			renderModelList.Clear();
			instance = null;
		}
	}
}
