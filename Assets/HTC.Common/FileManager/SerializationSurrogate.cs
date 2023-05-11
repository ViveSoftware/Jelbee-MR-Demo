using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace com.HTC.Common
{
    public class Vector3IntSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var vec3int = (Vector3Int)obj;
            info.AddValue("x", vec3int.x);
            info.AddValue("y", vec3int.y);
            info.AddValue("z", vec3int.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var vec3int = (Vector3Int)obj;
            vec3int.x = info.GetInt32("x");
            vec3int.y = info.GetInt32("y");
            vec3int.z = info.GetInt32("z");
            return vec3int;
        }
    }

    public class Vector2IntSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var vec2int = (Vector2Int)obj;
            info.AddValue("x", vec2int.x);
            info.AddValue("y", vec2int.y);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var vec2int = (Vector2Int)obj;
            vec2int.x = info.GetInt32("x");
            vec2int.y = info.GetInt32("y");
            return vec2int;
        }
    }

    public class Vector3Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var vec3 = (Vector3)obj;
            info.AddValue("x", vec3.x);
            info.AddValue("y", vec3.y);
            info.AddValue("z", vec3.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var vec3 = (Vector3)obj;
            vec3.x = info.GetSingle("x");
            vec3.y = info.GetSingle("y");
            vec3.z = info.GetSingle("z");
            return vec3;
        }
    }

    public class QuternionSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var quternion = (Quaternion)obj;
            info.AddValue("x", quternion.x);
            info.AddValue("y", quternion.y);
            info.AddValue("z", quternion.z);
            info.AddValue("w", quternion.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var quternion = (Quaternion)obj;
            quternion.Set(
                info.GetSingle("x"),
                info.GetSingle("y"),
                info.GetSingle("z"),
                info.GetSingle("w"));
            return quternion;
        }
    }
}
