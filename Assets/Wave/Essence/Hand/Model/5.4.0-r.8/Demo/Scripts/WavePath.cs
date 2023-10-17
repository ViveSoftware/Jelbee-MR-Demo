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
using Wave.Native;

namespace Wave.Essence.Hand.Model.Demo
{
	[RequireComponent(typeof(HandManager))]
	public class WavePath : MonoBehaviour
	{
		internal HandManager mInst = null;
		private void Awake()
		{
			mInst = GetComponent<HandManager>();
		}
		bool isWavePath = false, finished = false;
		private void Update()
		{
			if (mInst == null || finished) { return; }

			if (!isWavePath &&
				(mInst.GetHandTrackerStatus() == HandManager.TrackerStatus.Available)
			)
			{
				Log.d("WavePath", "Stop XR Hand Tracking.", true);
				mInst.StopHandTracker(HandManager.TrackerType.Natural);
				isWavePath = true;
			}
			if (isWavePath && mInst.GetHandTrackerStatus() == HandManager.TrackerStatus.NotStart)
			{
				Log.d("WavePath", "Change to wave path.", true);
				mInst.UseXRDevice = false;
				Log.d("WavePath", "Start Wave Hand Tracking.", true);
				mInst.StartHandTracker(HandManager.TrackerType.Natural);
				finished = true;
			}
		}
	}
}
