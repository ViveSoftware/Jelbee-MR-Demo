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

namespace Wave.Essence.Interaction.Mode.Demo
{
	public class ControllerPoseModeTest : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Controller.Model.Demo.ControllerPoseModeTest";
		void DEBUG(string msg) { Log.d(LOG_TAG, m_Controller + " " + msg, true); }

		[SerializeField]
		private XR_Hand m_Controller = XR_Hand.Dominant;
		public XR_Hand Controller { get { return m_Controller; } set { m_Controller = value; } }

		[SerializeField]
		private Text m_Text = null;
		public Text ModeText { get { return m_Text; } set { m_Text = value; } }

		private void Start()
		{
			if (WXRDevice.GetRoleDevice(XR_Device.Head).TryGetFeatureValue(XR_Feature.userPresence, out bool value))
				DEBUG("Start() userPresence: " + value);
			else
				DEBUG("Start() Not support InputFeature - userPresence.");
		}

		void Update()
		{
			if (m_Text == null)
				return;
			if (WXRDevice.GetControllerPoseMode(m_Controller, out XR_ControllerPoseMode PoseMode))
			{
				switch(PoseMode)
				{
					case XR_ControllerPoseMode.Trigger:
						m_Text.text = (m_Controller == XR_Hand.Dominant ? "Right" : "Left") + ": Trigger";
						break;
					case XR_ControllerPoseMode.Panel:
						m_Text.text = (m_Controller == XR_Hand.Dominant ? "Right" : "Left") + ": Panel";
						break;
					case XR_ControllerPoseMode.Handle:
						m_Text.text = (m_Controller == XR_Hand.Dominant ? "Right" : "Left") + ": Handle";
						break;
					default:
						m_Text.text = (m_Controller == XR_Hand.Dominant ? "Right" : "Left") + ": Raw";
						break;
				}
			}
		}

		public void SetTriggerMode()
		{
			if (WXRDevice.SetControllerPoseMode(m_Controller, XR_ControllerPoseMode.Trigger))
				DEBUG("SetTriggerMode() succeeded.");
			else
				DEBUG("SetTriggerMode() failed.");
		}

		public void SetPanelMode()
		{
			if (WXRDevice.SetControllerPoseMode(m_Controller, XR_ControllerPoseMode.Panel))
				DEBUG("SetPanelMode() succeeded.");
			else
				DEBUG("SetPanelMode() failed.");
		}

		public void SetHandleMode()
		{
			if (WXRDevice.SetControllerPoseMode(m_Controller, XR_ControllerPoseMode.Handle))
				DEBUG("SetHandleMode() succeeded.");
			else
				DEBUG("SetHandleMode() failed.");
		}
	}
}
