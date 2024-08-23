// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Wave.Native;

namespace Wave.Essence.Controller.Model.Demo
{
	public class UpdateCanvas : MonoBehaviour
	{
		public GameObject rightIsValid = null;
		public GameObject rightName = null;
		public GameObject rightManufacturer = null;
		public GameObject rightSerialNumber = null;
		public GameObject rightPoseTracking = null;

		private Text rightIsValidText = null;
		private Text rightNameText = null;
		private Text rightManufacturerText = null;
		private Text rightSerialNumberText = null;
		private Text rightPoseTrackingText = null;

		public GameObject leftIsValid = null;
		public GameObject leftName = null;
		public GameObject leftManufacturer = null;
		public GameObject leftSerialNumber = null;
		public GameObject leftPoseTracking = null;

		private Text leftIsValidText = null;
		private Text leftNameText = null;
		private Text leftManufacturerText = null;
		private Text leftSerialNumberText = null;
		private Text leftPoseTrackingText = null;

		public GameObject hmd = null;
		public GameObject rightController = null;
		public GameObject leftController = null;

		private Text hmdText = null;
		private Text rightControllerText = null;
		private Text leftControllerText = null;

		void OnEnable()
		{
		}

		void OnDisable()
		{
		}

		// Start is called before the first frame update
		void Start()
		{
			if (rightIsValid != null)
			{
				rightIsValidText = rightIsValid.GetComponent<Text>();
			}

			if (rightName != null)
			{
				rightNameText = rightName.GetComponent<Text>();
			}

			if (rightManufacturer != null)
			{
				rightManufacturerText = rightManufacturer.GetComponent<Text>();
			}

			if (rightSerialNumber != null)
			{
				rightSerialNumberText = rightSerialNumber.GetComponent<Text>();
			}

			if (rightPoseTracking != null)
			{
				rightPoseTrackingText = rightPoseTracking.GetComponent<Text>();
			}

			if (leftIsValid != null)
			{
				leftIsValidText = leftIsValid.GetComponent<Text>();
			}

			if (leftName != null)
			{
				leftNameText = leftName.GetComponent<Text>();
			}

			if (leftManufacturer != null)
			{
				leftManufacturerText = leftManufacturer.GetComponent<Text>();
			}

			if (leftSerialNumber != null)
			{
				leftSerialNumberText = leftSerialNumber.GetComponent<Text>();
			}

			if (leftPoseTracking != null)
			{
				leftPoseTrackingText = leftPoseTracking.GetComponent<Text>();
			}

			if (hmd != null)
			{
				hmdText = hmd.GetComponent<Text>();
			}

			if (rightController != null)
			{
				rightControllerText = rightController.GetComponent<Text>();
			}

			if (leftController != null)
			{
				leftControllerText = leftController.GetComponent<Text>();
			}
		}

		// Update is called once per frame
		void Update()
		{
			InputDevice rightdevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

			if (rightdevice.isValid)
			{
				if (rightIsValidText != null)
					rightIsValidText.text = "Device is valid";

				if (rightNameText != null)
					rightNameText.text = "Name: " + rightdevice.name;

				if (rightManufacturerText != null)
					rightManufacturerText.text = "Manufacturer: " + rightdevice.manufacturer;

				if (rightSerialNumberText != null)
					rightSerialNumberText.text = "Serial number: " + rightdevice.serialNumber;

				bool validPoseState;

				if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(new InputFeatureUsage<bool>("IsTracked"), out validPoseState)
					&& validPoseState)
				{
					if (rightPoseTrackingText != null)
						rightPoseTrackingText.text = "Pose is updated!";
				} else
				{
					if (rightPoseTrackingText != null)
						rightPoseTrackingText.text = "Pose is not available!";
				}
			} else
			{
				if (rightIsValidText != null)
					rightIsValidText.text = "Device is not valid";

				if (rightNameText != null)
					rightNameText.text = "Device is not valid";

				if (rightManufacturerText != null)
					rightManufacturerText.text = "Device is not valid";

				if (rightSerialNumberText != null)
					rightSerialNumberText.text = "Device is not valid";

				if (rightPoseTrackingText != null)
					rightPoseTrackingText.text = "Device is not valid";
			}

			InputDevice leftdevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

			if (leftdevice.isValid)
			{
				if (leftIsValidText != null)
					leftIsValidText.text = "Device is valid";

				if (leftNameText != null)
					leftNameText.text = "Name: " + leftdevice.name;

				if (leftManufacturerText != null)
					leftManufacturerText.text = "Manufacturer: " + leftdevice.manufacturer;

				if (leftSerialNumberText != null)
					leftSerialNumberText.text = "Serial number: " + leftdevice.serialNumber;

				bool validPoseState;

				if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(new InputFeatureUsage<bool>("IsTracked"), out validPoseState)
					&& validPoseState)
				{
					if (leftPoseTrackingText != null)
						leftPoseTrackingText.text = "Pose is updated!";
				}
				else
				{
					if (leftPoseTrackingText != null)
						leftPoseTrackingText.text = "Pose is not available!";
				}
			}
			else
			{
				if (leftIsValidText != null)
					leftIsValidText.text = "Device is not valid";

				if (leftNameText != null)
					leftNameText.text = "Device is not valid";

				if (leftManufacturerText != null)
					leftManufacturerText.text = "Device is not valid";

				if (leftSerialNumberText != null)
					leftSerialNumberText.text = "Device is not valid";

				if (leftPoseTrackingText != null)
					leftPoseTrackingText.text = "Device is not valid";
			}

			#region Pose
			WVR_DeviceType[] checkDevice = { WVR_DeviceType.WVR_DeviceType_HMD, WVR_DeviceType.WVR_DeviceType_Controller_Right, WVR_DeviceType.WVR_DeviceType_Controller_Left };
			foreach (WVR_DeviceType deviceType in checkDevice)
			{
				Text deviceText = null;
				bool isTracked = WaveEssence.Instance.IsTracked(deviceType);
				switch (deviceType)
				{
					case WVR_DeviceType.WVR_DeviceType_HMD:
						deviceText = hmdText;
						if (deviceText == null)
							continue;
						if (isTracked)
							deviceText.text = "HMD is Tracked.\n";
						else
						{
							deviceText.text = "HMD lose Tracking.\n";
							continue;
						}
						break;
					case WVR_DeviceType.WVR_DeviceType_Controller_Right:
						deviceText = rightControllerText;
						if (deviceText == null)
							continue;
						if (isTracked)
							deviceText.text = "Right controller is Tracked.\n";
						else
						{
							deviceText.text = "Right controller lose Tracking.\n";
							continue;
						}
						break;
					case WVR_DeviceType.WVR_DeviceType_Controller_Left:
						deviceText = leftControllerText;
						if (deviceText == null)
							continue;
						if (isTracked)
							deviceText.text = "Left controller is Tracked.\n";
						else
						{
							deviceText.text = "Left controller lose Tracking.\n";
							continue;
						}
						break;
				}
				Vector3 pos = WaveEssence.Instance.GetDevicePosition(deviceType);
				Quaternion rot = WaveEssence.Instance.GetDeviceRotation(deviceType);
				Vector3 vel = WaveEssence.Instance.GetDeviceVelocity(deviceType);
				Vector3 angVel = WaveEssence.Instance.GetDeviceAngularVelocity(deviceType);
				if (pos != Vector3.zero)
					deviceText.text += $"position : {pos}\n";
				if (rot != Quaternion.identity)
					deviceText.text += $"rotation : {rot}\n";
				if (vel != Vector3.zero)
					deviceText.text += $"velocity : {vel}\n";
				if (angVel != Vector3.zero)
					deviceText.text += $"angular velocity : {angVel}\n";
			}

			#endregion

			#region Haptic
			Dictionary<XRNode, WVR_DeviceType> handTypes = new Dictionary<XRNode, WVR_DeviceType>()
			{
				{ XRNode.RightHand, WVR_DeviceType.WVR_DeviceType_Controller_Right},
				{ XRNode.LeftHand, WVR_DeviceType.WVR_DeviceType_Controller_Left}
			};

			foreach (var handType in handTypes)
			{
				Text deviceText = handType.Key == XRNode.LeftHand ? leftControllerText : rightControllerText;

				bool press;
				InputDevices.GetDeviceAtXRNode(handType.Key).TryGetFeatureValue(XR_Feature.primaryButton, out press);
				if (press)
				{
					WVR_InputId inputButton = handType.Key == XRNode.LeftHand ? WVR_InputId.WVR_InputId_Alias1_X : WVR_InputId.WVR_InputId_Alias1_A;
					uint durationMilliSec = 2500;
					WaveEssence.Instance.SendHapticImpulse(handType.Value, inputButton, durationMilliSec, intensity: WVR_Intensity.WVR_Intensity_Weak);
					deviceText.text += $"vibration :\n button : PrimaryButton,\n duration : {durationMilliSec} millisecond,\n intensity : Weak\n";
				}
				InputDevices.GetDeviceAtXRNode(handType.Key).TryGetFeatureValue(XR_Feature.secondaryButton, out press);
				if (press)
				{
					WVR_InputId inputButton = handType.Key == XRNode.LeftHand ? WVR_InputId.WVR_InputId_Alias1_Y : WVR_InputId.WVR_InputId_Alias1_B;
					uint durationMilliSec = 2000;
					WaveEssence.Instance.SendHapticImpulse(handType.Value, inputButton, durationMilliSec, intensity: WVR_Intensity.WVR_Intensity_Light);
					deviceText.text += $"vibration :\n button : SecondaryButton,\n duration : {durationMilliSec} millisecond,\n intensity : Light\n";
				}
				InputDevices.GetDeviceAtXRNode(handType.Key).TryGetFeatureValue(XR_Feature.primary2DAxisClick, out press);
				if (press)
				{
					uint durationMilliSec = 1500;
					WaveEssence.Instance.SendHapticImpulse(handType.Value, WVR_InputId.WVR_InputId_Alias1_Thumbstick, durationMilliSec, intensity: WVR_Intensity.WVR_Intensity_Normal);
					deviceText.text += $"vibration :\n button : Thumbstick,\n duration : {durationMilliSec} millisecond,\n intensity : Normal\n";
				}
				InputDevices.GetDeviceAtXRNode(handType.Key).TryGetFeatureValue(XR_Feature.gripButton, out press);
				if (press)
				{
					uint durationMilliSec = 1000;
					WaveEssence.Instance.SendHapticImpulse(handType.Value, WVR_InputId.WVR_InputId_Alias1_Grip, durationMilliSec, intensity: WVR_Intensity.WVR_Intensity_Strong);
					deviceText.text += $"vibration :\n button : GripButton,\n duration : {durationMilliSec} millisecond,\n intensity : Strong\n";
				}
				InputDevices.GetDeviceAtXRNode(handType.Key).TryGetFeatureValue(XR_Feature.triggerButton, out press);
				if (press)
				{
					uint durationMilliSec = 500;
					WaveEssence.Instance.SendHapticImpulse(handType.Value, WVR_InputId.WVR_InputId_Alias1_Trigger, durationMilliSec, intensity: WVR_Intensity.WVR_Intensity_Severe);
					deviceText.text += $"vibration :\n button : TriggerButton,\n duration : {durationMilliSec} millisecond,\n intensity : Severe\n";
				}
			}
			#endregion

		}
	}
}
