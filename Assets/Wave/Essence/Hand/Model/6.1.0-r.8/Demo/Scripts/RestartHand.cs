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

namespace Wave.Essence.Hand.Model.Demo
{
	public class RestartHand : MonoBehaviour
	{
		public HandManager.TrackerType Hand = HandManager.TrackerType.Natural;
		public void RestartHandTracking()
		{
			if (HandManager.Instance == null) { return; }
			Native.Log.d("Wave.Essence.Hand.Model.Demo.RestartHand", "RestartHand()", true);
			HandManager.Instance.RestartHandTracker(Hand);
		}
	}
}
