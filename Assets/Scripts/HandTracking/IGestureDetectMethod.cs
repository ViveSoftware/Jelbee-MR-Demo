using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.HTC.Gesture
{
    public abstract class IGestureDetectMethod : ScriptableObject
    {
        public float StartToleration;           //MaxTime from "before detecting current gesture" to "lost previous-gesture detection"
        public float DetectingToleration;       //MaxTime from "lost gesture detection" to "resume gesture detection " 
        public float Duration;                  //Time from start detecting to completed
        public abstract string Tag { get; }
        protected abstract bool isDetected();
        public KeyCode HotKey;

        public virtual void UpdateTime(float deltaTime)
        {
            
        }

        public bool IsDetected()
        {
#if UNITY_EDITOR
            return Input.GetKey(HotKey);
#else
            return isDetected();
#endif
        }

#if UNITY_EDITOR
        const string defalutFolder = "HTC/GestureDetection/Methods";
        const string defaultPath = "Assets/HTC/GestureDetection/Methods";

        protected static void createInstance<T>() where T:IGestureDetectMethod
        {
            T asset = CreateInstance<T>();
            string fileFolder = Path.Combine(Application.dataPath, defalutFolder);
            if (!Directory.Exists(fileFolder))
            {
                Directory.CreateDirectory(fileFolder);
            }

            AssetDatabase.CreateAsset(asset, Path.Combine(defaultPath, string.Format("{0}.asset", asset.Tag)));
            AssetDatabase.SaveAssets();
        }
#endif
    }
}