using com.HTC.Gesture;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrabberTracker : MonoBehaviour
{
    [Serializable]
    public class UnityEventGrabber : UnityEvent<GrabberTracker> { };

    [SerializeField] private float outsideRatio = 0.8f;
    [SerializeField, Tooltip("Trigger on outside or lost tracking")] private UnityEventGrabber OnGrabbingFailedHandler;
    [SerializeField, Tooltip("Trigger on enter fov with grab gesture")] private UnityEventGrabber OnGrabbingStartHandler;
    [SerializeField, Tooltip("Trigger on release grab gesture in fov")] private UnityEventGrabber OnGrabbingEndHandler;
    [SerializeField] private float gestureTolerance = 0.25f;
    [SerializeField] private float lostTrackingTolerance = 0.1f;
    [SerializeField] private Transform pivot;
    [SerializeField] private bool isLeft;

    public Transform Pivot { get { return pivot; } }
    public Func<bool> IsGestrureDetected;

    private float outsideAngleVertical;
    private float outsideAngleHorizontal;
    private bool isOutside;
    private bool isTrackerValid;
    private IGrabbableItem grabbingItem;
    private float curLostTrackingTolerance = 0;
    private float curGestureTolerance = 0;
    private bool startGrab = false;
    private float invalidPose = 0;
    private float validPose = 0;

    public IGrabbableItem GrabbingItem
    {
        get
        {
            return grabbingItem;
        }
    }

    void Start()
    {
        grabbingItem = null;
        outsideAngleVertical = Camera.main.fieldOfView * outsideRatio;
        outsideAngleHorizontal = outsideAngleVertical * Camera.main.aspect;
        isOutside = false;
        isTrackerValid = false;
    }

    public void SetTrackingValid(bool isValid)
    {
        if(isTrackerValid != isValid)
        {
            isTrackerValid = isValid;
            if(isTrackerValid)
            {
                Debug.Log("Grabber tracker tracked!");
                curLostTrackingTolerance = lostTrackingTolerance;
                isOutside = checkOutside();
            }
        }
    }

    private void Update()
    {
        bool outside = checkOutside();

#if !UNITY_EDITOR
        var isPoseValid = isLeft ? 
            VRModule.GetCurrentDeviceState(ViveRole.GetDeviceIndexEx<HandRole>(HandRole.LeftHand)).isPoseValid : 
            VRModule.GetCurrentDeviceState(ViveRole.GetDeviceIndexEx<HandRole>(HandRole.RightHand)).isPoseValid;


        if (!isPoseValid)
        {
            invalidPose = Time.frameCount;
            return;
        }
#endif

        validPose = Time.frameCount;

        if (IsGestrureDetected.Invoke() && !startGrab && outside)
        {
            startGrab = true;
            OnGrabbingStartHandler.Invoke(this);
        }
        else if (IsGestrureDetected.Invoke() && !startGrab)
        {
            // double check when hand suddenly appears in front of hmd
            if ((invalidPose + 1) == validPose)
            {
                startGrab = true;
                OnGrabbingStartHandler.Invoke(this);
            }
        }
        else if (!IsGestrureDetected.Invoke() && startGrab)
        {
            startGrab = false;
            OnGrabbingEndHandler.Invoke(this);
        }
    }

    private bool checkOutside()
    {
#if UNITY_EDITOR
        return true;
#endif

        RigidPose hmdPose = VivePose.GetPose(DeviceRole.Hmd);

        Vector3 hmdToGrabber = transform.position - hmdPose.pos;
        float verticalAngle = Vector3.Angle(
            Vector3.ProjectOnPlane(hmdToGrabber, hmdPose.right),
            hmdPose.forward);

        float horizontalAngle = Vector3.Angle(
            Vector3.ProjectOnPlane(hmdToGrabber, hmdPose.up),
            hmdPose.forward);

        return verticalAngle >= outsideAngleVertical || horizontalAngle >= outsideAngleHorizontal;
    }

    public void SetGrabItem(IGrabbableItem newItem)
    {
        if(grabbingItem != null)
        {
            grabbingItem.EndGrabbing(this);
        }

        grabbingItem = newItem;
        if (grabbingItem != null)
        {
            grabbingItem.StartGrabbing(this, isLeft);
        }
    }

    public void SetGrabFailed()
    {
        if(grabbingItem != null)
        {
            grabbingItem.GrabbingFailed(this);
            grabbingItem = null;
        }
    }
}
