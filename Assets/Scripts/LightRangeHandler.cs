using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightRangeHandler : MonoBehaviour
{
    private Light m_light;
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private float rangeScale;

    private void Awake()
    {
        m_light = GetComponent<Light>();
    }

    void Update()
    {
        m_light.range = target.transform.localScale.x * rangeScale;
    }
}
