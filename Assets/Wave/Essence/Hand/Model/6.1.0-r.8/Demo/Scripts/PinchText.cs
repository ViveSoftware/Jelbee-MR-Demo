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

namespace Wave.Essence.Hand.Model.Demo
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Text))]
	public class PinchText : MonoBehaviour
	{
		public bool isLeft = false;
		private Text m_Text = null;

		private void Awake()
		{
			m_Text = GetComponent<Text>();
		}

		private Vector3 origin = Vector3.zero, direction = Vector3.zero;
		void Update()
		{
			if (m_Text == null) { return; }

			HandManager.Instance.GetPinchOrigin(ref origin, isLeft);
			HandManager.Instance.GetPinchDirection(ref direction, isLeft);

			m_Text.text = (isLeft ? "Left Hand: " : "Right Hand: ");
			m_Text.text += "\nPinch Origin( " + origin.x.ToString("F3") + ", " + origin.y.ToString("F3") + ", " + origin.z.ToString("F3") + ")";
			m_Text.text += "\nPinch Direction( " + direction.x.ToString("F3") + ", " + direction.y.ToString("F3") + ", " + direction.z.ToString("F3") + ")";
		}
	}
}
