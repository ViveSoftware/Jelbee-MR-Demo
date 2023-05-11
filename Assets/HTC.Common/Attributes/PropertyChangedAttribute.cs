using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Common
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class PropertyChangedAttribute : PropertyAttribute
    {
        public string methodName;

        public PropertyChangedAttribute(string methodNameNoArguments)
        {
            methodName = methodNameNoArguments;
        }
    }
}
