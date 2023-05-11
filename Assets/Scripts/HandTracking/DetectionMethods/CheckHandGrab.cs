using HTC.UnityPlugin.Vive;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace com.HTC.Gesture
{
    public class CheckHandGrab : IGestureDetectMethod
    {
        [SerializeField]
        private ViveRoleProperty role;

        [SerializeField]
        private float validRatio;

        public override string Tag => "HandGrab";

        protected override bool isDetected()
        {
            return GestureRecognizeMethod.CheckHandHold(role, validRatio)
                || GestureRecognizeMethod.CheckFingerPurse(role);
        }

#if UNITY_EDITOR
        [MenuItem("HTC/GestureDetection/Create Hand Grab")]
        public static void CreateInstance()
        {
            createInstance<CheckHandGrab>();
        }
#endif
    }
}