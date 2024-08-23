// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.Hand.Model.Demo
{
	[RequireComponent(typeof(Button))]
	public class ActivateHandTracking : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.Hand.Model.Demo.ActivateHandTracking";
		StringBuilder m_sb = null;
		StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Log.d(LOG_TAG, msg, true); }

		public HandManager.TrackerType Hand = HandManager.TrackerType.Natural;

		private Button m_btn = null;
		private void Awake()
		{
			m_btn = GetComponent<Button>();
		}
		private HandManager.TrackerStatus m_Status = HandManager.TrackerStatus.NoSupport;
		private void Update()
		{
			if (HandManager.Instance == null || m_btn == null) { return; }

			m_Status = HandManager.Instance.GetHandTrackerStatus(Hand);
			if (m_Status == HandManager.TrackerStatus.Starting || m_Status == HandManager.TrackerStatus.Stopping || m_Status == HandManager.TrackerStatus.NoSupport)
				m_btn.interactable = false;
			else
				m_btn.interactable = true;

			if (m_Status == HandManager.TrackerStatus.Available)
				m_btn.GetComponentInChildren<Text>().text = "Stop Hand Tracking";
			else
				m_btn.GetComponentInChildren<Text>().text = "Start Hand Tracking";
		}

		public void Activate()
		{
			if (m_Status == HandManager.TrackerStatus.Available)
			{
				sb.Clear().Append("Activate() StopHandTracker(").Append(Hand.Name()).Append(")"); DEBUG(sb);
				HandManager.Instance.StopHandTracker(Hand);
			}
			else
			{
				sb.Clear().Append("Activate() StartHandTracker(").Append(Hand.Name()).Append(")"); DEBUG(sb);
				HandManager.Instance.StartHandTracker(Hand);
			}
		}
	}
}
