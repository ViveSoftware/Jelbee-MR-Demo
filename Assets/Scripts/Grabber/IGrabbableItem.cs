using com.HTC.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGrabbableItem : MonoBehaviour
{
    [SerializeField] Quaternion leftGrabbingRot;
    [SerializeField] Quaternion rightGrabbingRot;

    protected GrabberTracker grabber;

    public void StartGrabbing(GrabberTracker grabber, bool isLeft)
    {
        this.grabber = grabber;
        transform.SetParent(grabber.Pivot, false);
        transform.localRotation = isLeft ? leftGrabbingRot : rightGrabbingRot;
        startGrabbing();
    }
    protected virtual void startGrabbing() { }

    public void EndGrabbing(GrabberTracker grabber)
    {
        transform.SetParent(null, true);
        endGrabbing();
    }
    protected virtual void endGrabbing() { }

    public abstract void GrabbingFailed(GrabberTracker grabber);

#if UNITY_EDITOR
    [Button("Set right rotation")]
    public void SetRightRot()
    {
        rightGrabbingRot = transform.localRotation;
    }

    [Button("Set left rotation")]
    public void SetLeftRot()
    {
        leftGrabbingRot = transform.localRotation;
    }

    [Button("Apply right rotation")]
    public void ApplyRightRot()
    {
        transform.localRotation = rightGrabbingRot;
    }

    [Button("Apply left rotation")]
    public void ApplyLeftRot()
    {
        transform.localRotation = leftGrabbingRot;
    }
#endif
}
