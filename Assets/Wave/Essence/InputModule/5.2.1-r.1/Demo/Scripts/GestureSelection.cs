// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using UnityEngine.UI;
using Wave.Native;
using Wave.Essence.Hand;

namespace Wave.Essence.InputModule.Demo
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Toggle))]
	public class GestureSelection : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.InputModule.Demo.GestureSelection";
		void DEBUG(string msg)
		{
			if (Log.EnableDebugLog)
				Log.d(LOG_TAG, msg, true);
		}

		public HandManager.GestureType Gesture = HandManager.GestureType.Unknown;

		Toggle m_Toggle = null;
		private void Awake()
		{
			m_Toggle = GetComponent<Toggle>();
		}
		private void Start()
		{
			if (m_Toggle == null || HandManager.Instance == null) { return; }

			switch (Gesture)
			{
				case HandManager.GestureType.Fist:
					m_Toggle.isOn = HandManager.Instance.GestureOptions.Gesture.Fist;
					break;
				case HandManager.GestureType.Five:
					m_Toggle.isOn = HandManager.Instance.GestureOptions.Gesture.Five;
					break;
				case HandManager.GestureType.OK:
					m_Toggle.isOn = HandManager.Instance.GestureOptions.Gesture.OK;
					break;
				case HandManager.GestureType.ThumbUp:
					m_Toggle.isOn = HandManager.Instance.GestureOptions.Gesture.ThumbUp;
					break;
				case HandManager.GestureType.IndexUp:
					m_Toggle.isOn = HandManager.Instance.GestureOptions.Gesture.IndexUp;
					break;
				case HandManager.GestureType.Palm_Pinch:
					m_Toggle.isOn = HandManager.Instance.GestureOptions.Gesture.Palm_Pinch;
					break;
				case HandManager.GestureType.Yeah:
					m_Toggle.isOn = HandManager.Instance.GestureOptions.Gesture.Yeah;
					break;
				default:
					break;
			}

			DEBUG("Start() " + Gesture + ": " + m_Toggle.isOn);
		}
		public void ActivateGesture()
		{
			if (m_Toggle == null || HandManager.Instance == null) { return; }
			DEBUG("ActivateGesture() " + Gesture + ": " + m_Toggle.isOn);

			switch (Gesture)
			{
				case HandManager.GestureType.Fist:
					HandManager.Instance.GestureOptions.Gesture.Fist = m_Toggle.isOn;
					break;
				case HandManager.GestureType.Five:
					HandManager.Instance.GestureOptions.Gesture.Five = m_Toggle.isOn;
					break;
				case HandManager.GestureType.OK:
					HandManager.Instance.GestureOptions.Gesture.OK = m_Toggle.isOn;
					break;
				case HandManager.GestureType.ThumbUp:
					HandManager.Instance.GestureOptions.Gesture.ThumbUp = m_Toggle.isOn;
					break;
				case HandManager.GestureType.IndexUp:
					HandManager.Instance.GestureOptions.Gesture.IndexUp = m_Toggle.isOn;
					break;
				case HandManager.GestureType.Palm_Pinch:
					HandManager.Instance.GestureOptions.Gesture.Palm_Pinch = m_Toggle.isOn;
					break;
				case HandManager.GestureType.Yeah:
					HandManager.Instance.GestureOptions.Gesture.Yeah = m_Toggle.isOn;
					break;
				default:
					break;
			}
		}
	}
}
