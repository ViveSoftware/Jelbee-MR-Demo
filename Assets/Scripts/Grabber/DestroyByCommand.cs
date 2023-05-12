using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByCommand : IDestroyHandler
{
    [SerializeField]
    private string destroyCmdKey = "Planet";
    
    private void Start()
    {
        DestroyCommandMediator.RegisterHandler(destroyCmdKey, destroy);
    }

    private void OnDestroy()
    {
        DestroyCommandMediator.UnregisterHandler(destroyCmdKey, destroy);
    }

    public override void RunDestroy(bool isInstant)
    {

    }

    public override void StopDestroy()
    {
        
    }

    private void destroy()
    {
        Destroy(gameObject);
    }
}
