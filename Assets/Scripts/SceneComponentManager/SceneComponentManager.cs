using com.HTC.Common;
using com.HTC.WVRLoader;
using System.Collections.Generic;
using UnityEngine;

public class SceneComponentManager : Singleton<SceneComponentManager>
{
    [SerializeField] private ScenePlaneGenerator generator;

    [SerializeField] private SceneMeshGenerator meshGenerator;

    public List<PlaneData> Planes { get; private set; }
    public List<Mesh> SceneMeshes { get; private set; }

    private List<PlaneController> planeControllers;
    public List<PlaneController> PlaneControllers { get { return planeControllers; } }

    public void LoadScenePlanes()
    {
        Planes = SceneComponentLoader.LoadScenePlanes(ShapeTypeEnum.all);
    }

    public void GenerateScenePlanes()
    {
        if (planeControllers != null)
        {
            foreach (PlaneController plane in planeControllers)
            {
                Destroy(plane.gameObject);
            }
            planeControllers = null;
        }
        planeControllers = generator.GenerateScenePlanes(Planes);
        foreach (PlaneController planeController in PlaneControllers)
        {
            planeController.gameObject.SetActive(isPlaneActivated);
        }
    }

    public void GenerateSceneMeshes()
    {
        SceneMeshPermissionHelper.RequestSceneMeshPermission();

        if(SceneMeshPermissionHelper.permissionGranted)
        {
            SceneMeshes = ScenePerceptionUtility.Instance.LoadVisualMeshes();
        }

        if (SceneMeshes == null || SceneMeshes.Count == 0)
        {
            SceneMeshes = new List<Mesh>();
            SceneMeshes.Add(meshGenerator.Generate(Planes));
        }
    }

    private bool isPlaneActivated = true;
    public bool IsPlanesActivated
    {
        get
        {
            return isPlaneActivated;
        }
        set
        {
            isPlaneActivated = value;
            foreach (PlaneController planeController in PlaneControllers)
            {
                planeController.gameObject.SetActive(isPlaneActivated);
            }
        }
    }
}