using com.HTC.WVRLoader;
using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetector : PivotAutoDetector
{
    const string TableType = "table";
    const string WallType = "wall";

    public override void GeneratePivot(string pivotName)
    {
        //Find closet table
        float maxDist = float.MaxValue;
        var userPose = VivePose.GetPose(DeviceRole.Hmd);

        PlaneController nearestDeskPlane = null;
        List<PlaneController> wallControllers = new List<PlaneController>();
        foreach (PlaneController controller in SceneComponentManager.Instance.PlaneControllers)
        {
            if (controller.Data.Type == TableType)
            {
                float dist = Vector3.Distance(userPose.pos, controller.transform.position);
                if (dist < maxDist)
                {
                    nearestDeskPlane = controller;
                    maxDist = dist;
                }
            }

            if(controller.Data.Type == WallType)
            {
                wallControllers.Add(controller);
            }
        }

        if(nearestDeskPlane == null)
        {
            Debug.Log("MRDemo: Cannot find valid table, unable to find the valid wall.");
            return;
        }

        //Find a wall 
        Vector3 vecUserToTable = Vector3.ProjectOnPlane(nearestDeskPlane.transform.position - userPose.pos, Vector3.up).normalized;
        float minDot = 1;
        Transform wallPivot = null;
        for (int i=0; i<wallControllers.Count; i++)
        {
            float dot = Vector3.Dot(vecUserToTable, wallControllers[i].transform.forward);
            if(dot <= minDot)
            {
                minDot = dot;
                wallPivot = wallControllers[i].transform;
            }
        }

        if(wallPivot == null)
        {
            Debug.Log("MRDemo: Cannot find appropriate wall!");
            return;
        }

        Vector3 pos = wallPivot.position;
        Vector3 forward = -wallPivot.forward;
        PivotManager.Instance.SetPivot(pivotName, pos, forward);
    }
}
