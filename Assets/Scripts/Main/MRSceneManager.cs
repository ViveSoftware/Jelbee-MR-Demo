using com.HTC.Common;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MRSceneManager : Singleton<MRSceneManager>
{
    [SerializeField]
    private bool enterSetup = false;

    [SerializeField]
    private PassthroughSetting passthrough;

    [SerializeField]
    private GameObject setupWarning;

    private const string SetupScene = "Setup";
    private const string GameFlow = "GameFlow";
    private const string RobotAssistant = "RobotAssistant";

    protected override void AwakeSingleton()
    {
        Wave.OpenXR.InputDeviceHand.ActivateNaturalHand(true);
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (enterSetup)
        {
            SceneManager.LoadSceneAsync(SetupScene, LoadSceneMode.Single);
            return;
        }
#endif

        SceneComponentManager.Instance.LoadScenePlanes();
        SceneComponentManager.Instance.GenerateScenePlanes();

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
        }
    }

    private void Update()
    {
        if (ViveInput.GetPressDownEx(ControllerRole.LeftHand, ControllerButton.AKey) || ViveInput.GetPressDownEx(ControllerRole.RightHand, ControllerButton.AKey))
        {
            SceneManager.LoadSceneAsync(SetupScene, LoadSceneMode.Single);
        }
    }

    public void RestartGame()
    {
        SceneManager.UnloadSceneAsync(GameFlow);
        SceneManager.UnloadSceneAsync(RobotAssistant);
        SceneManager.LoadSceneAsync(GameFlow, LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync(RobotAssistant, LoadSceneMode.Additive);
    }
}
