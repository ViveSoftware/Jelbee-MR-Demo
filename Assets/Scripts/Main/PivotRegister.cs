using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotRegister : MonoBehaviour
{
    [SerializeField] private string pivotName;
    void Start()
    {
        BoxPivotManager.Instance.Register(pivotName, transform);
    }
}
