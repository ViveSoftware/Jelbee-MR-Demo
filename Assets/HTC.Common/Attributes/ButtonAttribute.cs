using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Common
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string ButtonLabel;

        public ButtonAttribute(string buttonLabel = null)
        {
            ButtonLabel = buttonLabel;
        }
    }
}