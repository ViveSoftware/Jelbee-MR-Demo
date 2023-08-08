using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence;
using Wave.Native;

public static class SceneMeshPermissionHelper
{
	public static bool permissionGranted { get; private set; } = false;

	private static PermissionManager pmInstance = null;
	private const string scenePerceptionPermissionString = "wave.permission.GET_SCENE_MESH";

	private const string LOG_TAG = "SceneMeshPermissionHelper";

	public static void RequestSceneMeshPermission()
	{
		Log.d(LOG_TAG, "Request Scene Mesh Permission");
		string[] permArray = {
			   scenePerceptionPermissionString
			};

		pmInstance = PermissionManager.instance;

		pmInstance?.requestPermissions(permArray, requestDoneCallback);
	}
	private static void requestDoneCallback(List<PermissionManager.RequestResult> results)
	{
		foreach (PermissionManager.RequestResult permissionRequestResult in results)
		{
			if (permissionRequestResult.PermissionName.Equals(scenePerceptionPermissionString))
			{
				permissionGranted = permissionRequestResult.Granted;

				Log.d(LOG_TAG, "Scene Mesh permission granted = " + permissionGranted);
			}
		}
	}
}
