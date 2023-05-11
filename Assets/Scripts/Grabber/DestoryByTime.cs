using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryByTime : IDestroyHandler
{
    [SerializeField]
    private float timer = 3;

    public override void RunDestroy(bool isInstant)
    {
        if (isInstant)
        {
            Destroy();
        }
        else
        {
            Invoke("Destroy", timer);
        }
    }

    public override void StopDestroy()
    {
        CancelInvoke();
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
