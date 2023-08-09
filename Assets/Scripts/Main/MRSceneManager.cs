using com.HTC.Common;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wave.Essence.ScenePerception;
using Wave.Native;

public class MRSceneManager : MonoBehaviour
{
    [SerializeField]
    private bool enterSetup = false;

    [SerializeField]
    private PassthroughSetting passthrough;

    [SerializeField]
    private ScenePerceptionManager scenePerceptionManager;

    [SerializeField]
    private GameObject setupWarning;

    [SerializeField]
    private ScenePlaneGenerator planeGenerator;

    private const string SetupScene = "Setup";
    private const string GameFlow = "GameFlow";
    private const string RobotAssistant = "RobotAssistant";
    private const string LOG_TAG = "JelbeeMR";

    private MRScenePerceptionHelper scenePerceptionHelper;
    private bool isGameSceneLoaded = false;

    protected void Awake()
    {
        Wave.OpenXR.InputDeviceHand.ActivateNaturalHand(true);
        isGameSceneLoaded = false;
        EventMediator.LeaveSetupMode += leaveSetupMode;
        EventMediator.RestartGame += restartGame;
    }

    private void OnDestroy()
    {
        EventMediator.LeaveSetupMode -= leaveSetupMode;
        EventMediator.RestartGame -= restartGame;
    }

    private void restartGame()
    {
        SceneManager.UnloadSceneAsync(GameFlow);
        SceneManager.UnloadSceneAsync(RobotAssistant);
        SceneManager.LoadSceneAsync(GameFlow, LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync(RobotAssistant, LoadSceneMode.Additive);
    }

    private void leaveSetupMode()
    {
        SceneManager.UnloadSceneAsync(SetupScene);
        initialize();
    }

    private void enterSetupMode()
    {
        if(isGameSceneLoaded)
        {
            SceneManager.UnloadSceneAsync(GameFlow);
            SceneManager.UnloadSceneAsync(RobotAssistant);
            isGameSceneLoaded = false;
        }
        
        SceneManager.LoadSceneAsync(SetupScene, LoadSceneMode.Additive);
    }

#if UNITY_EDITOR
    private void Start()
    {
        SceneComponentManager.Instance.LoadScenePlanes();

        if (enterSetup)
        {
            enterSetupMode();
            return;
        }
        
        initialize();
    }
#else
    private IEnumerator Start()
    {
        scenePerceptionHelper = new MRScenePerceptionHelper(scenePerceptionManager);
        scenePerceptionHelper.OnEnable();

        //wait for ScenePerceptionManager start scene
        yield return new WaitUntil(() => scenePerceptionHelper.isSceneComponentRunning == true);
        scenePerceptionHelper.StartScenePerception(onPerceptionStartSuccess, onPerceptionStartFailed);
    }

    private void onPerceptionStartSuccess()
    {
        SceneComponentManager.Instance.LoadScenePlanes();
        initialize();
    }

    private void onPerceptionStartFailed()
    {
        Log.e(LOG_TAG, "Start scene perception error, please check setting page and make sure the TRACKING MODE is set to DEFAULT");
    }

    private void OnDisable()
    {
        if (scenePerceptionHelper != null)
            scenePerceptionHelper.OnDisable();
    }
#endif

    private void initialize()
    {
        Log.i(LOG_TAG, "initialize");
        SceneComponentManager.Instance.GenerateScenePlanes(planeGenerator);

        if (PivotManager.Instance.IsSaveFileExisted)
        {
            PivotManager.Instance.Load();
        }
        else
        {
            PivotManager.Instance.AutoDetect();
        }

        setupWarning.SetActive(!PivotManager.Instance.IsValid);

        if (PivotManager.Instance.IsValid)
        {
            passthrough.gameObject.SetActive(true);
            SceneManager.LoadSceneAsync(GameFlow, LoadSceneMode.Additive);
            SceneManager.LoadSceneAsync(RobotAssistant, LoadSceneMode.Additive);
            isGameSceneLoaded = true;
        }
    }

    private void Update()
    {
        if (ViveInput.GetPressDownEx(ControllerRole.LeftHand, ControllerButton.AKey) || ViveInput.GetPressDownEx(ControllerRole.RightHand, ControllerButton.AKey))
        {
            enterSetupMode();
        }
    }
}
