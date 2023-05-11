using com.HTC.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;

public class ScenePerceptionUtility : Singleton<ScenePerceptionUtility>
{
	[SerializeField]
	private ScenePerceptionManager scenePerceptionMgr = null;

	private const string LOG_TAG = "ScenePerceptionUtility";
	private bool isRunning = false;

	private void OnEnable()
	{
		//Check whether feature is supported on device or not
		if ((Interop.WVR_GetSupportedFeatures() & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_ScenePerception) != 0)
		{
			WVR_Result result = scenePerceptionMgr.StartScene();
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, $"StartScene Error: {result}");
				return;
			}

			result = scenePerceptionMgr.StartScenePerception(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, $"StartScenePerception Error: {result}");
				return;
			}

			isRunning = true;
		}
		else
		{
			Log.e(LOG_TAG, "Scene Perception is not available on the current device.");
		}
	}

	private void OnDisable()
	{
		if (isRunning)
		{
			scenePerceptionMgr.StopScene();
			isRunning = false;
		}
	}

	public List<Mesh> LoadVisualMeshes()
    {
		if (isRunning)
		{
			return loadSceneMeshes(WVR_SceneMeshType.WVR_SceneMeshType_VisualMesh);
		}
		else
        {
			Log.e(LOG_TAG, "Scene Perception is not available on the current device. Load empty meshes");
			return null;
		}
	}
	public List<Mesh> LoadColliderMesh()
    {
		if (isRunning)
		{
			return loadSceneMeshes(WVR_SceneMeshType.WVR_SceneMeshType_ColliderMesh);
		}
		else
		{
			Log.e(LOG_TAG, "Scene Perception is not available on the current device. Load empty meshes");
			return null;
		}
	}

    private List<Mesh> loadSceneMeshes(WVR_SceneMeshType meshType)
    {
		WVR_ScenePerceptionState latestPerceptionState = WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
		WVR_Result result = scenePerceptionMgr.GetScenePerceptionState(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh, ref latestPerceptionState);
		
		if (result != WVR_Result.WVR_Success)
		{
			Log.e(LOG_TAG, $"GetScenePerceptionState error: {result}");
			return null;
		}

		WVR_SceneMesh[] sceneMeshes;
		result = scenePerceptionMgr.GetSceneMeshes(meshType, out sceneMeshes);

		if (result != WVR_Result.WVR_Success)
		{
			Log.e(LOG_TAG, $"GetSceneMeshes error: {result}");
			return null;
		}

		List<Mesh> meshes = new List<Mesh>(); 
		foreach(WVR_SceneMesh sceneMesh in sceneMeshes)
        {
			GameObject sceneMeshGO = scenePerceptionMgr.GenerateSceneMesh(sceneMesh, null);

			if(sceneMeshGO)
            {
				MeshFilter meshFilter = sceneMeshGO.GetComponent<MeshFilter>();
				if(meshFilter && meshFilter.mesh)
                {
					meshes.Add(meshFilter.mesh);
				}
            }

			Destroy(sceneMeshGO);
		}

		return meshes;
	}
}
