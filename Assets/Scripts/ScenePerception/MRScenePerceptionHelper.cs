using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;

[Serializable]
public class MRScenePerceptionHelper
{
    public ScenePerceptionManager scenePerceptionManager = null;

    WVR_ScenePerceptionState planePerceptionState;
    public bool isSceneComponentRunning { get; private set; } = false;//True after StartScene() is called successfully.
    public bool isScenePerceptionStarted { get; private set; } = false; //True after StartScenePerception() is called successfully.

    private const string LOG_TAG = "JelbeeMR";

    public MRScenePerceptionHelper(ScenePerceptionManager scenePerceptionManager)
    {
        this.scenePerceptionManager = scenePerceptionManager;
    }

    public void OnEnable()
    {
        if (!IsScenePerceptionSupported())
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
        else
        {
            Log.e(LOG_TAG, "Start scene failed!");
        }
    }

    public void OnDisable()
    {
        if (!isSceneComponentRunning) return;

        StopScenePerception();

        scenePerceptionManager.StopScene();
        isSceneComponentRunning = false;
    }

    public void StartScenePerception(Action successHandler, Action failedHandler)
    {
        if (isSceneComponentRunning && !isScenePerceptionStarted)
        {
            WVR_Result result = scenePerceptionManager.StartScenePerception(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane);

            if (result == WVR_Result.WVR_Success)
            {
                Log.i(LOG_TAG, "Start scene perception for 2d planes success");
                isScenePerceptionStarted = true;
                ScenePerceptionGetState();
                successHandler?.Invoke();
            }
            else
            {
                Log.e(LOG_TAG, "Start scene perception for 2d planes error.");
                failedHandler?.Invoke();
            }
        }
        else
        {
            Log.e(LOG_TAG, $"Start scene perception for 2d planes error: isSceneComponentRunning[{isSceneComponentRunning}] isScenePerceptionStarted[{isScenePerceptionStarted}]"); ;
        }
    }

    public void StopScenePerception()
    {
        if (isSceneComponentRunning && isScenePerceptionStarted)
        {
            WVR_Result result = scenePerceptionManager.StopScenePerception(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane);

            if (result == WVR_Result.WVR_Success)
            {
                isScenePerceptionStarted = false;
            }
        }
    }

    public void ScenePerceptionGetState()
    {
        WVR_ScenePerceptionState latestPerceptionState = WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
        WVR_Result result = scenePerceptionManager.GetScenePerceptionState(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane, ref latestPerceptionState);
        if (result == WVR_Result.WVR_Success)
        {
            planePerceptionState = latestPerceptionState; //Update perception state for the perception target
        }
    }

    private bool IsScenePerceptionSupported()
    {
        return (Interop.WVR_GetSupportedFeatures() &
                (ulong)WVR_SupportedFeature.WVR_SupportedFeature_ScenePerception) != 0;
    }


    public bool CurrentPerceptionTargetIsCompleted()
    {
        return planePerceptionState == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Completed;
    }

    public bool CurrentPerceptionTargetIsEmpty()
    {
        return planePerceptionState == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
    }
}
