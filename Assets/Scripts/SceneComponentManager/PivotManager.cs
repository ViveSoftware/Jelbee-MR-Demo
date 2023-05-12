using com.HTC.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static HTC.UnityPlugin.Vive.ViveRoleBindingsHelper;

public class PivotManager : Singleton<PivotManager>
{
    [Serializable]
    public class PivotBindingData
    {
        public string Key;
        public Vector3 Pos;
        public Quaternion Rot;
    }

    [Serializable]
    public class PivotDef
    {
        public string Key;
        public PivotAutoDetector AutoDetector;
    }

    [SerializeField] private List<PivotDef> pivotDefs;

    public List<PivotDef> Defs { get { return pivotDefs; } }

    [SerializeField] private bool showPivotGizmo;
    [SerializeField] private PivotGizmo pivotGizmoPrefab;

    private Dictionary<string, Transform> pivotMap = new Dictionary<string, Transform>();

    private string PIVOT_CONFIG_PATH => Path.Combine(Application.persistentDataPath, "pivot_binding.bin");
    
    //Used in Game

    public bool IsSaveFileExisted
    {
        get
        {
            return File.Exists(PIVOT_CONFIG_PATH);
        }
    }

    public bool IsValid
    {
        get
        {
            for(int i=0; i<pivotDefs.Count; ++i)
            {
                if (!pivotMap.ContainsKey(pivotDefs[i].Key))
                {
                    Debug.Log($"Pivot [{pivotDefs[i].Key}] is non-existent");
                    return false;
                }
            }
            return true;
        }
    }

    public void Load()
    {
        refreshPivotMap();

        List<PivotBindingData> bindings = FileManager.LoadData<List<PivotBindingData>>(PIVOT_CONFIG_PATH);
        if(bindings != null)
        {
            foreach (PivotBindingData binding in bindings)
            {
                GameObject pivotObj = new GameObject(string.Format("pivot_{0}", binding.Key));
                pivotMap[binding.Key] = pivotObj.transform;
                pivotMap[binding.Key].SetPositionAndRotation(binding.Pos, binding.Rot);

                if(showPivotGizmo && pivotGizmoPrefab != null)
                {
                    PivotGizmo pivotGizmo = Instantiate(pivotGizmoPrefab, pivotObj.transform);
                    pivotGizmo.SetLabel(binding.Key);
                }
            }
        }
    }

    private void refreshPivotMap() 
    {
        if (pivotMap != null)
        {
            foreach (KeyValuePair<string, Transform> pair in pivotMap)
            {
                Destroy(pair.Value.gameObject);
            }
            pivotMap.Clear();
        }
    }

    public void AutoDetect()
    {
        refreshPivotMap();

        foreach (PivotDef pivotDef in pivotDefs)
        {
            pivotDef.AutoDetector.GeneratePivot(pivotDef.Key);
        }
    }

    public Transform GetPivot(string key)
    {
        if(pivotMap.ContainsKey(key))
        {
            return pivotMap[key];
        }
        else
        {
            Debug.LogError($"Pivot of [{key}] is not exist!");
            return null;
        }
    }

    public Transform GetLightRoot()
    {
        return GetPivot(LIGHT_KEY);
    }

    //Used in Setup

    private const string LIGHT_KEY = "DirectionalLight";

    public void SetLight(Vector3 pos, Vector3 forward)
    {
        SetPivot(LIGHT_KEY, pos, forward);
    }

    public void SetPivot(string key, Vector3 pos, Vector3 forward)
    {
        Transform pivot;

        if(!pivotMap.TryGetValue(key, out pivot))
        {
            GameObject pivotObj = new GameObject(string.Format("pivot_{0}", key));
            pivotMap[key] = pivotObj.transform;
            pivot = pivotObj.transform;
        }

        pivot.position = pos;
        pivot.forward = forward;
    }

    public void Save()
    {
        List<PivotBindingData> bindings = new List<PivotBindingData>();
        foreach (KeyValuePair<string, Transform> bindingPair in pivotMap)
        {
            bindings.Add(new PivotBindingData()
            {
                Key = bindingPair.Key,
                Pos = bindingPair.Value.position,
                Rot = bindingPair.Value.rotation
            });
        }

        FileManager.SaveData(bindings, PIVOT_CONFIG_PATH);
    }
}

