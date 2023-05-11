using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Common
{
    public class PassthroughSetting : MonoBehaviour
    {
        private void OnEnable()
        {
#if !UNITY_EDITOR
            PassthroughUtils.EnablePassThrough();
#endif
        }

        private void OnDisable()
        {
#if !UNITY_EDITOR
            PassthroughUtils.DisablePassThrough();
#endif
        }

        private void OnApplicationPause(bool isPaused)
        {
#if !UNITY_EDITOR
            if (isPaused)
            {
                PassthroughUtils.DisablePassThrough();
            }
            else
            {
                PassthroughUtils.EnablePassThrough();
            }
#endif
        }

    }
}