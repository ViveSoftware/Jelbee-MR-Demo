
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.ScenePerception
{
	public class ScenePerceptionObjectTools
	{
		private static string TAG = "ScenePerceptionObjectTools";

		/// <summary>
		/// A helper function to create a GameObject in world space with Mesh related components for showing scene planes in the scene.
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
		/// <param name="trackingOrigin">
		/// Pass the tracking origin, for example, WaveRig's transform.  This will be used to convert the scene
		/// plane pose from tracking space to world space.  If null, the scene plane's local pose will be returned
		/// in tracking space.
		/// </param>
		/// <returns>
		/// A GameObject with all the necessary Mesh related components for rendering a scene plane in the scene.
		/// </returns>
		public static GameObject GenerateScenePlaneMesh(ScenePlane scenePlane, Material meshMaterial, bool attachMeshCollider = false, Transform trackingOrigin = null) //use pose and extend to generate planes
		{
			var pose = scenePlane.pose;
			if (trackingOrigin != null) //Apply origin correction to the anchor pose
				TrackingSpaceToWorldSpace(trackingOrigin, scenePlane.pose.position, scenePlane.pose.rotation, out pose.position, out pose.rotation);

			//Log.d(LOG_TAG, "GenerateScenePlaneMesh Position: " + planePositionUnity.ToString());
			//Log.d(LOG_TAG, "GenerateScenePlaneMesh Rotation (Euler): " + planeRotationUnity.eulerAngles.ToString());

			Mesh planeMesh = MeshTools.GenerateQuadMesh();
			// Scale the Quad mesh to plane size.
			var scale = new Vector3(scenePlane.extent.x, scenePlane.extent.y, 1);
			var vs = planeMesh.vertices;
			for (int i = 0; i < planeMesh.vertices.Length; i++)
				vs[i] = Vector3.Scale(vs[i], scale);
			planeMesh.vertices = vs;

			GameObject planeMeshGameObject = new GameObject();

			planeMeshGameObject.transform.position = pose.position;
			planeMeshGameObject.transform.rotation = pose.rotation;

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
		/// A helper function to create a GameObject in world space with Mesh related components for showing the 
		/// extents box of scene objects in the scene.
		/// </summary>
		/// <param name="sceneObject">
		/// The scene object to be created.
		/// </param>
		/// <param name="meshMaterial">
		/// The <see cref="Material"/> to be applied to the <see cref="MeshRenderer"/>.
		/// </param>
		/// <param name="attachMeshCollider">
		/// Specify whether or not a mesh collider should be attached to the generated mesh.
		/// Set to false by default.
		/// </param>
		/// <param name="trackingOrigin">
		/// Pass the tracking origin, for example, WaveRig's transform.  This will be used to convert the scene
		/// plane pose from tracking space to world space.  If null, the scene plane's local pose will be returned
		/// in tracking space.
		/// </param>
		/// <returns>
		/// A GameObject with all the necessary Mesh related components for rendering a scene object in the scene.
		/// </returns>
		public static GameObject GenerateSceneObjectExtentMesh(SceneObject sceneObject, Material meshMaterial, bool attachMeshCollider = false, Transform trackingOrigin = null) //use pose and extend to generate objects
		{
			var pose = sceneObject.pose;
			if (trackingOrigin != null)
				TrackingSpaceToWorldSpace(trackingOrigin, sceneObject.pose.position, sceneObject.pose.rotation, out pose.position, out pose.rotation);

			GameObject objectMeshGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			objectMeshGameObject.transform.position = pose.position;
			objectMeshGameObject.transform.rotation = pose.rotation;

			MeshRenderer objectMeshRenderer = objectMeshGameObject.GetComponent<MeshRenderer>();
			MeshFilter filter = objectMeshGameObject.GetComponent<MeshFilter>();
			Mesh mesh = filter.mesh;

			// Do Scale first, then Rotation, then Translation
			var scale = sceneObject.extent;
			var matScale = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

			var vertices = mesh.vertices;
			for (int i = 0; i < vertices.Length; i++)
				vertices[i] = matScale.MultiplyPoint3x4(vertices[i]);
			mesh.vertices = vertices;

			objectMeshRenderer.material = meshMaterial;

			if (!attachMeshCollider)
			{
				if (objectMeshGameObject.TryGetComponent<BoxCollider>(out var collider))
				{
					collider.enabled = false;
					collider.center = Vector3.zero;
					collider.size = sceneObject.extent;
				}
			}
			else
			{
				if (objectMeshGameObject.TryGetComponent<BoxCollider>(out var collider))
				{
					collider.center = Vector3.zero;
					collider.size = sceneObject.extent;
				}
			}

			Log.d(TAG, Log.CSB.Append("GenerateSceneObjectExtentMesh ").AppendVector3("pos", pose.position).AppendVector3(", rot", pose.rotation.eulerAngles).AppendVector3(", sz=", scale).Append(", sem=").Append(sceneObject.semanticName).Append(", uuid=").Append(sceneObject.uuid));

			return objectMeshGameObject;
		}

		/// <summary>
		///  A helper function to help create a GameObject with Mesh related components for showing scene meshes in the scene.
		/// </summary>
		/// <param name="manager">
		/// Used to get the scene mesh buffer.
		/// </param>
		/// <param name="sceneObject">
		/// The scene object to be created.
		/// </param>
		/// <param name="meshMaterial">
		/// The <see cref="Material"/> to be applied to the <see cref="MeshRenderer"/>.
		/// </param>
		/// <param name="attachMeshCollider">
		/// Specify whether or not a mesh collider should be attached to the generated mesh.
		/// Set to false by default.
		/// </param>
		/// <param name="trackingOrigin">
		/// Pass the tracking origin, for example, WaveRig's transform.  This will be used to convert the scene
		/// plane pose from tracking space to world space.  If null, the scene plane's local pose will be returned
		/// in tracking space.
		/// </param>
		/// <returns>
		/// A GameObject with all the necessary Mesh related components for rendering a scene mesh in the scene.
		/// </returns>
		public static GameObject GenerateSceneObjectMesh(ScenePerceptionManager manager, SceneObject sceneObject, Material meshMaterial, bool attachMeshCollider = false, Transform trackingOrigin = null)
		{

			Vector3[] vertices;
			int[] indices;

			if (manager.GetSceneMeshBuffer(sceneObject.meshBufferId, out vertices, out indices) != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "GenerateSceneObjectMesh: Failed to get scene mesh buffer.");
				return null;
			}

			// For debug
			//if (wvrVertices.Length == 8)
			//{
			//	var v = Vector3.zero;
			//	var sb = Log.CSB.Append("GenerateSceneObjectMesh: the runtime returned vertices").AppendLine();
			//	for (int i = 0; i < 8; i++)
			//	{
			//		v.x = wvrVertices[i].v0;
			//		v.y = wvrVertices[i].v1;
			//		v.z = wvrVertices[i].v2;
			//		sb.Append("  ").AppendVector3("v" + i, v).AppendLine();
			//	}

			//	Log.d(TAG, sb, true);
			//}

			var pose = sceneObject.pose;
			if (trackingOrigin != null) //Apply origin correction to the anchor pose
				TrackingSpaceToWorldSpace(trackingOrigin, sceneObject.pose.position, sceneObject.pose.rotation, out pose.position, out pose.rotation);

			Mesh mesh = MeshTools.GenerateMeshDataFromTriangle(vertices, indices);

			GameObject obj = new GameObject();
			obj.transform.position = pose.position;
			obj.transform.rotation = pose.rotation;

			MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
			MeshFilter filter = obj.AddComponent<MeshFilter>();

			filter.mesh = mesh;
			renderer.material = meshMaterial;

			if (attachMeshCollider)
			{
				obj.AddComponent<MeshCollider>();
			}

			return obj;
		}

		/// <summary>
		///  A helper function to help create a GameObject with Mesh related components for showing scene meshes in the scene.
		/// </summary>
		/// <param name="manager">
		/// Used to get the scene mesh buffer.
		/// </param>
		/// <param name="sceneMesh">
		/// The scene mesh to be created.
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
		public static GameObject GenerateSceneMesh(ScenePerceptionManager manager, SceneMesh sceneMesh, Material meshMaterial, bool attachMeshCollider = false, Transform trackingOrigin = null)
		{
			Vector3[] vertices;
			int[] indices;

			if (manager.GetSceneMeshBuffer(sceneMesh.meshBufferId, out vertices, out indices) != WVR_Result.WVR_Success)
			{
				Log.e(TAG, "GenerateSceneMesh: Failed to get scene mesh buffer.");
				return null;
			}
			var pos = Vector3.zero;
			var rot = Quaternion.identity;

			Mesh generatedSceneMesh = MeshTools.GenerateMeshDataFromTriangle(vertices, indices);
			if (trackingOrigin != null) //Apply origin correction to the anchor pose
				TrackingSpaceToWorldSpace(trackingOrigin, Vector3.zero, Quaternion.identity, out pos, out rot);


			GameObject sceneMeshGameObject = new GameObject();
			sceneMeshGameObject.transform.position = pos;
			sceneMeshGameObject.transform.rotation = rot;

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
		/// A helper function which turn tracking space pose to world space pose according to the tracking origin..
		/// </summary>
		/// <param name="trackingOrigin">
		/// The tracking origin reference.
		/// </param>
		/// <param name="trackingPos">
		/// The position from tracking space.
		/// </param>
		/// <param name="trackingRot">
		/// The rotation from tracking space.
		/// </param>
		/// <param name="worldPos">
		/// The converted position in world space.
		/// </param>
		/// <param name="worldRot">
		/// The converted rotation in world space.
		/// </param>
		/// <returns>if trackingOrigin is null, return false.</returns>
		public static bool TrackingSpaceToWorldSpace(Transform trackingOrigin, Vector3 trackingPos, Quaternion trackingRot, out Vector3 worldPos, out Quaternion worldRot)
		{
			worldPos = Vector3.zero;
			worldRot = Quaternion.identity;
			if (trackingOrigin == null) return false;
			worldPos = trackingOrigin.TransformPoint(trackingPos);
			worldRot = trackingOrigin.rotation * trackingRot;
			return true;
		}

		/// <summary>
		/// A helper function which turn world space pose to tracking space pose according to the tracking origin..
		/// </summary>
		/// <param name="trackingOrigin">
		/// The tracking origin reference.
		/// </param>
		/// <param name="worldPos">
		/// The converted position in world space.
		/// </param>
		/// <param name="worldRot">
		/// The converted rotation in world space.
		/// </param>
		/// <param name="trackingPos">
		/// The position from tracking space.
		/// </param>
		/// <param name="trackingRot">
		/// The rotation from tracking space.
		/// </param>
		/// <returns>if trackingOrigin is null, return false.</returns>
		public static bool WorldSpaceToTrackingSpace(Transform trackingOrigin, Vector3 worldPos, Quaternion worldRot, out Vector3 trackingPos, out Quaternion trackingRot)
		{
			trackingPos = Vector3.zero;
			trackingRot = Quaternion.identity;
			if (trackingOrigin == null) return false;
			trackingPos = trackingOrigin.InverseTransformPoint(worldPos);
			trackingRot = Quaternion.Inverse(trackingOrigin.rotation) * worldRot;
			return true;
		}
	}
}
