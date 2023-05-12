using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace Wave.Essence.ScenePerception.Editor
{
	using UnityEditor;
	using Wave.Essence.ScenePerception;

	[CustomEditor(typeof(ScenePerceptionManager))]
	public class ScenePerceptionManagerEditor : Editor
	{
		static string PropertyName_TrackingOrigin = "trackingOrigin";
		static GUIContent Label_TrackingOrigin = new GUIContent("Tracking Origin", "Assign the current Trackign Origin here (e.g. Root of a VR Camera Rig) for pose correction.");
		SerializedProperty Property_TrackingOrigin;

		public override void OnInspectorGUI()
		{
			if (Property_TrackingOrigin == null) Property_TrackingOrigin = serializedObject.FindProperty(PropertyName_TrackingOrigin);

			EditorGUILayout.HelpBox("Scene Perception Manager is a component which provided APIs related to the Scene Perception feature.", MessageType.None);

			EditorGUILayout.PropertyField(Property_TrackingOrigin, new GUIContent(Label_TrackingOrigin));
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
