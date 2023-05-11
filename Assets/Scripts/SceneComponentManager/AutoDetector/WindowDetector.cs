using com.HTC.WVRLoader;
using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PivotManager;

public class WindowDetector : PivotAutoDetector
{
    public override void GeneratePivot(string pivotName)
    {
        float maxDist = float.MaxValue;
        var userPose = VivePose.GetPose(DeviceRole.Hmd);

        Transform pivot = null;
        foreach(PlaneController planeController in SceneComponentManager.Instance.PlaneControllers)
        {
            if(planeController.Data.Type == ShapeTypeEnum.window.ToString())
            {
                float dist = Vector3.Distance(userPose.pos, planeController.transform.position);
                if (dist < maxDist)
                {
                    pivot = planeController.transform;
                    maxDist = dist;
                }
            }
        }

        if(pivot == null)
        {
            Debug.Log("Cannot fine appropriate window!");
            return;
        }

        Vector3 pos = pivot.position;
        Vector3 forward = -pivot.forward;
        PivotManager.Instance.SetPivot(pivotName, pos, forward);
    }
}
