using com.HTC.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPivotManager : Singleton<BoxPivotManager>
{
    private Dictionary<string, Transform> pivotMap = new Dictionary<string, Transform>();

    public void Register(string name, Transform pivot)
    {
        pivotMap[name] = pivot;
    }

    public void Unregister(string name, Transform pivot)
    {
        pivotMap.Remove(name);
    }

    public Transform Get(string pivotName)
    {
        Transform pivot = null;
        if (pivotMap.TryGetValue(pivotName, out pivot))
        {
            return pivot;
        }
        return null;
    }
}
