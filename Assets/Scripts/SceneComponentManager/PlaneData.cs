using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.WVRLoader
{
    public enum ObjectTypeEnum
    {
        ScenePlane,
    }

    public enum ShapeTypeEnum
    {
        all,
        wall,
        table,
        window,
        door,
        floor,
        ceiling,
        chair
    }

    public abstract class SceneObjectData
    {
        public abstract bool FromString(string objectStr);
    }

    public class PlaneData : SceneObjectData
    {
        public string Type;
        public string Tag;
        public string UID;
        public string ParentID;
        public Vector3[] Points;
        public Vector3 Center;
        public Vector4 Orientation;
        public float Width;
        public float Height;

        public override string ToString()
        {
            return string.Format("ScenePlane,{0},{1},{2},{3},{4},{5},{6},{7}", Type, Tag, UID, ParentID, Points[0], Points[1], Points[2], Points[3]);
        }

        //Updated: 20220905
        //Format: "'ObjectIdentity','realworld type','tag,uuid','parent's uuid'(='0' if no parent),'p1(Vector3) p2(Vector3) p3(Vector3) p4(Vector3) center(Vector3) forward(Vector3) width(float) height(float)'
        //Right Hand to Left Hand coordinate
        public override bool FromString(string objectStr)
        {
            Debug.LogFormat("Parse plane: {0}", objectStr);

            string[] shapeParams = objectStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (shapeParams.Length != 6)
            {
                Debug.Log("Plane string format is invalid, ignore it.");
                return false;
            }

            Type = shapeParams[1];
            Tag = shapeParams[2];
            UID = shapeParams[3];
            ParentID = shapeParams[4];

            string[] numParams = shapeParams[5].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Points = new Vector3[4]
            {
                    new Vector3(float.Parse(numParams[0]), float.Parse(numParams[1]), -float.Parse(numParams[2])),
                    new Vector3(float.Parse(numParams[3]), float.Parse(numParams[4]), -float.Parse(numParams[5])),
                    new Vector3(float.Parse(numParams[6]), float.Parse(numParams[7]), -float.Parse(numParams[8])),
                    new Vector3(float.Parse(numParams[9]), float.Parse(numParams[10]), -float.Parse(numParams[11])),
            };
            
            Center = new Vector3(float.Parse(numParams[12]), float.Parse(numParams[13]), -float.Parse(numParams[14]));
            Orientation = new Vector4(-float.Parse(numParams[15]), -float.Parse(numParams[16]), float.Parse(numParams[17]), -float.Parse(numParams[18]));
            Width = float.Parse(numParams[19]);
            Height = float.Parse(numParams[20]);

            return true;
        }

        public Vector3 Normal
        {
            get
            {
                return Vector3.Cross(
                    Points[1] - Points[0],
                    Points[2] - Points[0]).normalized;
            }
        }
    }    
}