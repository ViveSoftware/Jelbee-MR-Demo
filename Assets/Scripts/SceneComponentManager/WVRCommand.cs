using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.WVRLoader
{
    public class WVRCommand
    {
        public static string CreateCommand_GetSceneComponents( 
            string uuid = null,  
            ObjectTypeEnum objectType = ObjectTypeEnum.ScenePlane, 
            ShapeTypeEnum type = ShapeTypeEnum.all, 
            string tag = null)
        {
            string command = string.Format("getSceneComponents,{0},{1},{2},{3}",
                string.IsNullOrEmpty(uuid) ? "0" : uuid,
                objectType.ToString(),
                type == ShapeTypeEnum.all ? " " : type.ToString(),
                string.IsNullOrEmpty(tag) ? " " : tag);

            return command;
        }
    }
}