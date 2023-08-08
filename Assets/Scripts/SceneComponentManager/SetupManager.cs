using com.HTC.WVRLoader;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupManager : MonoBehaviour
{
    [SerializeField] private ScenePlaneGenerator generator;

    [SerializeField] private PivotGizmo pivotGizmoPrefab;
    [SerializeField] private Transform pivotGizmoRoot;

    [SerializeField] private PivotBtn pivotBtnPrefab;
    [SerializeField] private RectTransform pivotListViewRoot;
    [SerializeField] private GameObject lightGizmo;
    [SerializeField] private Transform lightDeskGizmo;
    [SerializeField] private Light dirLight;

    private string currentKey = null;
    private PlaneController deskController;

    private Dictionary<string, PivotGizmo> pivotGizmoMap = new Dictionary<string, PivotGizmo>();

    private Dictionary<ShapeTypeEnum, Action<PlaneController, string>> pivotSetupHandlerMap;

    void Start()
    {
        SceneComponentManager.Instance.GenerateScenePlanes(generator);

        foreach(PlaneController planeController in SceneComponentManager.Instance.PlaneControllers)
        {
            (planeController as SetupPlaneController).OnClickedHandler += OnScenePlaneClicked;
            if(planeController.ShapeType == ShapeTypeEnum.table)
            {
                deskController = planeController;
            }
        }

        foreach(PivotManager.PivotDef pivotDef in PivotManager.Instance.Defs)
        {
            PivotBtn pivotBtn = Instantiate(pivotBtnPrefab, pivotListViewRoot);
            pivotBtn.Init(OnPivotBtnClicked, pivotDef.Key);
        }

        pivotSetupHandlerMap = new Dictionary<ShapeTypeEnum, Action<PlaneController, string>>();
        pivotSetupHandlerMap[ShapeTypeEnum.wall] = SetPivotOnVerticalPlane;
        pivotSetupHandlerMap[ShapeTypeEnum.window] = SetPivotOnVerticalPlane;
        pivotSetupHandlerMap[ShapeTypeEnum.door] = SetPivotOnVerticalPlane;
        pivotSetupHandlerMap[ShapeTypeEnum.table] = SetPivotOnHorizontalPlane;

        lightGizmo.gameObject.SetActive(false);
    }

    private void OnPivotBtnClicked(string key)
    {
        lightGizmo.gameObject.SetActive(false);
        currentKey = key;
    }

    public void OnLightBtnClicked()
    {
        currentKey = null;
        lightGizmo.gameObject.SetActive(true);

        if(deskController != null)
        {
            lightDeskGizmo.position = deskController.transform.position + Vector3.up * 0.025f;
            lightDeskGizmo.forward = calculateDeskForward(deskController);
        }
    }

    private void Update()
    {
        if(lightGizmo.gameObject.activeSelf)
        {
            if(ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger))
            {
                PivotManager.Instance.SetLight(dirLight.transform.position, dirLight.transform.forward);
                lightGizmo.gameObject.SetActive(false);
            }
        }
    }

    private void OnScenePlaneClicked(PlaneController planeController)
    {
        Debug.Log(planeController.Data.Type + " is clicked!");

        if(!string.IsNullOrEmpty(currentKey))
        {
            PivotGizmo pivotGizmo;
            if(!pivotGizmoMap.TryGetValue(currentKey, out pivotGizmo))
            {
                pivotGizmo = Instantiate(pivotGizmoPrefab, pivotGizmoRoot);
                pivotGizmo.SetLabel(currentKey);
                pivotGizmoMap[currentKey] = pivotGizmo;
            }
            pivotSetupHandlerMap[planeController.ShapeType].Invoke(planeController, currentKey);

            pivotGizmo.transform.SetPositionAndRotation(
                PivotManager.Instance.GetPivot(currentKey).position,
                PivotManager.Instance.GetPivot(currentKey).rotation);
        }
    }

    private void SetPivotOnVerticalPlane(PlaneController planeController, string pivotKey)
    {
        Vector3 pos = planeController.transform.position;
        Vector3 forward = -planeController.transform.forward;
        PivotManager.Instance.SetPivot(pivotKey, pos, forward);
    }

    private void SetPivotOnHorizontalPlane(PlaneController planeController, string pivotKey)
    {
        Vector3 pos = planeController.transform.position;
        Vector3 forward = calculateDeskForward(planeController);
        PivotManager.Instance.SetPivot(pivotKey, pos, forward);
    }

    private static Vector3 calculateDeskForward(PlaneController planeController)
    {
        Vector3 pos = planeController.transform.position;

        RigidPose hmdPos = VRModule.GetCurrentDeviceState(ViveRole.GetDeviceIndexEx(DeviceRole.Hmd)).pose;
        Vector3 horizontalDir = Vector3.Scale(pos - hmdPos.pos, new Vector3(1, 0, 1)).normalized;

        Vector3 vec1 = (planeController.Data.Points[1] - planeController.Data.Points[0]).normalized;
        float dot1 = Vector3.Dot(horizontalDir, vec1);

        Vector3 vec2 = (planeController.Data.Points[3] - planeController.Data.Points[0]).normalized;
        float dot2 = Vector3.Dot(horizontalDir, vec2);

        Vector3 forward = Vector3.forward;
        if (Mathf.Abs(dot1) < Mathf.Abs(dot2))
        {
            //dot2
            if (dot2 < 0)
            {
                forward = -vec2;
            }
            else
            {
                forward = vec2;
            }
        }
        else
        {
            //dot1
            if (dot1 < 0)
            {
                forward = -vec1;
            }
            else
            {
                forward = vec1;
            }
        }
        return forward;
    }

    public void Save()
    {
        PivotManager.Instance.Save();
    }

    public void StartGame()
    {
        EventMediator.LeaveSetupMode();
    }
}
