using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
    [Serializable]
    public class ScenePerceptionHelper
    {
		public ScenePerceptionManager scenePerceptionManager = null;
        public bool isSceneComponentRunning { get; private set; } //True after StartScene() is called successfully.
		public bool isScenePerceptionStarted { get; private set; } //True after StartScenePerception() is called successfully.
        
        private Dictionary<WVR_ScenePerceptionTarget, WVR_ScenePerceptionState> perceptionStateDictionary = new Dictionary<WVR_ScenePerceptionTarget, WVR_ScenePerceptionState>();
        public WVR_ScenePerceptionTarget currentPerceptionTarget = WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane;

		private const string LOG_TAG = "ScenePerceptionHelper";

		public void OnEnable()
        {
            if(!IsScenePerceptionSupported())
            {
				Log.e(LOG_TAG, "ScenePerception Not Supported");
                throw new Exception("Scene Perception is not supported on this device"); 
            }

			SceneMeshPermissionHelper.RequestSceneMeshPermission();

            WVR_Result result = scenePerceptionManager.StartScene();
            if (result == WVR_Result.WVR_Success)
            {
                isSceneComponentRunning = true;
            }
        }

        public void OnDisable()
        {
            if (!isSceneComponentRunning) return;

			StopScenePerception();

			scenePerceptionManager.StopScene();
			isSceneComponentRunning = false;
        }

		public void StartScenePerception()
		{
			if (isSceneComponentRunning && !isScenePerceptionStarted)
			{
				if (currentPerceptionTarget == WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh && !SceneMeshPermissionHelper.permissionGranted)
				{
					Log.e(LOG_TAG, "Scene Mesh Permission not granted, cannot not start scene perception with scene mesh as perception target.");
				}

				WVR_Result result = scenePerceptionManager.StartScenePerception(currentPerceptionTarget);

				if (result == WVR_Result.WVR_Success)
				{
					isScenePerceptionStarted = true;
					ScenePerceptionGetState();
				}
			}
		}

		public void StopScenePerception()
		{
			if (isSceneComponentRunning && isScenePerceptionStarted)
			{
				WVR_Result result = scenePerceptionManager.StopScenePerception(currentPerceptionTarget);

				if (result == WVR_Result.WVR_Success)
				{
					isScenePerceptionStarted = false;
				}
			}
		}

		public void ScenePerceptionGetState()
        {
            WVR_ScenePerceptionState latestPerceptionState = WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
            WVR_Result result = scenePerceptionManager.GetScenePerceptionState(currentPerceptionTarget, ref latestPerceptionState);
            if (result == WVR_Result.WVR_Success)
            {
                perceptionStateDictionary[currentPerceptionTarget] = latestPerceptionState; //Update perception state for the perception target
            }
        }
        
        private bool IsScenePerceptionSupported()
        {
            return (Interop.WVR_GetSupportedFeatures() &
                    (ulong) WVR_SupportedFeature.WVR_SupportedFeature_ScenePerception) != 0;
        }
		

		public bool CurrentPerceptionTargetIsCompleted()
        {
            return perceptionStateDictionary[currentPerceptionTarget] == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Completed;
        }

		public bool CurrentPerceptionTargetIsEmpty()
		{
			return perceptionStateDictionary[currentPerceptionTarget] == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
		}
	}
}
