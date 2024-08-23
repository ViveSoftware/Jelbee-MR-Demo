#if UNITY_EDITOR
#define USE_FAKE_DATA
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.ScenePerception
{

	internal class ScenePerceptionInternal
	{
		private const string TAG = "ScenePerceptionManager";
		internal ScenePerceptionManager manager;
		internal bool useFakeData = false;

		internal ScenePerceptionManager.ScenePerceptionTestObject fake2DPlane;
		internal ScenePerceptionManager.ScenePerceptionTestObject fake3DObject;
		internal ScenePerceptionManager.ScenePerceptionTestObject fakeSceneMesh;

		WVR_Result GetScenePlanesFakedata(out WVR_ScenePlane[] planes)
		{
			WVR_ScenePlane obj = new WVR_ScenePlane()
			{
				uuid = new WVR_Uuid() { data = new byte[16] },
				parentUuid = new WVR_Uuid() { data = new byte[16] },
				meshBufferId = (ulong)WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane + 1,
				pose = new WVR_Pose_t() { position = new WVR_Vector3f_t { v0 = 0, v1 = 1, v2 = -1 }, rotation = new WVR_Quatf_t { x = 0, y = 0, z = 0, w = -1 } },
				extent = new WVR_Extent2Df() { width = 0.1f, height = 0.1f },
				semanticName = new WVR_SemanticLabelName() { name = "Test Plane".ToCharArray() },
			};

			obj.pose.position = UnityToWVRConversionHelper.GetWVRVector(fake2DPlane.position);
			obj.pose.rotation = UnityToWVRConversionHelper.GetWVRQuaternion(fake2DPlane.rotation);
			obj.extent = UnityToWVRConversionHelper.GetWVRExtent2D(fake2DPlane.mesh.bounds);

			obj.extent.width *= fake2DPlane.scale.x;
			obj.extent.height *= fake2DPlane.scale.y;

			for (int i = 0; i < obj.uuid.data.Length; i++)
			{
				obj.uuid.data[i] = (byte)(i + WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane);
				obj.parentUuid.data[i] = (byte)(i + WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane);
			}

			planes = new WVR_ScenePlane[] { obj };
			return WVR_Result.WVR_Success;
		}

		public WVR_Result GetScenePlanes(WVR_PoseOriginModel originModel, out WVR_ScenePlane[] planes, WVR_ScenePlaneType planeType = WVR_ScenePlaneType.WVR_ScenePlaneType_Max, WVR_ScenePlaneLabel planeLabel = WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Max)
		{
#if USE_FAKE_DATA
			if ((Application.isEditor || useFakeData) && fake2DPlane != null)
				return GetScenePlanesFakedata(out planes);
#endif

			uint planeCount = 0;
			WVR_ScenePlaneFilter[] scenePlaneFilterArray = null;
			if (planeType != WVR_ScenePlaneType.WVR_ScenePlaneType_Max ||
				planeLabel != WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Max)
				scenePlaneFilterArray = new WVR_ScenePlaneFilter[] { new WVR_ScenePlaneFilter() { planeType = planeType, planeLabel = planeLabel } }; planes = new WVR_ScenePlane[0]; //Empty array

			WVR_Result result = Interop.WVR_GetScenePlanes(scenePlaneFilterArray, 0, out planeCount, originModel, IntPtr.Zero); //Get plane count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetScenePlanes 1 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetScenePlanes 1 Plane Count Output: " + planeCount);
			}

			if (planeCount <= 0) return result; //No need to further get planes if there are no planes.
			planes = new WVR_ScenePlane[planeCount];

			WVR_ScenePlane defaultPlane = default(WVR_ScenePlane);
			WVR_ScenePlane[] outPlanes = new WVR_ScenePlane[planes.Length];
			IntPtr planesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(defaultPlane) * outPlanes.Length);

			long offset = 0;
			if (IntPtr.Size == 4)
				offset = planesPtr.ToInt32();
			else
				offset = planesPtr.ToInt64();

			for (int i = 0; i < outPlanes.Length; i++)
			{
				IntPtr planePtr = new IntPtr(offset);

				Marshal.StructureToPtr(outPlanes[i], planePtr, false);

				offset += Marshal.SizeOf(defaultPlane);
			}

			result = Interop.WVR_GetScenePlanes(scenePlaneFilterArray, planeCount, out planeCount, originModel, planesPtr); //Get planes
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetScenePlanes 2 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetScenePlanes 2 successful.");
			}

			if (IntPtr.Size == 4)
				offset = planesPtr.ToInt32();
			else
				offset = planesPtr.ToInt64();

			for (int i = 0; i < outPlanes.Length; i++)
			{
				IntPtr planePtr = new IntPtr(offset);

				outPlanes[i] = (WVR_ScenePlane)Marshal.PtrToStructure(planePtr, typeof(WVR_ScenePlane));

				offset += Marshal.SizeOf(defaultPlane);
			}

			planes = outPlanes;

			Marshal.FreeHGlobal(planesPtr);

			return result;
		}

		WVR_Result GetSceneObjectsFakeData(out WVR_SceneObject[] objects)
		{
			var fakeObj = fake3DObject;
			WVR_SceneObject obj = new WVR_SceneObject()
			{
				uuid = new WVR_Uuid() { data = new byte[16] },
				parentUuid = new WVR_Uuid() { data = new byte[16] },
				meshBufferId = (ulong)WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject + 1,
				pose = new WVR_Pose_t() { position = new WVR_Vector3f_t { v0 = 0, v1 = 1, v2 = -1 }, rotation = new WVR_Quatf_t { x = 0, y = 0, z = 0, w = -1 } },
				extent = new WVR_Extent3Df() { width = 0.1f, depth = 0.1f, height = 0.1f },
				semanticName = new WVR_SemanticLabelName() { name = "Test Object".ToCharArray() },
			};

			obj.pose.position = UnityToWVRConversionHelper.GetWVRVector(fakeObj.position);
			obj.pose.rotation = UnityToWVRConversionHelper.GetWVRQuaternion(fakeObj.rotation);
			obj.extent = UnityToWVRConversionHelper.GetWVRExtent3D(fakeObj.mesh.bounds);

			obj.extent.width *= fakeObj.scale.x;
			obj.extent.height *= fakeObj.scale.y;
			obj.extent.depth *= fakeObj.scale.z;

			for (int i = 0; i < obj.uuid.data.Length; i++)
			{
				obj.uuid.data[i] = (byte)(i + WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject);
				obj.parentUuid.data[i] = (byte)(i + WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject);
			}

			objects = new WVR_SceneObject[] { obj };
			return WVR_Result.WVR_Success;

		}

		public WVR_Result GetSceneObjects(WVR_PoseOriginModel originModel, out WVR_SceneObject[] objects)
		{
#if USE_FAKE_DATA
			if ((Application.isEditor || useFakeData) && fake3DObject != null)
				return GetSceneObjectsFakeData(out objects);
#endif

			UInt32 count = 0;
			WVR_Result result = Interop.WVR_GetSceneObjects(0, out count, originModel, IntPtr.Zero); //Get objects count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetSceneObjects1 failed with result " + result.ToString());
				objects = new WVR_SceneObject[0];
				return result;
			}
			else
			{
				//Log.d(TAG, "WVR_GetSceneObjects1 Object count output: " + count);
			}

			if (count <= 0)
			{
				objects = new WVR_SceneObject[0];
				return result; //No need to further get objects if there are no objects.
			}

			WVR_SceneObject defaultObject = default(WVR_SceneObject);
			WVR_SceneObject[] outObjects = new WVR_SceneObject[count];
			IntPtr objectsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(defaultObject) * outObjects.Length);

			long offset = 0;
			if (IntPtr.Size == 4)
				offset = objectsPtr.ToInt32();
			else
				offset = objectsPtr.ToInt64();

			for (int i = 0; i < outObjects.Length; i++)
			{
				IntPtr objectPtr = new IntPtr(offset);

				Marshal.StructureToPtr(outObjects[i], objectPtr, false);

				offset += Marshal.SizeOf(defaultObject);
			}

			result = Interop.WVR_GetSceneObjects(count, out count, originModel, objectsPtr); //Get objects
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetSceneObjects2 failed with result " + result.ToString());
				objects = new WVR_SceneObject[0];
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetSceneObjects 2 successful.");
			}

			if (IntPtr.Size == 4)
				offset = objectsPtr.ToInt32();
			else
				offset = objectsPtr.ToInt64();

			for (int i = 0; i < outObjects.Length; i++)
			{
				IntPtr objectPtr = new IntPtr(offset);

				outObjects[i] = (WVR_SceneObject)Marshal.PtrToStructure(objectPtr, typeof(WVR_SceneObject));

				offset += Marshal.SizeOf(defaultObject);
			}

			objects = outObjects;

			Marshal.FreeHGlobal(objectsPtr);

			return result;
		}

		private WVR_Result GetSceneMeshesFakeData(WVR_SceneMeshType meshType, out WVR_SceneMesh[] meshes)
		{
			WVR_SceneMesh obj = new WVR_SceneMesh();
			obj.meshBufferId = (ulong)WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh + 1;
			meshes = new WVR_SceneMesh[] { obj };
			return WVR_Result.WVR_Success;
		}

		public WVR_Result GetSceneMeshes(WVR_SceneMeshType meshType, out WVR_SceneMesh[] meshes)
		{
#if USE_FAKE_DATA
			if ((Application.isEditor || useFakeData) && fakeSceneMesh != null)
				return GetSceneMeshesFakeData(meshType, out meshes);
#endif

			UInt32 meshCount = 0;
			meshes = new WVR_SceneMesh[0]; //Empty array

			WVR_Result result = Interop.WVR_GetSceneMeshes(meshType, 0, out meshCount, IntPtr.Zero); //Get mesh count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetSceneMeshes 1 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetSceneMeshes 1 Mesh Count Output: " + meshCount);
			}

			if (meshCount <= 0) return result; //No need to further get meshes if there are no meshes.
			meshes = new WVR_SceneMesh[meshCount];

			WVR_SceneMesh defaultMesh = default(WVR_SceneMesh);
			WVR_SceneMesh[] outMeshes = new WVR_SceneMesh[meshes.Length];

			IntPtr meshesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(defaultMesh) * outMeshes.Length);

			long offset = 0;
			if (IntPtr.Size == 4)
				offset = meshesPtr.ToInt32();
			else
				offset = meshesPtr.ToInt64();

			for (int i = 0; i < outMeshes.Length; i++)
			{
				IntPtr meshPtr = new IntPtr(offset);
				Marshal.StructureToPtr(outMeshes[i], meshPtr, false);
				offset += Marshal.SizeOf(defaultMesh);
			}

			result = Interop.WVR_GetSceneMeshes(meshType, meshCount, out meshCount, meshesPtr); //Get meshes
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetSceneMeshes 2 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetSceneMeshes 2 successful.");
			}

			if (IntPtr.Size == 4)
				offset = meshesPtr.ToInt32();
			else
				offset = meshesPtr.ToInt64();

			for (int i = 0; i < outMeshes.Length; i++)
			{
				IntPtr meshPtr = new IntPtr(offset);
				outMeshes[i] = (WVR_SceneMesh)Marshal.PtrToStructure(meshPtr, typeof(WVR_SceneMesh));
				offset += Marshal.SizeOf(defaultMesh);
			}

			meshes = outMeshes;

			Marshal.FreeHGlobal(meshesPtr);

			return result;
		}

		WVR_Result GetSceneMeshBufferFakeData(UInt64 meshBufferId, out WVR_Vector3f_t[] vertexBuffer, out UInt32[] indexBuffer)
		{
            ScenePerceptionManager.ScenePerceptionTestObject fakeObj = null;
			bool shiftCenter = false;
			switch ((WVR_ScenePerceptionTarget)meshBufferId - 1)
			{
				case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane:
					shiftCenter = true;
					fakeObj = fake2DPlane ?? null;
					break;
				case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject:
					shiftCenter = true;
					fakeObj = fake3DObject ?? null;
					break;
				case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh:
					fakeObj = fakeSceneMesh ?? null;
					break;
			}
			if (fakeObj == null || fakeObj.mesh == null || !fakeObj.mesh.isReadable)
			{
				vertexBuffer = null;
				indexBuffer = null;
				return WVR_Result.WVR_Error_Data_Invalid;
			}

			var mesh = fakeObj.mesh;
			var scale = fakeObj.scale;
			vertexBuffer = new WVR_Vector3f_t[mesh.vertexCount];
			indexBuffer = new UInt32[mesh.triangles.Length];

			var center = shiftCenter ? fakeObj.mesh.bounds.center : Vector3.zero;
			var vertices = mesh.vertices;
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				// According to wave's object define, center is the pivot.
				// Move vertex's pivot to the center of the bounds
				var vertex = vertices[i] - center;
				vertex = Vector3.Scale(vertex, scale);
				vertexBuffer[i] = UnityToWVRConversionHelper.GetWVRVector(vertex);
			}

			var triangles = mesh.triangles;
			var tCount = mesh.triangles.Length / 3;
			for (int i = 0; i < tCount; i++)
			{
				int l = i * 3;
				int m = i * 3 + 1;
				int n = i * 3 + 2;
				// Reverse the order of the triangle to simulate GL convention
				indexBuffer[l] = (UInt32)triangles[l];  // 0
				indexBuffer[m] = (UInt32)triangles[n];  // 2
				indexBuffer[n] = (UInt32)triangles[m];  // 1
			}

			return WVR_Result.WVR_Success;
		}


		public WVR_Result GetSceneMeshBuffer(UInt64 meshBufferId, out WVR_Vector3f_t[] vertexBuffer, out UInt32[] indexBuffer)
		{
#if USE_FAKE_DATA
			if ((Application.isEditor || useFakeData))
				return GetSceneMeshBufferFakeData(meshBufferId, out vertexBuffer, out indexBuffer);
#endif

			vertexBuffer = new WVR_Vector3f_t[0];
			indexBuffer = new UInt32[0];

			WVR_SceneMeshBuffer currentBuffer = new WVR_SceneMeshBuffer();

			currentBuffer.vertexCapacityInput = 0;
			currentBuffer.vertexCountOutput = 0;
			currentBuffer.vertexBuffer = IntPtr.Zero;
			currentBuffer.indexCapacityInput = 0;
			currentBuffer.indexCountOutput = 0;
			currentBuffer.indexBuffer = IntPtr.Zero;

			WVR_Result result = Interop.WVR_GetSceneMeshBuffer(meshBufferId, ScenePerceptionManager.GetCurrentPoseOriginModel(), ref currentBuffer); //Get vertex and index count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetSceneMeshBuffer 1 failed with result " + result.ToString());
				return result;
			}
			else
			{
				currentBuffer.vertexCapacityInput = currentBuffer.vertexCountOutput;
				currentBuffer.indexCapacityInput = currentBuffer.indexCountOutput;

				//Log.d(LOG_TAG, "WVR_GetSceneMeshBuffer 1 Vertex Count Output: " + currentBuffer.vertexCapacityInput + ", Index Count Output: " + currentBuffer.indexCapacityInput);
			}

			WVR_Vector3f_t[] vertexBufferArray = new WVR_Vector3f_t[currentBuffer.vertexCapacityInput];
			UInt32[] indexBufferArray = new UInt32[currentBuffer.indexCapacityInput];

			WVR_Vector3f_t defaultWVRVector3f = default(WVR_Vector3f_t);
			currentBuffer.vertexBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(defaultWVRVector3f) * (int)currentBuffer.vertexCapacityInput);
			currentBuffer.indexBuffer = Marshal.AllocHGlobal(sizeof(UInt32) * (int)currentBuffer.indexCapacityInput);

			result = Interop.WVR_GetSceneMeshBuffer(meshBufferId, ScenePerceptionManager.GetCurrentPoseOriginModel(), ref currentBuffer); //Get buffers
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetSceneMeshBuffer 2 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Fill vertex buffer
				int offset = 0;
				for (int i = 0; i < currentBuffer.vertexCapacityInput; i++)
				{
					if (IntPtr.Size == 4)
						vertexBufferArray[i] = (WVR_Vector3f_t)Marshal.PtrToStructure(new IntPtr(currentBuffer.vertexBuffer.ToInt32() + offset), typeof(WVR_Vector3f_t));
					else
						vertexBufferArray[i] = (WVR_Vector3f_t)Marshal.PtrToStructure(new IntPtr(currentBuffer.vertexBuffer.ToInt64() + offset), typeof(WVR_Vector3f_t));

					offset += Marshal.SizeOf(defaultWVRVector3f);
				}

				//Fill index buffer
				offset = 0;
				for (int i = 0; i < currentBuffer.indexCapacityInput; i++)
				{
					if (IntPtr.Size == 4)
						indexBufferArray[i] = (UInt32)Marshal.PtrToStructure(new IntPtr(currentBuffer.indexBuffer.ToInt32() + offset), typeof(UInt32));
					else
						indexBufferArray[i] = (UInt32)Marshal.PtrToStructure(new IntPtr(currentBuffer.indexBuffer.ToInt64() + offset), typeof(UInt32));

					offset += sizeof(UInt32);
				}

				//Log.d(LOG_TAG, "WVR_GetSceneMeshBuffer 2 successful.");
			}

			vertexBuffer = vertexBufferArray;
			indexBuffer = indexBufferArray;

			Marshal.FreeHGlobal(currentBuffer.vertexBuffer);
			Marshal.FreeHGlobal(currentBuffer.indexBuffer);

			return result;
		}

		public WVR_Result CreateSpatialAnchor(WVR_SpatialAnchorName anchorName, WVR_Vector3f_t anchorPosition, WVR_Quatf_t anchorRotation, WVR_PoseOriginModel originModel, out ulong anchorHandle)
		{
			WVR_SpatialAnchorCreateInfo spatialAnchorCreateInfo = new WVR_SpatialAnchorCreateInfo();
			spatialAnchorCreateInfo.anchorName = anchorName;
			spatialAnchorCreateInfo.originModel = originModel;
			spatialAnchorCreateInfo.pose.position = anchorPosition;
			spatialAnchorCreateInfo.pose.rotation = anchorRotation;

			WVR_SpatialAnchorCreateInfo[] spatialAnchorCreateInfoArray = { spatialAnchorCreateInfo };
			WVR_Result result;
			result = Interop.WVR_CreateSpatialAnchor(spatialAnchorCreateInfoArray, out anchorHandle);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_CreateSpatialAnchor failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_CreateSpatialAnchor Anchor handle: " + anchorHandle);
			}

			return result;
		}

		public WVR_Result GetSpatialAnchorState(UInt64 anchorHandle, WVR_PoseOriginModel originModel, out WVR_SpatialAnchorState anchorState)
		{
			anchorState = default;
			anchorState.anchorName = default;
			anchorState.anchorName.name = new char[256];
			WVR_Result result;
			result = Interop.WVR_GetSpatialAnchorState(anchorHandle, originModel, out anchorState);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetSpatialAnchorState failed with result " + result.ToString());
				return result;
			}

			return result;
		}
	}
}
