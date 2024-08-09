// "Wave SDK 
// © 2023 HTC Corporation. All Rights Reserved.
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
	public class GraspText : MonoBehaviour
	{
		public bool isLeft = false;
		private Text m_Text = null;

		private void Awake()
		{
			m_Text = GetComponent<Text>();
		}

		private float strength = 0;
		private bool isGrasping = false;
		void Update()
		{
			if (m_Text == null) { return; }

			strength = HandManager.Instance.GetGraspStrength(isLeft);
			isGrasping = HandManager.Instance.IsHandGrasping(isLeft);

			m_Text.text += $"\nGrasp Strength : {strength}";
			m_Text.text += $"\nGrasping : {isGrasping}";
		}
	}
}
