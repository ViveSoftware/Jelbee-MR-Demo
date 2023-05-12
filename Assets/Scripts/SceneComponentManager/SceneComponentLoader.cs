using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.HTC.WVRLoader
{
    public static class SceneComponentLoader
    {
        public static List<PlaneData> LoadScenePlanes(ShapeTypeEnum shapeType)
        {
            string wvrCmd = WVRCommand.CreateCommand_GetSceneComponents(type: shapeType);

            string wvrCmdResult = WVRUtility.WVR_GetParameterDynamicSize(wvrCmd);
            Debug.LogFormat("Invoke getSceneComponents with param: [{0}] Get result:[{1}]", wvrCmd, wvrCmdResult);

            if (string.IsNullOrEmpty(wvrCmdResult) || wvrCmdResult == WVRUtility.INVALID_RESULT)
            {
                Debug.Log("WVR command is invalid");
                return null;
            }
            else
            {
                List<PlaneData> planes = null;
                parseToObjects(wvrCmdResult, out planes);
                return planes;
            }
        }

        private static bool parseToObjects<T>(string str, out List<T> sceneObjs) where T: SceneObjectData, new()
        {
            try
            {
                string[] shapeStrs = str.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                sceneObjs = new List<T>();

                foreach (string shapeStr in shapeStrs)
                {
                    T obj = new T();
                    if (obj.FromString(shapeStr))
                    {
                        sceneObjs.Add(obj);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Error occurred when parsing scene components from WVR: " + e.Message);
                sceneObjs = null;
                return false;
            }
        }
    }
}