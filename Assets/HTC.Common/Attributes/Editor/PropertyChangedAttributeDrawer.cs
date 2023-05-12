using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.HTC.Common
{
    [CustomPropertyDrawer(typeof(PropertyChangedAttribute))]
    public class PropertyChangedAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            if (EditorGUI.EndChangeCheck())
            {
                PropertyChangedAttribute at = attribute as PropertyChangedAttribute;

                MethodInfo method = property.serializedObject.targetObject.GetType().GetMethod(at.methodName);

                if (method != null && method.GetParameters().Count() == 0)// Only instantiate methods with 0 parameters
                    method.Invoke(property.serializedObject.targetObject, null);
            }
        }
    }
}
