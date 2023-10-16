using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
    [Serializable]
    public class ScenePerceptionHelper
    {
		public enum SceneTarget
		{
			TwoDimensionPlane = WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane,
			ThreeDimensionObject = WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject,
			SceneMesh = WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh,
		}

		public ScenePerceptionManager scenePerceptionManager = null;
		public bool IsSceneStarted { get; private set; } //True after StartScene() is called successfully.

		private readonly List<bool> perceptionStartedDictionary = new List<bool>(3) { false, false, false };
		private readonly List<WVR_ScenePerceptionState> perceptionStateDictionary =
			new List<WVR_ScenePerceptionState>(3) {
				WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty,
				WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty,
				WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty, };

		public SceneTarget target = SceneTarget.TwoDimensionPlane;

		private const string TAG = "ScenePerceptionHelper";
		LogPanel logPanel;
		MonoBehaviour context;

		// LogD is thread safe
		void LogD(string log)
		{
			if (logPanel != null) logPanel.AddLog(log);
			Log.d(TAG, log);
		}

		// LogE is thread safe
		void LogE(string log)
		{
			if (logPanel != null) logPanel.AddLog("E " + log);
			Log.e(TAG, log);
		}

		public void Init(MonoBehaviour p, LogPanel logPanel)
		{
			context = p;
			this.logPanel = logPanel;
		}

		public void OnEnable()
		{
			if (!IsScenePerceptionSupported())
			{
				LogE("ScenePerception Not Supported");
				throw new Exception("Scene Perception is not supported on this device"); 
			}

			SceneMeshPermissionHelper.RequestSceneMeshPermission();

			WVR_Result result = scenePerceptionManager.StartScene();
			if (result == WVR_Result.WVR_Success)
			{
				IsSceneStarted = true;
			}
		}

		public void OnDisable()
		{
			if (!IsSceneStarted) return;
			StopScenePerception();
			scenePerceptionManager.StopScene();
		}

		public void StartScenePerception(SceneTarget target)
		{
			if (IsSceneStarted && !perceptionStartedDictionary[(int)target])
			{
				if (target == SceneTarget.SceneMesh && !SceneMeshPermissionHelper.permissionGranted)
				{
					LogE("Scene Mesh Permission not granted, cannot not start scene perception with scene mesh as perception target.");
				}

				WVR_Result result = scenePerceptionManager.StartScenePerception((WVR_ScenePerceptionTarget)target);

				if (result == WVR_Result.WVR_Success)
				{
					perceptionStartedDictionary[(int)target] = true;
					ScenePerceptionGetState(target);
				}
			}
		}


		public void StopScenePerception(SceneTarget target)
		{
			if (IsSceneStarted && perceptionStartedDictionary[(int)target])
			{
				WVR_Result result = scenePerceptionManager.StopScenePerception((WVR_ScenePerceptionTarget)target);

				if (result == WVR_Result.WVR_Success)
				{
					perceptionStartedDictionary[(int)target] = false;
				}
			}
		}

		public void StopScenePerception()
		{
			StopScenePerception(SceneTarget.TwoDimensionPlane);
			StopScenePerception(SceneTarget.SceneMesh);
			StopScenePerception(SceneTarget.ThreeDimensionObject);
		}

		public void ScenePerceptionGetState(SceneTarget target)
		{
			WVR_ScenePerceptionState latestPerceptionState = WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
			WVR_Result result = scenePerceptionManager.GetScenePerceptionState((WVR_ScenePerceptionTarget)target, ref latestPerceptionState);
			if (result == WVR_Result.WVR_Success)
			{
				perceptionStateDictionary[(int)target] = latestPerceptionState; //Update perception state for the perception target
			}
		}

		private bool IsScenePerceptionSupported()
		{
#if UNITY_EDITOR
			if (Application.isEditor)
				return true;
#endif
			return (Interop.WVR_GetSupportedFeatures() &
					(ulong) WVR_SupportedFeature.WVR_SupportedFeature_ScenePerception) != 0;
		}

		public bool IsStarted(SceneTarget target)
		{
			return perceptionStartedDictionary[(int)target];
		}

		public bool StateIsCompleted(SceneTarget target)
		{
			return perceptionStateDictionary[(int)target] == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Completed;
		}

		public bool StateIsEmpty(SceneTarget target)
		{
			return perceptionStateDictionary[(int)target] == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
		}
	}
}
