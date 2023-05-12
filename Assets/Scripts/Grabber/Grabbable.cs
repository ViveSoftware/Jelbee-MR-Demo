using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : IGrabbableItem
{
    [SerializeField] private int ItemLabel = 0;
    [SerializeField] protected new Rigidbody rigidbody;
    [SerializeField] protected IDestroyHandler destroyHandler;

    UniversePortalBall universePortalBall = null;

    private void Start()
    {
        PortalBallBehaviour portalBallBehaviour = PortalSceneManager.Instance.GetCurrentPortalBallBehaviour();
        if (portalBallBehaviour != null && portalBallBehaviour is UniversePortalBall)
        {
            universePortalBall = (UniversePortalBall)portalBallBehaviour;
        }
        else
        {
            Debug.LogError("[Grabbable][Start] UniversePortalBall null.");
            Destroy(gameObject);
        }
    }

    protected override void startGrabbing()
    {
        Debug.Log("[Grabbable][startGrabbing] StartGrabbing");
        rigidbody.isKinematic = true;
        if (ItemLabel > 0 && universePortalBall != null) universePortalBall.SetOutParent(ItemLabel, this.gameObject);
    }

    protected override void endGrabbing()
    {
        Debug.Log("[Grabbable][endGrabbing] EndGrabbing");
        destroyHandler.RunDestroy();
        if (ItemLabel > 0 && universePortalBall != null) universePortalBall.SetInParent(ItemLabel, this.gameObject);
    }

    public override void GrabbingFailed(GrabberTracker grabber)
    {
        destroyHandler.RunDestroy(true);
    }
}
