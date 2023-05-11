using com.HTC.WVRLoader;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PivotManager;

public class DeskDetector : PivotAutoDetector
{
    public override void GeneratePivot(string pivotName)
    {
        float maxDist = float.MaxValue;
        var userPose = VivePose.GetPose(DeviceRole.Hmd);

        PlaneController planeController = null;
        foreach (PlaneController controller in SceneComponentManager.Instance.PlaneControllers)
        {
            if (controller.Data.Type == ShapeTypeEnum.table.ToString())
            {
                float dist = Vector3.Distance(userPose.pos, controller.transform.position);
                if (dist < maxDist)
                {
                    planeController = controller;
                    maxDist = dist;
                }
            }
        }

        if (planeController == null)
        {
            Debug.Log("Cannot fine appropriate table!");
            return;
        }

        Vector3 pos = planeController.transform.position;
        Vector3 forward = calculateDeskForward(planeController);
        PivotManager.Instance.SetPivot(pivotName, pos, forward);
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
}
