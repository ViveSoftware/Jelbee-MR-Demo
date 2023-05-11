using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
	public class GeneratedSceneMeshContainer : IDisposable
	{
		private readonly List<GeneratedSceneMesh> generatedSceneMeshes = new List<GeneratedSceneMesh>();

		private readonly ScenePerceptionManager scenePerceptionManager;
		private readonly Material generatedMeshMaterialWireframe;

		public WVR_SceneMeshType currentSceneMeshType = WVR_SceneMeshType.WVR_SceneMeshType_VisualMesh;

		private const string LOG_TAG = "GeneratedSceneMeshContainer";

		public GeneratedSceneMeshContainer(ScenePerceptionManager scenePerceptionManager, Material generatedMeshMaterialWireframe)
		{
			this.scenePerceptionManager = scenePerceptionManager ?? throw new ArgumentNullException(nameof(scenePerceptionManager));
			this.generatedMeshMaterialWireframe = generatedMeshMaterialWireframe ?? throw new ArgumentNullException(nameof(generatedMeshMaterialWireframe));
		}
		public void Dispose()
		{
			foreach (var sceneMesh in generatedSceneMeshes)
			{
				sceneMesh.Dispose();
			}
			generatedSceneMeshes.Clear();
		}

		private GeneratedSceneMesh FindGeneratedSceneMesh(ulong meshBufferId)
		{
			foreach (var generatedSceneMesh in generatedSceneMeshes)
			{
				if (generatedSceneMesh.sceneMesh.meshBufferId == meshBufferId)
				{
					return generatedSceneMesh;
				}
			}
			return null;
		}

		enum SceneMeshAction { NONE, ADD, REMOVE }
		IEnumerable<Tuple<SceneMeshAction, WVR_SceneMesh, GeneratedSceneMesh>> SceneMeshActionEnumerator()
		{
			WVR_Result result = scenePerceptionManager.GetSceneMeshes(currentSceneMeshType, out WVR_SceneMesh[] currentSceneMeshes);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "Failed to get scene meshes");
				yield break;
			}

			//Check if generated scene mesh still exsits
			List<int> sceneMeshIndexToRemove = new List<int>();
			for (int i = 0; i < generatedSceneMeshes.Count; i++)
			{
				bool sceneMeshExists = false;
				foreach (WVR_SceneMesh sceneMesh in currentSceneMeshes)
				{
					if (sceneMesh.meshBufferId == generatedSceneMeshes[i].sceneMesh.meshBufferId) //scene mesh still exists
					{
						sceneMeshExists = true;
						break;
					}
				}

				if (!sceneMeshExists)
				{
					sceneMeshIndexToRemove.Add(i);
				}
			}

			foreach (int index in sceneMeshIndexToRemove) //Remove all scene meshes that no longer exists
			{
				yield return new Tuple<SceneMeshAction, WVR_SceneMesh, GeneratedSceneMesh>(SceneMeshAction.REMOVE, default, generatedSceneMeshes[index]);
			}

			for (var index = 0; index < currentSceneMeshes.Length; index++)
			{
				WVR_SceneMesh currentSceneMesh = currentSceneMeshes[index];
				GeneratedSceneMesh generatedSceneMesh = FindGeneratedSceneMesh(currentSceneMesh.meshBufferId);
				if (generatedSceneMesh == null && currentSceneMesh.meshBufferId != 0)
				{
					yield return new Tuple<SceneMeshAction, WVR_SceneMesh, GeneratedSceneMesh>(SceneMeshAction.ADD, currentSceneMesh, null);
				}
				else
				{
					yield return new Tuple<SceneMeshAction, WVR_SceneMesh, GeneratedSceneMesh>(SceneMeshAction.NONE, currentSceneMesh, generatedSceneMesh);
				}
			}
		}

		//only call if scenePerceptionHelper.CurrentPerceptionTargetIsCompleted -- which was perceptionStateDictionary[currentPerceptionTarget] == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Completed
		public void UpdateAssumingThePerceptionTargetIsCompleted()
		{


			foreach (Tuple<SceneMeshAction, WVR_SceneMesh, GeneratedSceneMesh> sceneMeshAction in SceneMeshActionEnumerator())
			{
				SceneMeshAction action = sceneMeshAction.Item1;
				WVR_SceneMesh currentSceneMesh = sceneMeshAction.Item2;
				GeneratedSceneMesh generateSceneMesh = sceneMeshAction.Item3;

				switch (action)
				{
					case SceneMeshAction.ADD:
						//Log.d(LOG_TAG, "SceneMeshAction.ADD");
						GeneratedSceneMesh newGeneratedSceneMesh = NewGeneratedSceneMesh(currentSceneMesh);
						if (newGeneratedSceneMesh != null) generatedSceneMeshes.Add(newGeneratedSceneMesh);
						break;
					case SceneMeshAction.REMOVE:
						//Log.d(LOG_TAG, "SceneMeshAction.REMOVE");
						generatedSceneMeshes.Remove(generateSceneMesh);
						generateSceneMesh.Dispose();
						break;
					case SceneMeshAction.NONE:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
		private GeneratedSceneMesh NewGeneratedSceneMesh(WVR_SceneMesh sceneMesh)
		{
			//Log.d(LOG_TAG, "Add new scene mesh");
			GameObject newGeneratedSceneMeshGO = GenerateNewGameObject(sceneMesh);
			if (newGeneratedSceneMeshGO != null)
			{
				GeneratedSceneMesh newGeneratedSceneMesh = new GeneratedSceneMesh() { sceneMesh = sceneMesh, go = newGeneratedSceneMeshGO};
				return newGeneratedSceneMesh;
			}

			return null;

		}
		private GameObject GenerateNewGameObject(WVR_SceneMesh sceneMesh)
		{
			//Log.d(LOG_TAG, "Add new scene mesh");
			GameObject newSceneMeshGO = scenePerceptionManager.GenerateSceneMesh(sceneMesh, generatedMeshMaterialWireframe, false);

			if (newSceneMeshGO == null)
			{
				return null;
			}

			//Process Mesh For Wireframe rendering
			MeshFilter generatedSceneMeshFilter = newSceneMeshGO.GetComponent<MeshFilter>();
			if (generatedSceneMeshFilter && generatedSceneMeshFilter.mesh)
			{
				Mesh generatedSceneMeshInstance = generatedSceneMeshFilter.mesh;
				generatedSceneMeshFilter.mesh = ProcessSceneMeshForWireframe(generatedSceneMeshInstance);
			}

			return newSceneMeshGO;
		}

		private Mesh ProcessSceneMeshForWireframe(Mesh generatedSceneMesh)
		{
			int[] originalTriangles = generatedSceneMesh.triangles;
			Vector3[] originalVertices = generatedSceneMesh.vertices;
			Vector3[] originalNormals = generatedSceneMesh.normals;

			Mesh processedMesh = new Mesh();

			if (originalTriangles.Length >= 65535) //Check if processed vertex count is higher than 16 bit limit
			{
				processedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			int[] processedTriangles = new int[originalTriangles.Length];
			Vector3[] processedVertices = new Vector3[originalTriangles.Length];
			Vector3[] processedNormals = new Vector3[originalTriangles.Length];
			Vector2[] processedUVs = new Vector2[originalTriangles.Length];

			for (var i = 0; i < originalTriangles.Length; i += 3)
			{
				processedVertices[i] = originalVertices[originalTriangles[i]];
				processedVertices[i + 1] = originalVertices[originalTriangles[i + 1]];
				processedVertices[i + 2] = originalVertices[originalTriangles[i + 2]];

				processedUVs[i] = new Vector2(0f, 0f);
				processedUVs[i + 1] = new Vector2(1f, 0f);
				processedUVs[i + 2] = new Vector2(0f, 1f);

				processedTriangles[i] = i;
				processedTriangles[i + 1] = i + 1;
				processedTriangles[i + 2] = i + 2;

				processedNormals[i] = originalNormals[originalTriangles[i]];
				processedNormals[i + 1] = originalNormals[originalTriangles[i + 1]];
				processedNormals[i + 2] = originalNormals[originalTriangles[i + 2]];
			}

			processedMesh.vertices = processedVertices;
			processedMesh.triangles = processedTriangles;
			processedMesh.normals = processedNormals;
			processedMesh.uv = processedUVs;

			return processedMesh;
		}
	}
}
