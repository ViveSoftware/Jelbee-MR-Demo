using com.HTC.Common;
using com.HTC.WVRLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePlaneGenerator : MonoBehaviour
{
    [SerializeField]
    private Transform root;

    [SerializeField]
    private List<PlaneController> planePrefabs;
    private Dictionary<ShapeTypeEnum, PlaneController> planePrefabMap;

    private void Awake()
    {
        //initialize prefab map
        planePrefabMap = new Dictionary<ShapeTypeEnum, PlaneController>();
        foreach (PlaneController planePrefab in planePrefabs)
        {
            planePrefabMap[planePrefab.ShapeType] = planePrefab;
        }
    }

    public List<PlaneController> GenerateScenePlanes(List<PlaneData> planes)
    {
        List<PlaneController> planeControllers = new List<PlaneController>();
        foreach (PlaneData planeData in planes)
        {
            PlaneController planeController = createPlane(planeData);
            if (planeController != null) planeControllers.Add(planeController);
        }
        return planeControllers;
    }

    public PlaneController createPlane(PlaneData data)
    {
        PlaneController prefab = null;

        ShapeTypeEnum shapeType = ShapeTypeEnum.all;
        Enum.TryParse(data.Type, out shapeType);
        if(shapeType == ShapeTypeEnum.all)
        {
            Debug.LogError($"Plane type: [{data.Type}] is invalid");
            return null;
        }

        planePrefabMap.TryGetValue(shapeType, out prefab);

        if (prefab != null)
        {
            PlaneController newPlane = Instantiate(prefab, root);
            newPlane.Initialize(data);
            return newPlane;
        }
        else
        {
            Debug.LogError($"Plane create error: prefab of type [ {data.Type} ] is not exist");
            return null;
        }
    }
}
