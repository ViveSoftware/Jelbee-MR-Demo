using System.Runtime.InteropServices;
using UnityEngine;
using Wave.Native;

namespace com.HTC.Common
{
    public static class PassthroughUtils
    {
        private const string TAG = nameof(PassthroughUtils);
        private static Color originColor;

        private static Camera _MainCamera = null;
        private static Camera MainCamera
        {
            get
            {
                if (_MainCamera == null)
                {
                    _MainCamera = Camera.main;
                }

                return _MainCamera;
            }
        }

        public static bool EnablePassThrough()
        {
            //Log.i(TAG, "EnablePassThrough");
            MainCamera.clearFlags = CameraClearFlags.SolidColor;
            originColor = MainCamera.backgroundColor;
            MainCamera.backgroundColor = Color.white * 0;

            Interop.WVR_SetPassthroughImageFocus(WVR_PassthroughImageFocus.Scale);

            return WVR_enablePassthrough(true);
        }

        public static void DisablePassThrough()
        {
            //Log.i(TAG, "DisablePassThrough");
            MainCamera.clearFlags = CameraClearFlags.Skybox;
            MainCamera.backgroundColor = originColor;

            WVR_enablePassthrough(false);
        }

        private static bool WVR_enablePassthrough(bool enable)
        {
            Debug.Log($"{TAG}: Set passthrough {enable}");

            if (enable)
            {
                WVR_Result result = Interop.WVR_ShowPassthroughUnderlay(true);
                if (result != WVR_Result.WVR_Success)
                    Debug.Log($"{TAG}: WVR_ShowPassthroughUnderlay(true) failed: {result.ToString()}");
                return result == WVR_Result.WVR_Success;
            }
            else
            {
                WVR_Result result = Interop.WVR_ShowPassthroughUnderlay(false);
                if (result != WVR_Result.WVR_Success)
                    Debug.Log($"{TAG}: WVR_ShowPassthroughUnderlay(false) failed: {result.ToString()}");
            }

            return false;
        }
    }
}
