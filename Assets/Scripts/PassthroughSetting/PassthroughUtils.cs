using System.Runtime.InteropServices;
using UnityEngine;
using Wave.Native;

namespace com.HTC.Common
{
    public static class PassthroughUtils
    {
        private const string TAG = nameof(PassthroughUtils);

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
            MainCamera.backgroundColor = Color.white * 0;

            return SetWvrPassthroughEnabled(true);
        }

        public static void DisablePassThrough()
        {
            //Log.i(TAG, "DisablePassThrough");
            MainCamera.clearFlags = CameraClearFlags.Skybox;
            MainCamera.backgroundColor = new Color(49f / 255f, 77f / 255f, 121f / 255f, 5f / 255f);

            SetWvrPassthroughEnabled(false);
        }

        private static bool SetWvrPassthroughEnabled(bool enable)
        {
            Log.i(TAG, "SetWvrPassthroughEnabled(): " + enable);
            if (enable)
            {
                bool result = WVR_SwapPassthroughContent(true);
                if (!result)
                {
                    Log.e(TAG, "WVR_SwapPassthroughContent(true): fail");
                }
                else
                {
                    result = Interop.WVR_ShowPassthroughOverlay(true);
                    if (!result)
                    {
                        Log.e(TAG, "WVR_ShowPassthroughOverlay(true): fail");
                    }

                    return result;
                }
            }
            else
            {
                bool result = Interop.WVR_ShowPassthroughOverlay(false);
                if (!result)
                {
                    Log.e(TAG, "WVR_ShowPassthroughOverlay(false): fail");
                }

                result = WVR_SwapPassthroughContent(false);
                if (!result)
                {
                    Log.e(TAG, "WVR_SwapPassthroughContent(false): fail");
                }
            }

            return false;
        }

        [DllImport("wvr_api", EntryPoint = "WVR_SwapPassthroughContent", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool WVR_SwapPassthroughContent(bool enable);
    }
}
