using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using Wave.Native;
using Wave.XR;

namespace Wave.Essence.ScenePerception
{
	public class ScenePerceptionManager : MonoBehaviour
	{
		[SerializeField]
		public GameObject trackingOrigin = null;

		private const string LOG_TAG = "ScenePerceptionManager";

		/// <summary>
		/// Start the Scene Perception feature.
		/// </summary>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if Scene Perception is started successfully.
		/// </returns>
		public WVR_Result StartScene()
		{
			WVR_Result result = Interop.WVR_StartScene();
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_StartScene failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_StartScene successful.");
			}

			return result;
		}

		/// <summary>
		/// Stop the Scene Perception feature.
		/// </summary>
		public void StopScene()
		{
			//Log.d(LOG_TAG, "WVR_StopScene");
			Interop.WVR_StopScene();
		}

		public delegate void StartScenePerceptionDelegate(object sender, WVR_ScenePerceptionTarget perceptionTarget);
		public event StartScenePerceptionDelegate OnStartScenePerception;

		#region Scene Mesh

		/// <summary>
		/// Start perceiving scene perception targets of a specific type.
		/// Should be called after <see cref="StartScene()"/> is called successfully. 
		/// See <see cref="WVR_ScenePerceptionTarget"/> for the supported types of perception targets.
		/// </summary>
		/// <param name="perceptionTarget">
		/// The type of scene perception target to be perceived.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the perception process is started successfully.
		/// </returns>
		public WVR_Result StartScenePerception(WVR_ScenePerceptionTarget perceptionTarget)
		{
			WVR_Result result = Interop.WVR_StartScenePerception(perceptionTarget);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_StartScenePerception failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_StartScenePerception with target type " + perceptionTarget.ToString() + " started successfully.");
				OnStartScenePerception?.Invoke(this, perceptionTarget);
			}

			return result;
		}

		public delegate void StopScenePerceptionDelegate(object sender, WVR_ScenePerceptionTarget perceptionTarget);
		public event StopScenePerceptionDelegate OnStopScenePerception;

		/// <summary>
		/// Stop perceiving scene perception targets of a specific type. 
		/// See <see cref="WVR_ScenePerceptionTarget"/> for the supported types of perception targets.
		/// </summary>
		/// <returns>
		/// <param name="perceptionTarget">
		/// The type of scene perception target that should no longer be perceived.
		/// </param>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the perception process is stopped successfully.
		/// </returns>
		public WVR_Result StopScenePerception(WVR_ScenePerceptionTarget perceptionTarget)
		{
			WVR_Result result = Interop.WVR_StopScenePerception(perceptionTarget);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_StopScenePerception failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_StopScenePerception with target type " + perceptionTarget.ToString() + " stopped successfully.");
				OnStopScenePerception?.Invoke(this, perceptionTarget);
			}

			return result;
		}

		/// <summary>
		/// Get the current state of perception of a specific scene perception target type.
		/// </summary>
		/// <param name="perceptionTarget">
		/// The type of scene perception target currently perceived. 
		/// See <see cref="WVR_ScenePerceptionTarget"/> for the supported types of perception targets.
		/// </param>
		/// <param name="state">
		/// The state of perception of the perception target. See <see cref="WVR_ScenePerceptionState"/> for the types of states.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the state is retrieved successfully.
		/// </returns>
		public WVR_Result GetScenePerceptionState(WVR_ScenePerceptionTarget perceptionTarget, ref WVR_ScenePerceptionState state)
		{
			WVR_Result result = Interop.WVR_GetScenePerceptionState(perceptionTarget, ref state);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetScenePerceptionState failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetScenePerceptionState current state is:" + state.ToString());
			}

			return result;
		}

		/// <summary>
		/// Get all of the Scene Planes that are currently on the device.
		/// </summary>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the scene planes.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="planes">
		/// An array of <see cref="WVR_ScenePlane">Scene Planes</see> retrieved from the device.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the scene planes are retrieved successfully.
		/// </returns>
		public WVR_Result GetScenePlanes(WVR_PoseOriginModel originModel, out WVR_ScenePlane[] planes) //No filter
		{
			UInt32 planeCount = 0;
			planes = new WVR_ScenePlane[1]; //Empty array

			WVR_Result result = Interop.WVR_GetScenePlanes(null, 0, out planeCount, originModel, IntPtr.Zero); //Get plane count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetScenePlanes 1 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetScenePlanes 1 Plane Count Output: " + planeCount);
			}

			Array.Resize(ref planes, (int)planeCount);
			if (planeCount <= 0) return result; //No need to further get planes if there are no planes.

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

			result = Interop.WVR_GetScenePlanes(null, planeCount, out planeCount, originModel, planesPtr); //Get planes
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetScenePlanes 2 failed with result " + result.ToString());
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

		/// <summary>
		/// Get a filtered subset of the Scene Planes that are currently on the device.
		/// </summary>
		/// <param name="planeFilter">
		/// The filter to be applied.
		/// </param>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the scene planes.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="planes">
		/// An array of <see cref="WVR_ScenePlane">Scene Planes</see> retrieved from the device.
		/// Only planes that match the specified <see cref="WVR_ScenePlaneType"/> and <see cref="WVR_ScenePlaneLabel"/> in <paramref name="planeFilter"/> will be returned in this array.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the scene planes are retrieved successfully.
		/// </returns>
		public WVR_Result GetScenePlanes(WVR_ScenePlaneFilter planeFilter, WVR_PoseOriginModel originModel, out WVR_ScenePlane[] planes)
		{
			UInt32 planeCount = 0;
			WVR_ScenePlaneFilter[] scenePlaneFilterArray = { planeFilter };
			planes = new WVR_ScenePlane[1]; //Empty array

			WVR_Result result = Interop.WVR_GetScenePlanes(scenePlaneFilterArray, 0, out planeCount, originModel, IntPtr.Zero); //Get plane count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetScenePlanes 1 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetScenePlanes 1 Plane Count Output: " + planeCount);
			}

			Array.Resize(ref planes, (int)planeCount);
			if (planeCount <= 0) return result; //No need to further get planes if there are no planes.

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
				Log.e(LOG_TAG, "WVR_GetScenePlanes 2 failed with result " + result.ToString());
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

		/// <summary>
		/// Get a list of Scene Meshes of a specific type that are currently on the device.
		/// </summary>
		/// <param name="meshType">
		/// Type of scene mesh to be returned by the function.
		/// </param>
		/// <param name="meshes">
		/// /// An array of <see cref="WVR_SceneMesh">Scene Meshes</see> retrieved from the device.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the scene meshes are retrieved successfully.
		/// </returns>
		public WVR_Result GetSceneMeshes(WVR_SceneMeshType meshType, out WVR_SceneMesh[] meshes)
		{
			UInt32 meshCount = 0;
			meshes = new WVR_SceneMesh[1]; //Empty array

			WVR_Result result = Interop.WVR_GetSceneMeshes(meshType, 0, out meshCount, IntPtr.Zero); //Get mesh count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetSceneMeshes 1 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetSceneMeshes 1 Mesh Count Output: " + meshCount);
			}

			Array.Resize(ref meshes, (int)meshCount);
			if (meshCount <= 0) return result; //No need to further get meshes if there are no meshes.

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
				Log.e(LOG_TAG, "WVR_GetSceneMeshes 2 failed with result " + result.ToString());
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

		/// <summary>
		/// Get the vertices and indices of the mesh with its mesh buffer id.
		/// </summary>
		/// <param name="meshBufferId">
		/// The ID of the mesh to be retrieved.
		/// </param>
		/// <param name="vertexBuffer">
		/// An array of <see cref="WVR_Vector3f_t"/> which each represents a vertex of the mesh. Vertex coordinates are of OpenGL convention.
		/// </param>
		/// <param name="indexBuffer">
		/// An array of <see cref="UInt32"/> which represents the triangles of the mesh.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the scene mesh buffer is retrieved successfully.
		/// </returns>
		public WVR_Result GetSceneMeshBuffer(UInt64 meshBufferId, out WVR_Vector3f_t[] vertexBuffer, out UInt32[] indexBuffer)
		{
			vertexBuffer = new WVR_Vector3f_t[0];
			indexBuffer = new UInt32[0];

			WVR_SceneMeshBuffer currentBuffer = new WVR_SceneMeshBuffer();

			currentBuffer.vertexCapacityInput = 0;
			currentBuffer.vertexCountOutput = 0;
			currentBuffer.vertexBuffer = IntPtr.Zero;
			currentBuffer.indexCapacityInput = 0;
			currentBuffer.indexCountOutput = 0;
			currentBuffer.indexBuffer = IntPtr.Zero;

			WVR_Result result = Interop.WVR_GetSceneMeshBuffer(meshBufferId, GetCurrentPoseOriginModel(), ref currentBuffer); //Get vertex and index count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetSceneMeshBuffer 1 failed with result " + result.ToString());
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

			result = Interop.WVR_GetSceneMeshBuffer(meshBufferId, GetCurrentPoseOriginModel(), ref currentBuffer); //Get buffers
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetSceneMeshBuffer 2 failed with result " + result.ToString());
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

		/// <summary>
		/// A helper function to help create a GameObject with Mesh related components for showing scene planes in the scene.
		/// </summary>
		/// <param name="scenePlane">
		/// The scene plane to be rendered.
		/// </param>
		/// <param name="meshMaterial">
		/// The <see cref="Material"/> to be applied to the <see cref="MeshRenderer"/>.
		/// </param>
		/// <param name="attachMeshCollider">
		/// Specify whether or not a mesh collider should be attached to the generated mesh.
		/// Set to false by default.
		/// </param>
		/// <param name="applyTrackingOriginCorrection">
		/// Specify whether or not the <see cref="trackingOrigin">Tracking Origin reference</see> should be be used to convert the scene plane pose from tracking to world space.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="ScenePerceptionManager"/> instance in order for this parameter to work as intended.
		/// Set to true by default.
		/// </param>
		/// <returns>
		/// A GameObject with all the necessary Mesh related components for rendering a scene plane in the scene.
		/// </returns>
		public GameObject GenerateScenePlaneMesh(WVR_ScenePlane scenePlane, Material meshMaterial, bool attachMeshCollider = false, bool applyTrackingOriginCorrection = true) //use pose and extend to generate planes
		{
			WVR_Pose_t planePose = scenePlane.pose;
			WVR_Extent2Df planeDimensions = scenePlane.extent;

			Vector3 planePositionUnity = Vector3.zero;
			Quaternion planeRotationUnity = Quaternion.identity;

			if (applyTrackingOriginCorrection) //Apply origin correction to the anchor pose
			{
				ApplyTrackingOriginCorrectionToPlanePose(scenePlane, out planePositionUnity, out planeRotationUnity);
			}
			else
			{
				Coordinate.GetVectorFromGL(planePose.position, out planePositionUnity);
				Coordinate.GetQuaternionFromGL(planePose.rotation, out planeRotationUnity);

				planeRotationUnity *= Quaternion.Euler(0, 180f, 0);
			}

			//Log.d(LOG_TAG, "GenerateScenePlaneMesh Position: " + planePositionUnity.ToString());
			//Log.d(LOG_TAG, "GenerateScenePlaneMesh Rotation (Euler): " + planeRotationUnity.eulerAngles.ToString());

			Mesh planeMesh = MeshGenerationHelper.GenerateQuadMesh(MeshGenerationHelper.GenerateQuadVertex(planeDimensions));

			GameObject planeMeshGameObject = new GameObject();

			planeMeshGameObject.transform.position = planePositionUnity;
			planeMeshGameObject.transform.rotation = planeRotationUnity;

			MeshRenderer planeMeshRenderer = planeMeshGameObject.AddComponent<MeshRenderer>();
			MeshFilter planeMeshFilter = planeMeshGameObject.AddComponent<MeshFilter>();

			planeMeshFilter.mesh = planeMesh;
			planeMeshRenderer.material = meshMaterial;
			
			if (attachMeshCollider)
			{
				planeMeshGameObject.AddComponent<MeshCollider>();
			}

			return planeMeshGameObject;
		}

		/// <summary>
		///  A helper function to help create a GameObject with Mesh related components for showing scene meshes in the scene.
		/// </summary>
		/// <param name="sceneMesh">
		/// The scene mesh to be rendered.
		/// </param>
		/// <param name="meshMaterial">
		/// The <see cref="Material"/> to be applied to the <see cref="MeshRenderer"/>.
		/// </param>
		/// <param name="attachMeshCollider">
		/// Specify whether or not a mesh collider should be attached to the generated mesh.
		/// Set to false by default.
		/// </param>
		/// <returns>
		/// A GameObject with all the necessary Mesh related components for rendering a scene mesh in the scene.
		/// </returns>
		public GameObject GenerateSceneMesh(WVR_SceneMesh sceneMesh, Material meshMaterial, bool attachMeshCollider = false)
		{
			WVR_Vector3f_t[] sceneVertexBuffer;
			uint[] sceneIndexBuffer;

			if (GetSceneMeshBuffer(sceneMesh.meshBufferId, out sceneVertexBuffer, out sceneIndexBuffer) != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "GenerateSceneMesh: Failed to get scene mesh buffer.");
				return null;
			}

			Mesh generatedSceneMesh = MeshGenerationHelper.GenerateMesh(sceneVertexBuffer, sceneIndexBuffer);

			GameObject sceneMeshGameObject = new GameObject();
			MeshRenderer sceneMeshRenderer = sceneMeshGameObject.AddComponent<MeshRenderer>();
			MeshFilter sceneMeshFilter = sceneMeshGameObject.AddComponent<MeshFilter>();

			sceneMeshFilter.mesh = generatedSceneMesh;
			sceneMeshRenderer.material = meshMaterial;

			if (attachMeshCollider)
			{
				sceneMeshGameObject.AddComponent<MeshCollider>();
			}

			return sceneMeshGameObject;
		}

		/// <summary>
		/// A helper function for comparing two <see cref="WVR_Uuid"/>.
		/// </summary>
		/// <param name="uuid1">A <see cref="WVR_Uuid"/> of which will be in the comparison.</param>
		/// <param name="uuid2">A <see cref="WVR_Uuid"/> of which will be in the comparison.</param>
		/// <returns>
		/// true if the Uuids are the identical, false if they are not.
		/// </returns>
		public static bool IsUUIDEqual(WVR_Uuid uuid1, WVR_Uuid uuid2)
		{
			return WVRStructCompare.IsUUIDEqual(uuid1, uuid2);
		}

		/// <summary>
		/// A helper function which outputs the world space position and rotation of a <see cref="WVR_ScenePlane"/>.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="ScenePerceptionManager"/> instance in order for this function to work as intended.
		/// </summary>
		/// <param name="scenePlane">
		/// The target <see cref="WVR_ScenePlane"/> of which pose will be used in the conversion.
		/// </param>
		/// <param name="planePosition">
		/// The world space position of the scene plane.
		/// The tracking space position will be returned instead if the <see cref="trackingOrigin">Tracking Origin reference</see> is not assigned to the <see cref="ScenePerceptionManager"/> instance.
		/// </param>
		/// <param name="planeRotation">
		/// The world space rotation of the scene plane.
		/// The tracking space rotation will be returned instead if the <see cref="trackingOrigin">Tracking Origin reference</see> is not assigned to the <see cref="ScenePerceptionManager"/> instance.
		/// </param>
		public void ApplyTrackingOriginCorrectionToPlanePose(WVR_ScenePlane scenePlane, out Vector3 planePosition, out Quaternion planeRotation)
		{
			Coordinate.GetVectorFromGL(scenePlane.pose.position, out planePosition);
			Coordinate.GetQuaternionFromGL(scenePlane.pose.rotation, out planeRotation);

			planeRotation *= Quaternion.Euler(0, 180f, 0);

			if (trackingOrigin != null)
			{
				Matrix4x4 trackingSpaceOriginTRS = Matrix4x4.TRS(trackingOrigin.transform.position, trackingOrigin.transform.rotation, Vector3.one);

				Matrix4x4 worldSpacePlanePoseTRS = Matrix4x4.TRS(planePosition, planeRotation, Vector3.one);
				Matrix4x4 trackingSpacePlanePoseTRS = trackingSpaceOriginTRS * worldSpacePlanePoseTRS;

				planePosition = trackingSpacePlanePoseTRS.GetColumn(3); //4th Column of TRS Matrix is the position
				planeRotation = Quaternion.LookRotation(trackingSpacePlanePoseTRS.GetColumn(2), trackingSpacePlanePoseTRS.GetColumn(1));
			}
		}

		/// <summary>
		/// A helper function for comparing the poses of two <see cref="WVR_ScenePlane"/>.
		/// </summary>
		/// <param name="scenePlane1">A <see cref="WVR_ScenePlane"/> of which pose will be in the comparison.</param>
		/// <param name="scenePlane2">A <see cref="WVR_ScenePlane"/> of which pose will be in the comparison.</param>
		/// <returns>
		/// true if the poses of the planes are the identical, false if they are not.
		/// </returns>
		public static bool ScenePlanePoseEqual(WVR_ScenePlane scenePlane1, WVR_ScenePlane scenePlane2)
		{
			WVR_Pose_t scenePlane1Pose = scenePlane1.pose;
			WVR_Pose_t scenePlane2Pose = scenePlane2.pose;

			WVR_Vector3f_t scenePlane1PosePosition = scenePlane1Pose.position;
			WVR_Vector3f_t scenePlane2PosePosition = scenePlane2Pose.position;
			WVR_Quatf_t scenePlane1PoseRotation = scenePlane1Pose.rotation;
			WVR_Quatf_t scenePlane2PoseRotation = scenePlane2Pose.rotation;

			return (scenePlane1PosePosition.v0 == scenePlane2PosePosition.v0 &&
					scenePlane1PosePosition.v1 == scenePlane2PosePosition.v1 &&
					scenePlane1PosePosition.v2 == scenePlane2PosePosition.v2 &&
					scenePlane1PoseRotation.w == scenePlane2PoseRotation.w &&
					scenePlane1PoseRotation.x == scenePlane2PoseRotation.x &&
					scenePlane1PoseRotation.y == scenePlane2PoseRotation.y &&
					scenePlane1PoseRotation.z == scenePlane2PoseRotation.z);
		}

		/// <summary>
		/// A helper function for comparing the extends of two <see cref="WVR_ScenePlane"/>.
		/// </summary>
		/// <param name="scenePlane1">A <see cref="WVR_ScenePlane"/> of which extends will be in the comparison.</param>
		/// <param name="scenePlane2">A <see cref="WVR_ScenePlane"/> of which extends will be in the comparison.</param>
		/// <returns>
		/// true if the poses of the extends are the identical, false if they are not.
		/// </returns>
		public static bool ScenePlaneExtent2DEqual(WVR_ScenePlane scenePlane1, WVR_ScenePlane scenePlane2)
		{
			WVR_Extent2Df scenePlane1Extent2D = scenePlane1.extent;
			WVR_Extent2Df scenePlane2Extent2D = scenePlane2.extent;

			return (scenePlane1Extent2D.height == scenePlane2Extent2D.height &&
					scenePlane1Extent2D.width == scenePlane2Extent2D.width);
		}
		#endregion

		#region Spatial Anchor
		/// <summary>
		/// Create a Spatial Anchor.
		/// </summary>
		/// <param name="anchorName">
		/// The name of the anchor, must be within 256 characters including the null terminator.
		/// </param>
		/// <param name="anchorPosition">
		/// The position of the spatial anchor.
		/// </param>
		/// <param name="anchorRotation">
		/// The rotation as Euler angles in degrees of the spatial anchor.
		/// </param>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the anchor.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="anchorHandle">
		/// The handle of the created Anchor.
		/// </param>
		/// <param name="convertFromUnityToWVR">
		/// Specify whether or not the <paramref name="anchorPosition"/> and <paramref name="anchorRotation"/> should be coverted from Unity to WVR convention.
		/// Set to true by default.
		/// </param>
		/// <param name="applyTrackingOriginCorrection">
		/// Specify whether or not the <see cref="trackingOrigin">Tracking Origin reference</see> should be be used to convert <paramref name="anchorPosition"/> and <paramref name="anchorRotation"/> from world to tracking space.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="ScenePerceptionManager"/> instance in order for this parameter to work as intended.
		/// Set to true by default.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the anchor is created successfully.
		/// </returns>
		public WVR_Result CreateSpatialAnchor(char[] anchorName, Vector3 anchorPosition, Vector3 anchorRotation, WVR_PoseOriginModel originModel, out ulong anchorHandle, bool convertFromUnityToWVR = true, bool applyTrackingOriginCorrection = true)
		{
			anchorHandle = 0;

			if (anchorName.Length > 256)
			{
				Log.e(LOG_TAG, "CreateSpatialAnchor: anchor name should be under 256 characters.");
				return WVR_Result.WVR_Error_InvalidArgument;
			}

			WVR_SpatialAnchorName anchorNameWVR = default(WVR_SpatialAnchorName);
			anchorNameWVR.name = new char[256];
			anchorName.CopyTo(anchorNameWVR.name, 0);

			if (applyTrackingOriginCorrection && trackingOrigin != null) //Apply origin correction to the anchor pose (world to tracking space)
			{
				Matrix4x4 trackingSpaceOriginTRS = Matrix4x4.TRS(trackingOrigin.transform.position, trackingOrigin.transform.rotation, Vector3.one);
				Matrix4x4 worldSpaceAnchorPoseTRS = Matrix4x4.TRS(anchorPosition, Quaternion.Euler(anchorRotation), Vector3.one);

				Matrix4x4 trackingSpaceAnchorPoseTRS = trackingSpaceOriginTRS.inverse * worldSpaceAnchorPoseTRS;

				anchorPosition = trackingSpaceAnchorPoseTRS.GetColumn(3); //4th Column of TRS Matrix is the position
				anchorRotation = Quaternion.LookRotation(trackingSpaceAnchorPoseTRS.GetColumn(2), trackingSpaceAnchorPoseTRS.GetColumn(1)).eulerAngles;
			}

			WVR_Pose_t anchorPoseWVR = default(WVR_Pose_t);

			if (convertFromUnityToWVR)
			{
				anchorPoseWVR.position = UnityToWVRConversionHelper.GetWVRVector(anchorPosition);
				anchorPoseWVR.rotation = UnityToWVRConversionHelper.GetWVRQuaternion(Quaternion.Euler(anchorRotation.x, anchorRotation.y, anchorRotation.z));
			}
			else
			{
				anchorPoseWVR.position = UnityToWVRConversionHelper.GetWVRVector_NoConversion(anchorPosition);
				anchorPoseWVR.rotation = UnityToWVRConversionHelper.GetWVRQuaternion_NoConversion(Quaternion.Euler(anchorRotation.x, anchorRotation.y, anchorRotation.z));
			}

			WVR_SpatialAnchorCreateInfo spatialAnchorCreateInfo = default(WVR_SpatialAnchorCreateInfo);
			spatialAnchorCreateInfo.anchorName = anchorNameWVR;
			spatialAnchorCreateInfo.originModel = originModel;
			spatialAnchorCreateInfo.pose = anchorPoseWVR;

			WVR_SpatialAnchorCreateInfo[] spatialAnchorCreateInfoArray = { spatialAnchorCreateInfo };
			WVR_Result result = Interop.WVR_CreateSpatialAnchor(spatialAnchorCreateInfoArray, out anchorHandle);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_CreateSpatialAnchor failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_CreateSpatialAnchor Anchor handle: " + anchorHandle);
			}

			return result;
		}

		/// <summary>
		/// Create a Spatial Anchor.
		/// </summary>
		/// <param name="anchorName">
		/// The name of the anchor, must be within 256 characters including the null terminator.
		/// </param>
		/// <param name="anchorPosition">
		/// The position of the spatial anchor.
		/// </param>
		/// <param name="anchorRotation">
		/// The rotation as quaternion of the spatial anchor.
		/// </param>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the anchor.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="anchorHandle">
		/// The handle of the created Anchor.
		/// </param>
		/// <param name="convertFromUnityToWVR">
		/// Specify whether or not the <paramref name="anchorPosition"/> and <paramref name="anchorRotation"/> should be coverted from Unity to WVR convention.
		/// Set to true by default.
		/// </param>
		/// <param name="applyTrackingOriginCorrection">
		/// Specify whether or not the <see cref="trackingOrigin">Tracking Origin reference</see> should be be used to convert <paramref name="anchorPosition"/> and <paramref name="anchorRotation"/> from world to tracking space.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="ScenePerceptionManager"/> instance in order for this parameter to work as intended.
		/// Set to true by default.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the anchor is created successfully.
		/// </returns>
		public WVR_Result CreateSpatialAnchor(char[] anchorName, Vector3 anchorPosition, Quaternion anchorRotation, WVR_PoseOriginModel originModel, out ulong anchorHandle, bool convertFromUnityToWVR = true, bool applyTrackingOriginCorrection = true)
		{
			anchorHandle = 0;

			if (anchorName.Length > 256)
			{
				Log.e(LOG_TAG, "CreateSpatialAnchor: anchor name should be under 256 characters.");
				return WVR_Result.WVR_Error_InvalidArgument;
			}

			WVR_SpatialAnchorName anchorNameWVR = default(WVR_SpatialAnchorName);
			anchorNameWVR.name = new char[256];
			anchorName.CopyTo(anchorNameWVR.name, 0);

			if (applyTrackingOriginCorrection && trackingOrigin != null) //Apply origin correction to the anchor pose (world to tracking space)
			{
				Matrix4x4 trackingSpaceOriginTRS = Matrix4x4.TRS(trackingOrigin.transform.position, trackingOrigin.transform.rotation, Vector3.one);
				Matrix4x4 worldSpaceAnchorPoseTRS = Matrix4x4.TRS(anchorPosition, anchorRotation, Vector3.one);

				Matrix4x4 trackingSpaceAnchorPoseTRS = trackingSpaceOriginTRS.inverse * worldSpaceAnchorPoseTRS;

				anchorPosition = trackingSpaceAnchorPoseTRS.GetColumn(3); //4th Column of TRS Matrix is the position
				anchorRotation = Quaternion.LookRotation(trackingSpaceAnchorPoseTRS.GetColumn(2), trackingSpaceAnchorPoseTRS.GetColumn(1));
			}

			WVR_Pose_t anchorPoseWVR = default(WVR_Pose_t);

			if (convertFromUnityToWVR)
			{
				anchorPoseWVR.position = UnityToWVRConversionHelper.GetWVRVector(anchorPosition);
				anchorPoseWVR.rotation = UnityToWVRConversionHelper.GetWVRQuaternion(anchorRotation);
			}
			else
			{
				anchorPoseWVR.position = UnityToWVRConversionHelper.GetWVRVector_NoConversion(anchorPosition);
				anchorPoseWVR.rotation = UnityToWVRConversionHelper.GetWVRQuaternion_NoConversion(anchorRotation);
			}

			WVR_SpatialAnchorCreateInfo spatialAnchorCreateInfo = default(WVR_SpatialAnchorCreateInfo);
			spatialAnchorCreateInfo.anchorName = anchorNameWVR;
			spatialAnchorCreateInfo.originModel = originModel;
			spatialAnchorCreateInfo.pose = anchorPoseWVR;

			WVR_SpatialAnchorCreateInfo[] spatialAnchorCreateInfoArray = { spatialAnchorCreateInfo };
			WVR_Result result = Interop.WVR_CreateSpatialAnchor(spatialAnchorCreateInfoArray, out anchorHandle);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_CreateSpatialAnchor failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_CreateSpatialAnchor Anchor handle: " + anchorHandle);
			}

			return result;
		}

		/// <summary>
		/// Destroy a Spatial Anchor.
		/// </summary>
		/// <param name="anchorHandle">
		/// The handle of the Spatial Anchor to be destroyed.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the anchor is destroyed successfully.
		/// </returns>
		public WVR_Result DestroySpatialAnchor(ulong anchorHandle)
		{
			WVR_Result result = Interop.WVR_DestroySpatialAnchor(anchorHandle);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_DestroySpatialAnchor failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_DestroySpatialAnchor Anchor handle: " + anchorHandle);
			}

			return result;
		}

		/// <summary>
		/// Retrieve all of the exsisting Spatial Anchor handles.
		/// </summary>
		/// <param name="anchorHandles">
		/// An output array of Spatial Anchor handles.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the anchors are retrieved successfully.
		/// </returns>
		public WVR_Result GetSpatialAnchors(out UInt64[] anchorHandles)
		{
			anchorHandles = new UInt64[1];
			UInt32 anchorCount = 0;

			WVR_Result result = Interop.WVR_EnumerateSpatialAnchors(0, out anchorCount, out anchorHandles[0]); //Get anchor count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_EnumerateSpatialAnchors 1 failed with result " + result.ToString());
				return result;
			}
			else
			{

				//Log.d(LOG_TAG, "WVR_EnumerateSpatialAnchors 1 Anchor Count Output: " + anchorCount);
			}

			Array.Resize(ref anchorHandles, (int)anchorCount);
			if (anchorCount <= 0) return result;

			result = Interop.WVR_EnumerateSpatialAnchors(anchorCount, out anchorCount, out anchorHandles[0]); //Get anchors
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_EnumerateSpatialAnchors 2 failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_EnumerateSpatialAnchors 2 successful.");
			}

			return result;
		}

		/// <summary>
		/// Get the state of a Spatial Anchor.
		/// </summary>
		/// <param name="anchorHandle">
		/// The handle of the target Spatial Anchor.
		/// </param>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the anchor.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="anchorState">
		/// The anchor state retrieved.
		/// Only Spatial Anchors that have <see cref="WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Tracking">WVR_SpatialAnchorTrackingState_Tracking</see> as their <see cref="WVR_SpatialAnchorState.trackingState">tracking state</see> should be considered active. 
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the state of the anchor is retrieved successfully.
		/// </returns>
		public WVR_Result GetSpatialAnchorState(UInt64 anchorHandle, WVR_PoseOriginModel originModel, out WVR_SpatialAnchorState anchorState)
		{
			anchorState = default(WVR_SpatialAnchorState);

			WVR_Result result = Interop.WVR_GetSpatialAnchorState(anchorHandle, originModel, out anchorState);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetSpatialAnchorState failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetSpatialAnchorState successful.");
			}

			return result;
		}

		/// <summary>
		/// Get the state of a Spatial Anchor.
		/// This overload of the function returns the anchor state parameters seperately, and provides the option to apply tracking origin correction.
		/// </summary>
		/// <param name="anchorHandle">
		/// The handle of the target Spatial Anchor.
		/// </param>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the anchor.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="anchorTrackingState">
		/// The trakcing state of the anchor.
		/// Only Spatial Anchors that have <see cref="WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Tracking">WVR_SpatialAnchorTrackingState_Tracking</see> as their <paramref name="anchorTrackingState"/> should be considered active.
		/// </param>
		/// <param name="anchorPosition">
		/// The position of the anchor.
		/// </param>
		/// <param name="anchorRotation">
		/// The rotation as quaternion of the anchor.
		/// </param>
		/// <param name="anchorName">
		/// The name of the anchor.
		/// </param>
		/// <param name="applyTrackingOriginCorrection">
		/// Specify whether or not the <see cref="trackingOrigin">Tracking Origin reference</see> should be be used to convert <paramref name="anchorPosition"/> and <paramref name="anchorRotation"/> from tracking to world space.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="ScenePerceptionManager"/> instance in order for this parameter to work as intended.
		/// Set to true by default.
		/// </param>
		/// <returns></returns>
		public WVR_Result GetSpatialAnchorState(UInt64 anchorHandle, WVR_PoseOriginModel originModel, out WVR_SpatialAnchorTrackingState anchorTrackingState, out Vector3 anchorPosition, out Quaternion anchorRotation, out char[] anchorName, bool applyTrackingOriginCorrection = true)
		{
			WVR_SpatialAnchorState anchorState = default(WVR_SpatialAnchorState);
			anchorTrackingState = WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Stopped;
			anchorPosition = Vector3.zero;
			anchorRotation = Quaternion.identity;
			anchorName = new char[0];

			WVR_Result result = Interop.WVR_GetSpatialAnchorState(anchorHandle, originModel, out anchorState);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "WVR_GetSpatialAnchorState failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (applyTrackingOriginCorrection) //Apply origin correction to the anchor pose
				{
					ApplyTrackingOriginCorrectionToAnchorPose(anchorState, out anchorPosition, out anchorRotation);
				}
				else
				{
					anchorTrackingState = anchorState.trackingState;
					Coordinate.GetVectorFromGL(anchorState.pose.position, out anchorPosition);
					Coordinate.GetQuaternionFromGL(anchorState.pose.rotation, out anchorRotation);
				}

				Array.Resize(ref anchorName, anchorState.anchorName.name.Length);
				anchorState.anchorName.name.CopyTo(anchorName, 0);

				//Log.d(LOG_TAG, "WVR_GetSpatialAnchorState successful.");
			}

			return result;
		}

		/// <summary>
		/// A helper function which outputs the world space position and rotation of a <see cref="WVR_SpatialAnchorState"/>.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="ScenePerceptionManager"/> instance in order for this function to work as intended.
		/// </summary>
		/// <param name="anchorState">
		/// The target <see cref="WVR_SpatialAnchorState"/> of which pose will be used in the conversion.
		/// </param>
		/// <param name="anchorPosition">
		/// The world space position of the anchor.
		/// The tracking space position will be returned instead if the <see cref="trackingOrigin">Tracking Origin reference</see> is not assigned to the <see cref="ScenePerceptionManager"/> instance.
		/// </param>
		/// <param name="anchorRotation">
		/// The world space rotation of the anchor.
		/// The tracking space rotation will be returned instead if the <see cref="trackingOrigin">Tracking Origin reference</see> is not assigned to the <see cref="ScenePerceptionManager"/> instance.
		/// </param>
		public void ApplyTrackingOriginCorrectionToAnchorPose(WVR_SpatialAnchorState anchorState, out Vector3 anchorPosition, out Quaternion anchorRotation)
		{
			Coordinate.GetVectorFromGL(anchorState.pose.position, out anchorPosition);
			Coordinate.GetQuaternionFromGL(anchorState.pose.rotation, out anchorRotation);

			if (trackingOrigin != null)
			{
				Matrix4x4 trackingSpaceOriginTRS = Matrix4x4.TRS(trackingOrigin.transform.position, trackingOrigin.transform.rotation, Vector3.one);
				Matrix4x4 trackingSpaceAnchorPoseTRS = Matrix4x4.TRS(anchorPosition, anchorRotation, Vector3.one);

				Matrix4x4 worldSpaceAnchorPoseTRS = trackingSpaceOriginTRS * trackingSpaceAnchorPoseTRS;

				anchorPosition = worldSpaceAnchorPoseTRS.GetColumn(3); //4th Column of TRS Matrix is the position
				anchorRotation = Quaternion.LookRotation(worldSpaceAnchorPoseTRS.GetColumn(2), worldSpaceAnchorPoseTRS.GetColumn(1));
			}
		}

		/// <summary>
		/// A helper function for comparing the poses of two <see cref="WVR_SpatialAnchorState"/>.
		/// </summary>
		/// <param name="scenePlane1">A <see cref="WVR_SpatialAnchorState"/> of which pose will be in the comparison.</param>
		/// <param name="scenePlane2">A <see cref="WVR_SpatialAnchorState"/> of which pose will be in the comparison.</param>
		/// <returns>
		/// true if the poses of the anchors are the identical, false if they are not.
		/// </returns>
		public static bool AnchorStatePoseEqual(WVR_SpatialAnchorState anchorState1, WVR_SpatialAnchorState anchorState2)
		{
			WVR_Pose_t anchorState1Pose = anchorState1.pose;
			WVR_Pose_t anchorState2Pose = anchorState2.pose;

			WVR_Vector3f_t anchorState1PosePosition = anchorState1Pose.position;
			WVR_Vector3f_t anchorState2PosePosition = anchorState2Pose.position;
			WVR_Quatf_t anchorState1PoseRotation = anchorState1Pose.rotation;
			WVR_Quatf_t anchorState2PoseRotation = anchorState2Pose.rotation;

			return (anchorState1PosePosition.v0 == anchorState2PosePosition.v0 &&
					anchorState1PosePosition.v1 == anchorState2PosePosition.v1 &&
					anchorState1PosePosition.v2 == anchorState2PosePosition.v2 &&
					anchorState1PoseRotation.w == anchorState2PoseRotation.w &&
					anchorState1PoseRotation.x == anchorState2PoseRotation.x &&
					anchorState1PoseRotation.y == anchorState2PoseRotation.y &&
					anchorState1PoseRotation.z == anchorState2PoseRotation.z);
		}
		#endregion

		/// <summary>
		/// Get the <see cref="WVR_PoseOriginModel"/> in respect to the current <see cref="TrackingOriginModeFlags"/> in Unity.
		/// </summary>
		/// <returns>
		/// The <see cref="WVR_PoseOriginModel"/> in respect to the current <see cref="TrackingOriginModeFlags"/> in Unity.
		/// </returns>
		public static WVR_PoseOriginModel GetCurrentPoseOriginModel()
		{
			XRInputSubsystem subsystem = Utils.InputSubsystem;
			WVR_PoseOriginModel currentPoseOriginModel = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround;

			if (subsystem != null)
			{
				TrackingOriginModeFlags trackingOriginMode = subsystem.GetTrackingOriginMode();
				

				bool getOriginSuccess = ClientInterface.GetOrigin(trackingOriginMode, ref currentPoseOriginModel);

				if (getOriginSuccess)
				{
					return currentPoseOriginModel;
				}
			}

			return currentPoseOriginModel;
		}

		public static class MeshGenerationHelper
		{
			public static Vector3[] GenerateQuadVertex(WVR_Extent2Df extend2D)
			{
				Vector3[] vertices = new Vector3[4]; //Four corners

				vertices[0] = new Vector3(-extend2D.width / 2, -extend2D.height / 2, 0); //Bottom Left
				vertices[1] = new Vector3(extend2D.width / 2, -extend2D.height / 2, 0); //Bottom Right
				vertices[2] = new Vector3(-extend2D.width / 2, extend2D.height / 2, 0); //Top Left
				vertices[3] = new Vector3(extend2D.width / 2, extend2D.height / 2, 0); //Top Right

				return vertices;
			}

			public static Mesh GenerateQuadMesh(Vector3[] vertices)
			{
				Mesh quadMesh = new Mesh();
				quadMesh.vertices = vertices;

				//Create array that represents vertices of the triangles
				int[] triangles = new int[6];
				triangles[0] = 0;
				triangles[1] = 1;
				triangles[2] = 2;

				triangles[3] = 1;
				triangles[4] = 3;
				triangles[5] = 2;

				quadMesh.triangles = triangles;
				Vector2[] uv = new Vector2[vertices.Length];
				Vector4[] tangents = new Vector4[vertices.Length];
				Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
				for (int i = 0, y = 0; y < 2; y++)
				{
					for (int x = 0; x < 2; x++, i++)
					{
						uv[i] = new Vector2((float)x, (float)y);
						tangents[i] = tangent;
					}
				}
				quadMesh.uv = uv;
				quadMesh.tangents = tangents;
				quadMesh.RecalculateNormals();

				return quadMesh;
			}
			public static Mesh GenerateMesh(WVR_Vector3f_t[] vertexBuffer, UInt32[] indexBuffer)
			{
				Mesh generatedMesh = new Mesh();

				if (vertexBuffer.Length >= 65535)
				{
					generatedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
				}

				Vector3[] vertexBufferUnity = new Vector3[vertexBuffer.Length];
				for (int i=0; i<vertexBuffer.Length; i++)
				{
					Coordinate.GetVectorFromGL(vertexBuffer[i], out vertexBufferUnity[i]);
				}
				generatedMesh.vertices = vertexBufferUnity;

				int[] indexBufferUnity = new int[indexBuffer.Length];
				for (int i = 0; i < indexBuffer.Length; i++)
				{
					int indexMod3 = i % 3;
					if (indexMod3 == 0)
					{
						indexBufferUnity[i] = (int)indexBuffer[i];
					}
					else if (indexMod3 == 1)
					{
						indexBufferUnity[i] = (int)indexBuffer[i+1];
					}
					else if (indexMod3 == 2)
					{
						indexBufferUnity[i] = (int)indexBuffer[i-1];
					}
				}

				generatedMesh.triangles = indexBufferUnity;
				Vector2[] uv = new Vector2[vertexBuffer.Length];
				Vector4[] tangents = new Vector4[vertexBuffer.Length];
				Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
				for (int i = 0, y = 0; y < 2; y++)
				{
					for (int x = 0; x < 2; x++, i++)
					{
						uv[i] = new Vector2((float)x, (float)y);
						tangents[i] = tangent;
					}
				}
				generatedMesh.uv = uv;
				generatedMesh.tangents = tangents;
				generatedMesh.RecalculateNormals();


				return generatedMesh;
			}
		}

		private class UnityToWVRConversionHelper
		{
			public static WVR_Quatf_t GetWVRQuaternion(Quaternion rot)
			{
				WVR_Quatf_t qua = new WVR_Quatf_t();
				qua.x = rot.x;
				qua.y = rot.y;
				qua.z = -rot.z;
				qua.w = -rot.w;
				return qua;
			}

			public static WVR_Vector3f_t GetWVRVector(Vector3 pos)
			{
				WVR_Vector3f_t vec = new WVR_Vector3f_t();
				vec.v0 = pos.x;
				vec.v1 = pos.y;
				vec.v2 = -pos.z;
				return vec;
			}

			public static WVR_Quatf_t GetWVRQuaternion_NoConversion(Quaternion rot)
			{
				WVR_Quatf_t qua = new WVR_Quatf_t();
				qua.x = rot.x;
				qua.y = rot.y;
				qua.z = rot.z;
				qua.w = rot.w;
				return qua;
			}

			public static WVR_Vector3f_t GetWVRVector_NoConversion(Vector3 pos)
			{
				WVR_Vector3f_t vec = new WVR_Vector3f_t();
				vec.v0 = pos.x;
				vec.v1 = pos.y;
				vec.v2 = pos.z;
				return vec;
			}
		}
	}
}
