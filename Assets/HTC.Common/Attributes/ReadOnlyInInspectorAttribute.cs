using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Common
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyInInspectorAttribute : PropertyAttribute
    {

    }
}
