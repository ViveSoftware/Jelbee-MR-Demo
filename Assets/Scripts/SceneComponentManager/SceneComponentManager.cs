using com.HTC.Common;
using com.HTC.WVRLoader;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;
using static Wave.Essence.ScenePerception.ScenePerceptionManager;

public class SceneComponentManager : Singleton<SceneComponentManager>
{
    [SerializeField] private ScenePerceptionManager scenePerceptionManager;

    public List<PlaneData> Planes { get; private set; }
    
    private List<PlaneController> planeControllers;
    public List<PlaneController> PlaneControllers { get { return planeControllers; } }

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

    public void LoadScenePlanes()
    {
#if UNITY_EDITOR
        loadScenePlaneFromTestFiles();
#else
        loadScenePlaneFromAPI();
#endif
    }
    private void loadScenePlaneFromAPI()
    {
        WVR_Result result = scenePerceptionManager.GetScenePlanes(ScenePerceptionManager.GetCurrentPoseOriginModel(), out WVR_ScenePlane[] currentScenePlanes);
        if (result != WVR_Result.WVR_Success)
        {
            Debug.Log("Failed to get scene planes");
        }

        Planes = new List<PlaneData>();
        foreach(WVR_ScenePlane wvrPlane in currentScenePlanes)
        {
            if (parseWVRPlaneToPlaneData(wvrPlane, out PlaneData planeData))
            {
                Planes.Add(planeData);
                Debug.Log(planeData.ToString());
            }
        }
    }
    private static bool parseWVRPlaneToPlaneData(WVR_ScenePlane wvrPlane, out PlaneData planeData)
    {
        planeData = new PlaneData();

        WVR_Pose_t planePose = wvrPlane.pose;
        WVR_Extent2Df planeDimensions = wvrPlane.extent;
        Vector3 planePositionUnity = Vector3.zero;
        Quaternion planeRotationUnity = Quaternion.identity;

        Coordinate.GetVectorFromGL(planePose.position, out planePositionUnity);
        Coordinate.GetQuaternionFromGL(planePose.rotation, out planeRotationUnity);
        planeRotationUnity *= Quaternion.Euler(0, 180f, 0);

        Vector3[] vertices = MeshGenerationHelper.GenerateQuadVertex(planeDimensions);
        Matrix4x4 trs = Matrix4x4.TRS(planePositionUnity, planeRotationUnity, Vector3.one);

        planeData.UID = Guid.NewGuid().ToString();
        planeData.Points = new Vector3[4];
        //transfer quad vertice date from wave api to PlaneData
        planeData.Points[0] = trs.MultiplyPoint(vertices[1]);
        planeData.Points[1] = trs.MultiplyPoint(vertices[3]);
        planeData.Points[2] = trs.MultiplyPoint(vertices[2]);
        planeData.Points[3] = trs.MultiplyPoint(vertices[0]);
        planeData.Center = (planeData.Points[0] + planeData.Points[2]) / 2f;
        planeData.Width = planeDimensions.width;
        planeData.Height = planeDimensions.height;
        planeData.Type = planeLabelToStr(wvrPlane.planeLabel);
        return true;
    }
    /// <summary>
    /// Mapping WVR_ScenePlaneLabel to ShapeTypeEnum
    /// </summary>
    private static string planeLabelToStr(WVR_ScenePlaneLabel label)
    {
        switch (label)
        {
            case WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Ceiling: return "ceiling";
            case WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Floor: return "floor";
            case WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Wall: return "wall";
            case WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Desk: return "table";
            case WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Couch: return "chair";
            case WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Door: return "door";
            case WVR_ScenePlaneLabel.WVR_ScenePlaneLabel_Window: return "window";
            default: return "unknown";
        }
    }

    private void loadScenePlaneFromTestFiles()
    {
        TextAsset asset = Resources.Load<TextAsset>("SampleSceneComponents");
        if (asset != null)
        {
            string text = asset.text;
            Resources.UnloadAsset(asset);

            List<PlaneData> planes;
            parseToObjects(text, out planes);

            Planes = planes;
        }
        else
        {
            Planes = new List<PlaneData>();
        }
    }
    private static bool parseToObjects<T>(string str, out List<T> sceneObjs) where T : SceneObjectData, new()
    {
        try
        {
            string[] shapeStrs = str.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            sceneObjs = new List<T>();

            foreach (string shapeStr in shapeStrs)
            {
                T obj = new T();
                if (obj.FromString(shapeStr))
                {
                    sceneObjs.Add(obj);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Error occurred when parsing scene components from file: " + e.Message);
            sceneObjs = null;
            return false;
        }
    }

    public void GenerateScenePlanes(ScenePlaneGenerator generator)
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
}