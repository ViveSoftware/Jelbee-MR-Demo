using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;
using Wave.Native;
using Wave.XR;

namespace Wave.Essence.ScenePerception
{
	// Shorter name for the enum WVR_SpatialAnchorTrackingState
	public enum SpatialAnchorTrackingState
	{
		Tracking = WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Tracking,
		Paused = WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Paused,
		Stopped = WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Stopped,
	}

	public class SpatialAnchorState
	{
		public readonly SpatialAnchorTrackingState trackingState;
		public readonly Pose pose;
		public readonly string anchorName;

		public SpatialAnchorState(WVR_SpatialAnchorState wvrState)
		{
			trackingState = (SpatialAnchorTrackingState)wvrState.trackingState;
			Coordinate.GetVectorFromGL(wvrState.pose.position, out var position);
			Coordinate.GetQuaternionFromGL(wvrState.pose.rotation, out var rotation);
			pose = new Pose(position, rotation);
			anchorName = wvrState.anchorName.ToString();
		}

		public SpatialAnchorState(SpatialAnchorState other)
		{
			trackingState = other.trackingState;
			pose = other.pose;
			anchorName = other.anchorName;
		}
	}

	/// <summary>
	/// The ScenePlane class contains the information from WVR_ScenePlane.  Please not to modify the values in this class.
	/// </summary>
	public class ScenePlane
	{
		public readonly WVR_Uuid uuid;
		public readonly WVR_Uuid parentUuid;
		public readonly ulong meshBufferId;
		public readonly Pose pose;
		public readonly Vector2 extent;
		public readonly WVR_ScenePlaneType planeType;
		public readonly WVR_ScenePlaneLabel planeLabel;
		public readonly string semanticName;

		public ScenePlane(WVR_ScenePlane wvrPlane)
		{
			uuid = wvrPlane.uuid;
			parentUuid = wvrPlane.parentUuid;
			meshBufferId = wvrPlane.meshBufferId;
			Coordinate.GetVectorFromGL(wvrPlane.pose.position, out var position);
			Coordinate.GetQuaternionFromGL(wvrPlane.pose.rotation, out var rotation);
			var forwardDirRot = Quaternion.Euler(0, 180f, 0);
			pose = new Pose(position, rotation * forwardDirRot);
			extent = new Vector2(wvrPlane.extent.width, wvrPlane.extent.height);
			planeType = wvrPlane.planeType;
			planeLabel = wvrPlane.planeLabel;
			semanticName = wvrPlane.semanticName.ToString();
		}

		public ScenePlane(ScenePlane other)
		{
			uuid = other.uuid;
			parentUuid = other.parentUuid;
			meshBufferId = other.meshBufferId;
			pose = other.pose;
			extent = other.extent;
			planeType = other.planeType;
			planeLabel = other.planeLabel;
			semanticName = other.semanticName;
		}
	}

	/// <summary>
	/// The SceneObject class contains the information from WVR_SceneObject.  Please not to modify the values in this class.
	/// </summary>
	public class SceneObject
	{
		public readonly WVR_Uuid uuid;
		public readonly WVR_Uuid parentUuid;
		public readonly ulong meshBufferId;
		public readonly Pose pose;  // the center pose of the object
		public readonly Vector3 extent;  // width, height, depth
		public readonly string semanticName;

		public SceneObject(WVR_SceneObject wvrObject)
		{
			uuid = wvrObject.uuid;
			parentUuid = wvrObject.parentUuid;
			meshBufferId = wvrObject.meshBufferId;
			Coordinate.GetVectorFromGL(wvrObject.pose.position, out var position);
			Coordinate.GetQuaternionFromGL(wvrObject.pose.rotation, out var rotation);
			var forwardDirRot = Quaternion.Euler(0, 180f, 0);
			pose = new Pose(position, rotation * forwardDirRot);
			extent = new Vector3(wvrObject.extent.width, wvrObject.extent.height, wvrObject.extent.depth);
			semanticName = wvrObject.semanticName.ToString();
		}

		public SceneObject(SceneObject other)
		{
			uuid = other.uuid;
			parentUuid = other.parentUuid;
			meshBufferId = other.meshBufferId;
			pose = other.pose;
			extent = other.extent;
			semanticName = other.semanticName;
		}
	}

	/// <summary>
	/// The SceneMesh class contains the information from WVR_SceneMesh.  Please not to modify the values in this class.
	/// </summary>
	public class SceneMesh
	{
		public readonly ulong meshBufferId;

		public SceneMesh(WVR_SceneMesh wvrMesh)
		{
			meshBufferId = wvrMesh.meshBufferId;
		}

		public SceneMesh(SceneMesh other)
		{
			meshBufferId = other.meshBufferId;
		}
	}

	public class ScenePerceptionManager : MonoBehaviour
	{
		private readonly bool logPersisted = true;
		private readonly bool logCached = true;

		private const string TAG = "ScenePerceptionManager";

		private readonly ScenePerceptionInternal spInternal = new ScenePerceptionInternal();

		// Anchor's API will cost lot of responce time. Use Lock to avoid multi-thread issue.
		private readonly object lockObject = new object();
		private ThreadLocal<StringBuilder> threadLocalStringBuilder =
			new ThreadLocal<StringBuilder>(() => new StringBuilder());
		// This is a thread safe string builder
		public StringBuilder TSCSB { get { return threadLocalStringBuilder.Value.Clear(); } }

		[SerializeField, Tooltip("The reference point of tracking origin.  It could be the WaveRig's transform or the parent transform of Camera.main.  If leave it null, this manager will try it set to WaveRig or Camera.main.")]
		Transform trackingOrigin = null;

		// Always not null.  This is just a helper function.  Try to have your own way to get tracking origin.
		public Transform TrackingOrigin { get { CheckTrackingOrigin(); return trackingOrigin; } set { trackingOrigin = value; } }

		void CheckTrackingOrigin()
		{
			if (trackingOrigin == null)
			{
				if (WaveRig.Instance != null)
					trackingOrigin = WaveRig.Instance.transform;
				else if (Camera.main != null)
					trackingOrigin = Camera.main.transform.parent;
				else
					trackingOrigin = transform.root;
			}
		}


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
				Log.e(TAG, "WVR_StartScene failed with result " + result.ToString());
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
				Log.e(TAG, "WVR_StartScenePerception failed with result " + result.ToString());
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
				Log.e(TAG, "WVR_StopScenePerception failed with result " + result.ToString());
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
				Log.e(TAG, "WVR_GetScenePerceptionState failed with result " + result.ToString());
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
		[Obsolete("This function return GL convension of pose.  Please use GetScenePlanes(PoseOriginModel, ScenePlane[]) instead.")]
		public WVR_Result GetScenePlanes(WVR_PoseOriginModel originModel, out WVR_ScenePlane[] planes) //No filter
		{
			return spInternal.GetScenePlanes(originModel, out planes);
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
		[Obsolete("This function return GL convension of pose.  Please use GetScenePlanes(WVR_ScenePlaneFilter, PoseOriginModel, ScenePlane[]) instead.")]
		public WVR_Result GetScenePlanes(WVR_ScenePlaneFilter planeFilter, WVR_PoseOriginModel originModel, out WVR_ScenePlane[] planes)
		{
			return spInternal.GetScenePlanes(originModel, out planes, planeFilter.planeType, planeFilter.planeLabel);
		}

		/// <summary>
		/// Get a filtered subset of the Scene Planes that are currently on the device.
		/// <param name="originMode">
		/// The pose data of the scene planes will different depend on tracking origin mode.
		/// You can use <see cref="GetTrackingOriginModeFlags"/> to get the <see cref="TrackingOriginModeFlags">
		/// Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="planes">
		/// An array of <see cref="ScenePlane">Scene Planes</see> retrieved from the device.
		/// Only planes that match the specified <see cref="WVR_ScenePlaneType"/> and <see cref="WVR_ScenePlaneLabel"/> in <paramref name="planeFilter"/> will be returned in this array.
		/// </param>
		/// <param name="planeType">
		/// The type filter to be applied.  Optional. Default is no filter.
		/// </param>
		/// <param name="planeLabel">
		/// The label filter to be applied.  Optional.  Default is no filter.
		/// </param>
		/// </summary>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the scene planes are retrieved successfully.
		/// </returns>
		public WVR_Result GetScenePlanes(TrackingOriginModeFlags originMode, out ScenePlane[] planes, WVR_ScenePlaneType planeType = WVR_ScenePlaneType.WVR_ScenePlaneType_Max, WVR_ScenePlaneLabel planeLabel = WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Max) //No filter
		{
			if (originMode != TrackingOriginModeFlags.Floor && originMode != TrackingOriginModeFlags.Device)
			{
				planes = null;
				return WVR_Result.WVR_Error_Data_Invalid;
			}

			var originModel = originMode == TrackingOriginModeFlags.Floor ?
				WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround :
				WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead;

			var result = spInternal.GetScenePlanes(originModel, out WVR_ScenePlane[] outPlanes, planeType, planeLabel);

			if (result != WVR_Result.WVR_Success || outPlanes == null)
			{
				planes = new ScenePlane[0];
				return result;
			}

			planes = new ScenePlane[outPlanes.Length];
			for (int i = 0; i < outPlanes.Length; i++)
			{
				planes[i] = new ScenePlane(outPlanes[i]);
			}

			return result;
		}

		/// <summary>
		/// Get a filtered subset of the Scene Objects that are currently on the device.
		/// </summary>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the scene objects.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="objects">
		/// An array of <see cref="WVR_SceneObject">Scene Objects</see> retrieved from the device.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the scene objects are retrieved successfully.
		/// </returns>
		[Obsolete("This function return GL convension of pose.  Please use GetSceneObjects(PoseOriginModel, SceneObject[]) instead.")]
		public WVR_Result GetSceneObjects(WVR_PoseOriginModel originModel, out WVR_SceneObject[] objects)
		{
			return spInternal.GetSceneObjects(originModel, out objects);
		}

		/// <summary>
		/// Get a filtered subset of the Scene Objects that are currently on the device.
		/// <param name="originFlag">
		/// The pose data of the scene planes will different depend on tracking origin mode.
		/// You can use <see cref="GetTrackingOriginModeFlags"/> to get the <see cref="TrackingOriginModeFlags">
		/// Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="objects">
		/// An array of <see cref="SceneObject">Scene Objects</see> retrieved from the device.
		/// </param>
		/// </summary>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the scene objects are retrieved successfully.
		/// </returns>
		public WVR_Result GetSceneObjects(TrackingOriginModeFlags originMode, out SceneObject[] objects)
		{
			var originModel = originMode == TrackingOriginModeFlags.Floor ?
				WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround :
				WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead;

			var result = spInternal.GetSceneObjects(originModel, out WVR_SceneObject[] outObjects);
			if (result != WVR_Result.WVR_Success || outObjects == null)
			{
				objects = new SceneObject[0];
				return result;
			}

			objects = new SceneObject[outObjects.Length];
			for (int i = 0; i < outObjects.Length; i++)
				objects[i] = new SceneObject(outObjects[i]);

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
		[Obsolete("This function return GL convension of pose.  Please use GetSceneMeshes(SceneMeshType, SceneMesh[]) instead.")]
		public WVR_Result GetSceneMeshes(WVR_SceneMeshType meshType, out WVR_SceneMesh[] meshes)
		{
			return spInternal.GetSceneMeshes(meshType, out meshes);
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
		public WVR_Result GetSceneMeshes(WVR_SceneMeshType meshType, out SceneMesh[] meshes)
		{
			var result = spInternal.GetSceneMeshes(meshType, out WVR_SceneMesh[] outMeshes);
			if (result != WVR_Result.WVR_Success || outMeshes == null)
			{
				meshes = new SceneMesh[0];
				return result;
			}

			meshes = new SceneMesh[outMeshes.Length];
			for (int i = 0; i < outMeshes.Length; i++)
				meshes[i] = new SceneMesh(outMeshes[i]);

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
		[Obsolete("This function return GL convension of pose.  Please use GetSceneMeshBuffer(ulong, out Vector3[], out uint[]) instead.")]
		public WVR_Result GetSceneMeshBuffer(UInt64 meshBufferId, out WVR_Vector3f_t[] vertexBuffer, out UInt32[] indexBuffer)
		{
			return spInternal.GetSceneMeshBuffer(meshBufferId, out vertexBuffer, out indexBuffer);
		}

		/// <summary>
		/// Get the vertices and indices of the mesh with its mesh buffer id.
		/// </summary>
		/// <param name="meshBufferId">
		/// The ID of the mesh to be retrieved.
		/// </param>
		/// <param name="vertexBuffer">
		/// An array of <see cref="Vector3"/> which each represents a vertex of the mesh.
		/// </param>
		/// <param name="indexBuffer">
		/// An array of <see cref="int"/> which represents the triangles of the mesh.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the scene mesh buffer is retrieved successfully.
		/// </returns>
		public WVR_Result GetSceneMeshBuffer(ulong meshBufferId, out Vector3[] vertexBuffer, out int[] indexBuffer)
		{
			var result = spInternal.GetSceneMeshBuffer(meshBufferId, out var wvrVertexBuffer, out var wvrIndexBuffer);
			if (result != WVR_Result.WVR_Success || wvrVertexBuffer == null || wvrIndexBuffer == null)
			{
				vertexBuffer = new Vector3[0];
				indexBuffer = new int[0];
				return result;
			}

			vertexBuffer = new Vector3[wvrVertexBuffer.Length];
			for (int i = 0; i < wvrVertexBuffer.Length; i++)
			{
				vertexBuffer[i] = Coordinate.GetVectorFromGL(wvrVertexBuffer[i]);
			}

			indexBuffer = new int[wvrIndexBuffer.Length];
			// Unity is using clockwise winding order for front face.
			if (wvrIndexBuffer.Length % 3 != 0)
				Log.d(TAG, "MeshTools: Indices is not 3's multiple!");
			int tc = wvrIndexBuffer.Length / 3;  // Triangle Count
			for (int i = 0; i < tc; i++)
			{
				var j = i * 3;
				indexBuffer[j] = (int)wvrIndexBuffer[j];
				indexBuffer[j + 1] = (int)wvrIndexBuffer[j + 2];  // Index order need be reversed in Unity
				indexBuffer[j + 2] = (int)wvrIndexBuffer[j + 1];
			}
			return result;
		}

		[Obsolete("Please use ScenePerceptionObjectTools.GenerateSceneMesh instead.")]
		public GameObject GenerateSceneMesh(WVR_SceneMesh sceneMesh, Material meshMaterial, bool attachMeshCollider = false)
		{
			return ScenePerceptionObjectTools.GenerateSceneMesh(this, new SceneMesh(sceneMesh), meshMaterial, attachMeshCollider);
		}

		/// <summary>
		/// A helper function for comparing two <see cref="WVR_Uuid"/>.
		/// </summary>
		/// <param name="uuid1">A <see cref="WVR_Uuid"/> of which will be in the comparison.</param>
		/// <param name="uuid2">A <see cref="WVR_Uuid"/> of which will be in the comparison.</param>
		/// <returns>
		/// true if the Uuids are the identical, false if they are not.
		/// </returns>
		[Obsolete("Please directly compare the uuids by == or != instead of using this function.")]
		public static bool IsUUIDEqual(WVR_Uuid uuid1, WVR_Uuid uuid2)
		{
			return uuid1 == uuid2;
		}

		[Obsolete("Please use ScenePerceptionObjectTools.GenerateScenePlaneMesh instead.")]
		public GameObject GenerateScenePlaneMesh(WVR_ScenePlane scenePlane, Material meshMaterial, bool attachMeshCollider = false, bool applyTrackingOriginCorrection = true)
		{
			return ScenePerceptionObjectTools.GenerateScenePlaneMesh(new ScenePlane(scenePlane), meshMaterial, attachMeshCollider, TrackingOrigin);
		}


		[Obsolete("Please use ScenePerceptionObjectTools.TrackingSpaceToWorldSpace instead.")]
		public void ApplyTrackingOriginCorrectionToPlanePose(WVR_ScenePlane scenePlane, out Vector3 planePosition, out Quaternion planeRotation)
		{
			var obj = new ScenePlane(scenePlane);
			ScenePerceptionObjectTools.TrackingSpaceToWorldSpace(TrackingOrigin, obj.pose.position, obj.pose.rotation, out planePosition, out planeRotation);
		}

		[Obsolete("Please use ScenePlane.pose instead.")]
		public static bool ScenePlanePoseEqual(WVR_ScenePlane scenePlane1, WVR_ScenePlane scenePlane2)

		{
			var sp1 = new ScenePlane(scenePlane1);
			var sp2 = new ScenePlane(scenePlane2);
			return sp1.pose == sp2.pose;
		}

		[Obsolete("Please use ScenePlane.pose instead.")]
		public static bool ScenePlaneExtent2DEqual(WVR_ScenePlane scenePlane1, WVR_ScenePlane scenePlane2)
		{
			var sp1 = new ScenePlane(scenePlane1);
			var sp2 = new ScenePlane(scenePlane2);
			return sp1.extent == sp2.extent;
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
				Log.e(TAG, "CreateSpatialAnchor: anchor name should be under 256 characters.");
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
			WVR_Result result;
			lock (lockObject)
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
				Log.e(TAG, "CreateSpatialAnchor: anchor name should be under 256 characters.");
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
			WVR_Result result;
			lock (lockObject)
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
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_DestroySpatialAnchor(anchorHandle);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_DestroySpatialAnchor failed with result " + result.ToString());
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

			WVR_Result result;
			lock (lockObject)
			{
				result = Interop.WVR_EnumerateSpatialAnchors(0, out anchorCount, out anchorHandles[0]); //Get anchor count
				if (result != WVR_Result.WVR_Success)
				{
					Log.e(TAG, "WVR_EnumerateSpatialAnchors 1 failed with result " + result.ToString());
					return result;
				}
				else
				{

					//Log.d(LOG_TAG, "WVR_EnumerateSpatialAnchors 1 Anchor Count Output: " + anchorCount);
				}

				Array.Resize(ref anchorHandles, (int)anchorCount);
				if (anchorCount <= 0) return result;

				result = Interop.WVR_EnumerateSpatialAnchors(anchorCount, out anchorCount, out anchorHandles[0]); //Get anchors
			}
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_EnumerateSpatialAnchors 2 failed with result " + result.ToString());
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
			anchorState.anchorName = default(WVR_SpatialAnchorName);
			anchorState.anchorName.name = new char[256];
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_GetSpatialAnchorState(anchorHandle, originModel, out anchorState);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetSpatialAnchorState failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "WVR_GetSpatialAnchorState successful.");
			}

			return result;
		}

		public WVR_Result CacheSpatialAnchor(WVR_SpatialAnchorName cachedSpatialAnchorName, UInt64 anchorHandle)
		{
			WVR_SpatialAnchorCacheInfo spatialAnchorPersistInfo = new WVR_SpatialAnchorCacheInfo();
			spatialAnchorPersistInfo.cachedSpatialAnchorName = cachedSpatialAnchorName;
			spatialAnchorPersistInfo.spatialAnchor = anchorHandle;

			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_CacheSpatialAnchor(ref spatialAnchorPersistInfo);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_CacheSpatialAnchor failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logCached) Log.v(TAG, "WVR_CacheSpatialAnchor successful.");
			}

			return result;
		}

		public WVR_Result CacheSpatialAnchor(string cachedSpatialAnchorName, UInt64 anchorHandle)
		{
			return CacheSpatialAnchor(cachedSpatialAnchorName.ToSpatialAnchorName(), anchorHandle);
		}

		public WVR_Result UncacheSpatialAnchor(WVR_SpatialAnchorName cachedSpatialAnchorName)
		{
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_UncacheSpatialAnchor(ref cachedSpatialAnchorName);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_UncacheSpatialAnchor failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logCached) Log.v(TAG, "WVR_UncacheSpatialAnchor successful.");
			}

			return result;
		}

		public WVR_Result UncacheSpatialAnchor(string cachedSpatialAnchorName)
		{
			return UncacheSpatialAnchor(cachedSpatialAnchorName.ToSpatialAnchorName());
		}

		public WVR_Result GetCachedSpatialAnchorNames(out WVR_SpatialAnchorName[] cachedSpatialAnchorNames)
		{
			cachedSpatialAnchorNames = null;
			UInt32 cachedSpatialAnchorNamesCountOutput;
			WVR_Result result;
			lock (lockObject)
			{
				result = Interop.WVR_EnumerateCachedSpatialAnchorNames(
					0,
					out cachedSpatialAnchorNamesCountOutput,
					cachedSpatialAnchorNames);
				if (result != WVR_Result.WVR_Success)
				{
					Log.e(TAG, "WVR_EnumerateCachedSpatialAnchorNames 1 failed with result " + result.ToString());
					return result;
				}

				cachedSpatialAnchorNames = new WVR_SpatialAnchorName[cachedSpatialAnchorNamesCountOutput];
				result = Interop.WVR_EnumerateCachedSpatialAnchorNames(
					cachedSpatialAnchorNamesCountOutput,
					out _,
					cachedSpatialAnchorNames);
			}
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_EnumerateCachedSpatialAnchorNames 2 failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logCached) {
					Log.v(TAG, "WVR_EnumerateCachedSpatialAnchorNames successful.");
					if (cachedSpatialAnchorNames != null && cachedSpatialAnchorNames.Length > 0)
						foreach (var name in cachedSpatialAnchorNames)
							Log.v(TAG, TSCSB.Append("  ").Append(name.FromSpatialAnchorName()));
				}
			}

			return result;
		}

		public WVR_Result GetCachedSpatialAnchorNames(out string[] cachedSpatialAnchorNames)
		{
			cachedSpatialAnchorNames = null;
			WVR_SpatialAnchorName[] cachedWVRSpatialAnchorNames;
			WVR_Result result = GetCachedSpatialAnchorNames(out cachedWVRSpatialAnchorNames);
			if (result != WVR_Result.WVR_Success)
				return result;
			cachedSpatialAnchorNames = new string[cachedWVRSpatialAnchorNames.Length];
			for (int i = 0; i < cachedWVRSpatialAnchorNames.Length; i++)
				cachedSpatialAnchorNames[i] = cachedWVRSpatialAnchorNames[i].FromSpatialAnchorName();
			return result;
		}

		public WVR_Result ClearCachedSpatialAnchors()
		{
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_ClearCachedSpatialAnchors();
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_ClearCachedSpatialAnchors failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logCached) Log.v(TAG, "WVR_ClearCachedSpatialAnchors successful.");
			}

			return result;
		}

		public WVR_Result CreateSpatialAnchorFromCacheName(string cachedAnchorName, string spatialAnchorName, out UInt64 anchor)
		{
			if (logCached) Log.v(TAG, $"CreateSpatialAnchorFromCacheName(san={spatialAnchorName}, can={cachedAnchorName})");

			var info = new WVR_SpatialAnchorFromCacheNameCreateInfo();
			info.spatialAnchorName = spatialAnchorName.ToSpatialAnchorName();
			info.cachedSpatialAnchorName = cachedAnchorName.ToSpatialAnchorName();

			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_CreateSpatialAnchorFromCacheName(ref info, out anchor);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_CreateSpatialAnchorFromCacheName failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logCached) Log.v(TAG, "WVR_CreateSpatialAnchorFromCacheName successful.");
			}

			return result;
		}

		public WVR_Result PersistSpatialAnchor(WVR_SpatialAnchorName persistedSpatialAnchorName, UInt64 anchor)
		{
			WVR_SpatialAnchorPersistInfo spatialAnchorPersistInfo = new WVR_SpatialAnchorPersistInfo();
			spatialAnchorPersistInfo.persistedSpatialAnchorName = persistedSpatialAnchorName;
			spatialAnchorPersistInfo.spatialAnchor = anchor;
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_PersistSpatialAnchor(ref spatialAnchorPersistInfo);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_PersistSpatialAnchor failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logPersisted) Log.v(TAG, "WVR_PersistSpatialAnchor successful.");
			}

			return result;
		}

		public WVR_Result PersistSpatialAnchor(string persistedSpatialAnchorName, UInt64 anchor)
		{
			return PersistSpatialAnchor(persistedSpatialAnchorName.ToSpatialAnchorName(), anchor);
		}

		public WVR_Result UnpersistSpatialAnchor(string name)
		{
			var persistedSpatialAnchorName = name.ToSpatialAnchorName();
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_UnpersistSpatialAnchor(ref persistedSpatialAnchorName);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_UnpersistSpatialAnchor failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logPersisted) Log.v(TAG, "WVR_UnpersistSpatialAnchor successful.");
			}

			return result;
		}

		public WVR_Result GetPersistedSpatialAnchorNames(out WVR_SpatialAnchorName[] persistedSpatialAnchorNames)
		{
			persistedSpatialAnchorNames = null;
			uint persistedSpatialAnchorNamesCountOutput;
			WVR_Result result;
			lock (lockObject)
			{
				result = Interop.WVR_EnumeratePersistedSpatialAnchorNames(
					0,
					out persistedSpatialAnchorNamesCountOutput,
					persistedSpatialAnchorNames);
				if (result != WVR_Result.WVR_Success)
				{
					Log.e(TAG, "WVR_EnumeratePersistedSpatialAnchorNames 1 failed with result " + result.ToString());
					return result;
				}

				persistedSpatialAnchorNames = new WVR_SpatialAnchorName[persistedSpatialAnchorNamesCountOutput];
				result = Interop.WVR_EnumeratePersistedSpatialAnchorNames(
					persistedSpatialAnchorNamesCountOutput,
					out _,
					persistedSpatialAnchorNames);
			}
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_EnumeratePersistedSpatialAnchorNames 2 failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logPersisted)
				{
					var sb = TSCSB.Append("WVR_EnumeratePersistedSpatialAnchorNames successful.");
					if (persistedSpatialAnchorNames != null && persistedSpatialAnchorNames.Length > 0)
						foreach (var name in persistedSpatialAnchorNames)
						{
							sb.AppendLine().Append("  ").Append(name.FromSpatialAnchorName());
						}
					Log.v(TAG, sb);
				}
			}

			return result;
		}

		public WVR_Result GetPersistedSpatialAnchorNames(out string[] persistedSpatialAnchorNames)
		{
			persistedSpatialAnchorNames = null;
			WVR_Result result = GetPersistedSpatialAnchorNames(out WVR_SpatialAnchorName[] persistedSpatialAnchorWVRNames);
			if (result != WVR_Result.WVR_Success) return result;
			persistedSpatialAnchorNames = new string[persistedSpatialAnchorWVRNames.Length];
			for (int i = 0; i < persistedSpatialAnchorWVRNames.Length; i++)
				persistedSpatialAnchorNames[i] = persistedSpatialAnchorWVRNames[i].FromSpatialAnchorName();
			return result;
		}

		public WVR_Result ClearPersistedSpatialAnchors()
		{
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_ClearPersistedSpatialAnchors();
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_ClearPersistedSpatialAnchors failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logPersisted) Log.v(TAG, "WVR_ClearPersistedSpatialAnchors successful.");
			}

			return result;
		}

		public WVR_Result GetPersistedSpatialAnchorCount(
			out WVR_PersistedSpatialAnchorCountGetInfo getInfo)
		{
			getInfo = new WVR_PersistedSpatialAnchorCountGetInfo();
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_GetPersistedSpatialAnchorCount(ref getInfo);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_GetPersistedSpatialAnchorCount failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logPersisted) Log.v(TAG, "WVR_GetPersistedSpatialAnchorCount successful.");
			}

			return result;
		}

		public WVR_Result CreateSpatialAnchorFromPersistenceName(
			string persistedSpatialAnchorName,
			string spatialAnchorName,
			out UInt64 anchor /* WVR_SpatialAnchor* */)
		{
			var createInfo = new WVR_SpatialAnchorFromPersistenceNameCreateInfo();
			createInfo.persistedSpatialAnchorName = persistedSpatialAnchorName.ToSpatialAnchorName();
			createInfo.spatialAnchorName = spatialAnchorName.ToSpatialAnchorName();
			WVR_Result result;
			lock (lockObject)
				result = Interop.WVR_CreateSpatialAnchorFromPersistenceName(ref createInfo, out anchor);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "WVR_CreateSpatialAnchorFromPersistenceName failed with result " + result.ToString());
				return result;
			}
			else
			{
				if (logPersisted) Log.v(TAG, "WVR_CreateSpatialAnchorFromPersistenceName successful.");
			}

			return result;
		}

		public WVR_Result ExportPersistedSpatialAnchor(
			string persistedSpatialAnchorName,
			out byte[] data)
		{
			data = null;
			uint dataCountOutput;
			var persistedSpatialAnchorWVRName = persistedSpatialAnchorName.ToSpatialAnchorName();
			WVR_Result result;
			lock (lockObject)
			{
				result = Interop.WVR_ExportPersistedSpatialAnchor(ref persistedSpatialAnchorWVRName, 0, out dataCountOutput, data);
				if (result != WVR_Result.WVR_Success)
				{
					Log.e(TAG, "WVR_ExportPersistedSpatialAnchor 1 failed with result " + result.ToString());
					return result;
				}
				data = new byte[dataCountOutput];

				if (dataCountOutput != 0)
					result = Interop.WVR_ExportPersistedSpatialAnchor(ref persistedSpatialAnchorWVRName, dataCountOutput, out _, data);
				if (result != WVR_Result.WVR_Success)
				{
					Log.e(TAG, "WVR_ExportPersistedSpatialAnchor 2 failed with result " + result.ToString());
					return result;
				}
				else
				{
					if (logPersisted)
					{
						var sb = TSCSB
							.Append("WVR_ExportPersistedSpatialAnchor successful.")
							.Append(" (").Append(dataCountOutput).Append(" bytes)");
						Log.v(TAG, sb);
					}
				}
			}
			return result;
		}

		public WVR_Result ImportPersistedSpatialAnchor(byte[] data)
		{
			if (data == null || data.Length == 0) return WVR_Result.WVR_Error_Data_Invalid;
			Log.d(TAG, TSCSB.Append("ImportPersistedSpatialAnchor(").Append(data.Length).Append(")"));
			WVR_Result result;
			lock (lockObject)
			{
				result = Interop.WVR_ImportPersistedSpatialAnchor((uint)data.Length, data);
				if (result != WVR_Result.WVR_Success)
				{
					Log.e(TAG, "WVR_ImportPersistedSpatialAnchor failed with result " + result.ToString());
					return result;
				}
				else
				{
					if (logPersisted) Log.v(TAG, "WVR_ImportPersistedSpatialAnchor successful.");
				}
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
		/// The name char array of the anchor.
		/// </param>
		/// <param name="applyTrackingOriginCorrection">
		/// Specify whether or not the <see cref="trackingOrigin">Tracking Origin reference</see> should be be used to convert <paramref name="anchorPosition"/> and <paramref name="anchorRotation"/> from tracking to world space.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="ScenePerceptionManager"/> instance in order for this parameter to work as intended.
		/// Set to true by default.
		/// </param>
		/// <returns></returns>
		public WVR_Result GetSpatialAnchorState(UInt64 anchorHandle, WVR_PoseOriginModel originModel, out SpatialAnchorTrackingState anchorTrackingState, out Vector3 anchorPosition, out Quaternion anchorRotation, out char[] anchorName)
		{
			var wvrResult = GetSpatialAnchorState(anchorHandle, originModel, out var anchorState);
			anchorTrackingState = SpatialAnchorTrackingState.Stopped;
			anchorPosition = Vector3.zero;
			anchorRotation = Quaternion.identity;
			anchorName = null;
			if (wvrResult != WVR_Result.WVR_Success) return wvrResult;

			anchorTrackingState = (SpatialAnchorTrackingState)anchorState.trackingState;
			Coordinate.GetVectorFromGL(anchorState.pose.position, out anchorPosition);
			Coordinate.GetQuaternionFromGL(anchorState.pose.rotation, out anchorRotation);

			Array.Resize(ref anchorName, anchorState.anchorName.name.Length);
			anchorState.anchorName.name.CopyTo(anchorName, 0);

			return wvrResult;
		}

		public WVR_Result GetSpatialAnchorState(UInt64 anchorHandle, WVR_PoseOriginModel originModel, out SpatialAnchorTrackingState anchorTrackingState, out Vector3 anchorPosition, out Quaternion anchorRotation, out char[] anchorName, Pose trackingOriginPose)
		{
			var wvrResult = GetSpatialAnchorState(anchorHandle, originModel, out var anchorState);
			anchorTrackingState = SpatialAnchorTrackingState.Stopped;
			anchorPosition = Vector3.zero;
			anchorRotation = Quaternion.identity;
			anchorName = null;
			if (wvrResult != WVR_Result.WVR_Success) return wvrResult;

			ApplyTrackingOriginCorrectionToAnchorPose(trackingOriginPose, anchorState, out anchorPosition, out anchorRotation);

			Array.Resize(ref anchorName, anchorState.anchorName.name.Length);
			anchorState.anchorName.name.CopyTo(anchorName, 0);

			return wvrResult;
		}

		/// <summary>
		///	Get the state of a Spatial Anchor.
		///	This overload of the function returns the anchor state parameters seperately, and provides the option to apply tracking origin correction.
		/// </summary>
		/// <param name="anchorHandle">The handle of the target Spatial Anchor.</param>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the anchor.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="anchorTrackingState">
		/// The trakcing state of the anchor.
		/// Only Spatial Anchors that have <see cref="WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Tracking">WVR_SpatialAnchorTrackingState_Tracking</see> as their <paramref name="anchorTrackingState"/> should be considered active.
		/// </param>
		/// <param name="pose">The pose of anchor</param>
		/// <param name="anchorName">The name of the anchor</param>
		/// <param name="applyTrackingOriginCorrection">
		/// Specify whether or not the <see cref="trackingOrigin">Tracking Origin reference</see> should be be used to convert <paramref name="anchorPosition"/> and <paramref name="anchorRotation"/> from tracking to world space.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="ScenePerceptionManager"/> instance in order for this parameter to work as intended.
		/// Set to true by default.
		/// </param>
		/// <returns></returns>
		public WVR_Result GetSpatialAnchorState(UInt64 anchorHandle, WVR_PoseOriginModel originModel, out SpatialAnchorTrackingState anchorTrackingState, out Pose pose, out string anchorName)
		{
			anchorTrackingState = SpatialAnchorTrackingState.Stopped;
			pose = Pose.identity;
			anchorName = null;

			var wvrResult = GetSpatialAnchorState(anchorHandle, originModel, out var anchorState);
			if (wvrResult != WVR_Result.WVR_Success) return wvrResult;

			anchorTrackingState = (SpatialAnchorTrackingState)anchorState.trackingState;
			Coordinate.GetVectorFromGL(anchorState.pose.position, out pose.position);
			Coordinate.GetQuaternionFromGL(anchorState.pose.rotation, out pose.rotation);

			anchorName = anchorState.anchorName.FromSpatialAnchorName();

			return wvrResult;
		}

		public WVR_Result GetSpatialAnchorState(UInt64 anchorHandle, WVR_PoseOriginModel originModel, out SpatialAnchorTrackingState anchorTrackingState, out Pose pose, out string anchorName, Pose trackingOriginPose)
		{
			anchorTrackingState = SpatialAnchorTrackingState.Stopped;
			pose = Pose.identity;
			anchorName = null;

			var wvrResult = GetSpatialAnchorState(anchorHandle, originModel, out var anchorState);
			if (wvrResult != WVR_Result.WVR_Success) return wvrResult;

			anchorTrackingState = (SpatialAnchorTrackingState)anchorState.trackingState;
			ApplyTrackingOriginCorrectionToAnchorPose(trackingOriginPose, anchorState, out pose.position, out pose.rotation);

			anchorName = anchorState.anchorName.FromSpatialAnchorName();

			return wvrResult;
		}

		public Pose GetTrackingOriginPose()
		{
			return new Pose(trackingOrigin.transform.position, trackingOrigin.transform.rotation);
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
		public void ApplyTrackingOriginCorrectionToAnchorPose(Pose trackingOriginPose, WVR_SpatialAnchorState anchorState, out Vector3 anchorPosition, out Quaternion anchorRotation)
		{
			Coordinate.GetVectorFromGL(anchorState.pose.position, out anchorPosition);
			Coordinate.GetQuaternionFromGL(anchorState.pose.rotation, out anchorRotation);

			if (trackingOrigin != null)
			{
				Matrix4x4 trackingSpaceOriginTRS = Matrix4x4.TRS(trackingOriginPose.position, trackingOriginPose.rotation, Vector3.one);
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
		/// 
		public static bool AnchorStatePoseEqual(WVR_SpatialAnchorState anchorState1, WVR_SpatialAnchorState anchorState2)
		{
			return anchorState1.pose == anchorState2.pose;
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

		/// <summary>
		/// Get VR's <see cref="TrackingOriginModeFlags"/>.
		/// </summary>
		/// <returns>
		/// The <see cref="TrackingOriginModeFlags"/> in respect to the current <see cref="TrackingOriginModeFlags"/> in Unity.
		/// </returns>
		public static TrackingOriginModeFlags GetTrackingOriginModeFlags()
		{
			XRInputSubsystem subsystem = Utils.InputSubsystem;
			if (subsystem != null)
			{
				return subsystem.GetTrackingOriginMode();
			}

			return TrackingOriginModeFlags.Device;
		}

		// This class is only used for test in editor
		public class ScenePerceptionTestObject
		{
			public Vector3 position;  // In GL Convension
			public Quaternion rotation;  // In GL Convension
			public Vector3 scale;
			public Mesh mesh;
			public WVR_ScenePerceptionTarget type;
		}

		public void SetFakeData(ScenePerceptionTestObject fakeData)
		{
			if (fakeData == null) return;
			switch (fakeData.type)
			{
				case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane:
					spInternal.fake2DPlane = fakeData;
					break;
				case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject:
					spInternal.fake3DObject = fakeData;
					break;
				case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh:
					spInternal.fakeSceneMesh = fakeData;
					break;
			}
			spInternal.useFakeData = true;
		}

		/// <summary>
		/// <para>
		/// This function performs World Alignment for two players in a virtual reality environment, 
		/// using a shared Persist Anchor's pose as a reference point.
		/// The primary objective is to replicate the rigid spatial relationship between the two players and the Persist Anchor in the virtual world,
		/// mirroring their relative positions and orientations in the physical world.
		/// This ensures that the spatial arrangement and orientation of players relative to the Persist Anchor in the real world
		/// are accurately represented in the virtual environment, maintaining a consistent and unified spatial experience.
		/// </para>
		/// 
		/// <para>
		/// The diagram below illustrates the relationship between different points and vectors in the virtual world:
		///   World        Persist
		///   Center        Anchor
		///     W------d------A
		///      \ .         /|
		///       \  r2     / |
		///       r1   .  a1  a2
		///         \    ./   |
		///          \   / .  |
		///           \ /    .|
		///        P1  *      * P2
		///     CameraRig    CameraRig
		/// </para>
		/// 
		/// <para>
		/// Here's a breakdown of the symbols and terms used:
		/// - W represents the world center or reference point in the virtual world.
		/// - A represents the Persist Anchor.  It will be shared to all players.
		/// - d is the tranfrom from the world center W to the Persist Anchor A.
		/// - r1 and r2 represent the transforms from the world center W to the CameraRigs of Player1 (P1) and Player2 (P2), respectively.
		/// - a1 and a2 represent the pose from the Persist Anchor A to the CameraRigs of P1 and P2.
		/// - The dot "." line represent a line of the P2's CameraRigs to the world center.
		/// </para>
		/// 
		/// <para>
		/// Calculating r2 is crucial for aligning the virtual worlds between two players. It ensures that:
		/// - Both players have their CameraRigs positioned relative to the same Persist Anchor, 
		///   allowing for a shared point of reference in the virtual world.
		/// - The relative positions and orientations of the players' CameraRigs are adjusted 
		///   to maintain the perception of being in the same real world space, 
		///   which is essential for collaborative or competitive interactions within the VR environment.
		/// </para>
		/// 
		/// <para>
		/// The process involves using the known transform 'd' of player 1 to calculate
		/// the correct position and orientation for the other player's CameraRig in the virtual world.
		/// This is achieved through matrix transformations and inverse calculations in the script.
		/// </para>
		/// 
		/// <code>
		/// // example code:
		/// // In Player 1
		/// // Get Player1's CameraRig to world reference point's transform matrix.
		/// var r1 = p1WorldRefTranfrom.localToWorldMatrix.inverse * p1RigTransform.localToWorldMatrix;
		/// // Get pose from spatial anchor or the pose where persist anchor's created
		/// ScenePerceptionManager.GetSpatialAnchorState(anchorHandle, ScenePerceptionManager.GetCurrentPoseOriginModel(), out _, out Pose p1AnchorPose, out _);
		/// // Align naming.
		/// var p1A2RPosition = p1AnchorPose.position;
		/// var p1A2RRotation = p1AnchorPose.rotation;
		/// // Anchor pose should not have a scale other than Vector3.one to camera rig.
		/// Matrix4x4 a1 = Matrix4x4.TRS(p1A2RPosition, p1A2RRotation, Vector3.one);
		/// Matrix4x4 d = r1 * a1;
		/// var p1A2WPosition = d.GetColumn(3);
		/// var p1A2WRotation = d.rotation;
		/// var p1A2WScale = d.lossyScale;
		/// // Send p1A2WPosition, p1A2WRotation, p1A2WScale and Persisted Anchor exported data to other player
		/// 
		/// // In Player 2
		/// // Import persist anchor PA and create spatial anchor A from PA.
		/// ScenePerceptionManager.GetSpatialAnchorState(A, ScenePerceptionManager.GetCurrentPoseOriginModel(), out _, out Pose p2AnchorPose, out _);
		/// // Align naming
		/// var p2A2RPosition = p2AnchorPose.position;
		/// var p2A2RRotation = p2AnchorPose.rotation;
		/// // Calculate new camera rig's pose.
		/// ScenePerceptionManager.AlignWorld(p1A2WPosition, p1A2WRotation, p1A2WScale, p2A2RPosition, p2A2RRotation, out Vector3 p2R2WPosition, out Quaternion p2R2WRotation, out Vector3 p2R2WScale);
		/// Matrix4x4 p2R2W = Matrix4x4.TRS(p2R2WPosition, p2R2WRotation, p2R2WScale);
		/// Matrix4x4 p2LocalToWorld = p2WorldRefTranfrom.localToWorldMatrix * p2R2W;
		/// p2Transform.position = p2LocalToWorld.GetColumn(3);
		/// p2Transform.rotation = p2LocalToWorld.rotation;
		/// p2Transform.scale = p2LocalToWorld.lossyScale;
		/// </code>
		/// </summary>
		/// <param name="p1A2WPosition">The position of Player 1's Anchor to world reference point (The world root or a network shared transform)</param>
		/// <param name="p1A2WRotation">The rotation of Player 1's Anchor to world reference point (The world root or a network shared transform)</param>
		/// <param name="p1A2WScale">   The scale    of Player 1's Anchor to world reference point (The world root or a network shared transform)</param>
		/// <param name="p2A2RPosition">The position of Player 2's Anchor to Player 2's CameraRig</param>
		/// <param name="p2A2RRotation">The position of Player 2's Anchor to Player 2's CameraRig</param>
		/// <param name="p2R2WPosition">The position of Player 2's CameraRig to Player 2's world reference point (The world root or a network shared transform)</param>
		/// <param name="p2R2WRotation">The rotation of Player 2's CameraRig to Player 2's world reference point (The world root or a network shared transform)</param>
		/// <param name="p2R2WScale">   The scale    of Player 2's CameraRig to Player 2's world reference point (The world root or a network shared transform)</param>
		public static void AlignWorld(Vector3 p1A2WPosition, Quaternion p1A2WRotation, Vector3 p1A2WScale, Vector3 p2A2RPosition, Quaternion p2A2RRotation, out Vector3 p2R2WPosition, out Quaternion p2R2WRotation, out Vector3 p2R2WScale)
		{
			// Get d, the world pose of player0's anchor
			//var a1 = Matrix4x4.TRS(p1A2RPosition, p1A2RRotation, Vector3.one);  // Anchor pose should not have a scale other than Vector3.one to camera rig.
			//var r1 = Matrix4x4.TRS(p1R2WPosition, p1R2WRotation, p1R2WScale);
			//var d = r1 * a1;
			Matrix4x4 d = Matrix4x4.TRS(p1A2WPosition, p1A2WRotation, p1A2WScale);

			// Get player1's Persist Anchor related to player1's camera rig.
			Matrix4x4 a2 = Matrix4x4.TRS(p2A2RPosition, p2A2RRotation, Vector3.one);  // Anchor pose Should not have a scale other than Vector3.one to camera rig.

			// Calculate player2 camera rig's pose
			//var r2 = d * a2.inverse;
			AlignWorld(d, a2, out Matrix4x4 r2);

			// Set Pose
			p2R2WPosition = r2.GetColumn(3);
			p2R2WRotation = r2.rotation;
			p2R2WScale = r2.lossyScale;
		}

		/// <summary>
		/// See <see cref="AlignWorld(Vector3, Quaternion, Vector3, Vector3, Quaternion, out Vector3, out Quaternion, out Vector3)"/>
		/// </summary>
		/// <param name="p1A2W">The matrix of Player 1's Anchor to world reference point (The world root or a network shared transform)</param>
		/// <param name="p2A2R">The matrix of Player 2's Anchor to Player 2's CameraRig<</param>
		/// <param name="p2R2W">The new matrix of Player 2's CameraRig to Player 2's world reference point (The world root or a network shared transform)</param>
		public static void AlignWorld(Matrix4x4 p1A2W, Matrix4x4 p2A2R, out Matrix4x4 p2R2W)
		{
			// Calculate player2 camera rig's pose
			// r2 = d * a2.inverse;
			p2R2W = p1A2W * p2A2R.inverse;
		}

		[Obsolete]
		public static class MeshGenerationHelper
		{
			[Obsolete]
			public static Mesh GenerateMeshFromWVRMeshData(WVR_Vector3f_t[] wvrVertices, UInt32[] wvrIndices)
			{
				Vector3[] unityVertices = new Vector3[wvrVertices.Length];
				int[] unityIndices = new int[wvrIndices.Length];
				for (int i = 0; i < wvrVertices.Length; i++)
				{
					Coordinate.GetVectorFromGL(wvrVertices[i], out unityVertices[i]);
				}

				// Unity is using clockwise winding order for front face.
				if (wvrIndices.Length % 3 != 0)
					Log.d(TAG, "MeshTools: Indices is not 3's multiple!");
				int tc = wvrIndices.Length / 3;  // Triangle Count
				for (int i = 0; i < tc; i++)
				{
					var j = i * 3;
					unityIndices[j] = (int)wvrIndices[j];
					unityIndices[j + 1] = (int)wvrIndices[j + 2]; // Index order need be reversed in Unity
					unityIndices[j + 2] = (int)wvrIndices[j + 1];
				}

				return MeshTools.GenerateMeshDataFromTriangle(unityVertices, unityIndices);
			}
		}
	}

	internal class UnityToWVRConversionHelper
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

		internal static WVR_Extent3Df GetWVRExtent3D(Bounds bounds)
		{
			WVR_Extent3Df extent = new WVR_Extent3Df();
			var s = bounds.size;
			extent.width = s.x;
			extent.height = s.y;
			extent.depth = s.z;
			return extent;
		}

		internal static WVR_Extent2Df GetWVRExtent2D(Bounds bounds)
		{
			WVR_Extent2Df extent = new WVR_Extent2Df();
			var s = bounds.size;
			extent.width = s.x;
			extent.height = s.y;
			return extent;
		}
	}

	[Obsolete]
	public static class AnchorExtensions
	{
		[Obsolete("Use ToString() instead")]
		public static string FromSpatialAnchorName(this WVR_SpatialAnchorName wvrName)
		{
			return wvrName.ToString();
		}

		[Obsolete("Use new WVR_SpatialAnchorName(string) instead")]
		public static WVR_SpatialAnchorName ToSpatialAnchorName(this string name)
		{
			return new WVR_SpatialAnchorName(name);
		}
	}
}
