using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
	public class AnchorPrefab : MonoBehaviour
	{
		bool dirty = false;
		bool isAnchorHandleSet = false;
		ulong anchorHandle = 0;
		SpatialAnchorTrackingState trackingState = SpatialAnchorTrackingState.Stopped;
		Pose pose;
		string anchorName;

		// The mesh to show the anchor state
		[SerializeField] private MeshRenderer stateRenderer;

		void Start()
		{
			trackingState = SpatialAnchorTrackingState.Stopped;
			stateRenderer.material = new Material(stateRenderer.sharedMaterial);
		}

		public void SetAnchorHandle(ulong handle)
		{
			if (isAnchorHandleSet)
				throw new System.Exception("Anchor handle is already set.");
			anchorHandle = handle;
			isAnchorHandleSet = true;
			dirty = true;
		}

		public ulong GetAnchorHandle()
		{
			if (!isAnchorHandleSet)
				throw new System.Exception("Anchor handle is not set.");
			return anchorHandle;
		}

		public void SetAnchorState(SpatialAnchorTrackingState currentAnchorState, Pose pose)
		{
			trackingState = currentAnchorState;
			this.pose = pose;
			dirty = true;
		}

		public void SetAnchorName(string name)
		{
			anchorName = name;
		}

		public SpatialAnchorTrackingState GetTrackingState()
		{
			return trackingState;
		}

		public Pose GetPose()
		{
			return pose;
		}

		public string GetName()
		{
			return anchorName;
		}

		private void Update()
		{
			if (!dirty) return;

			switch (trackingState)
			{
				default:
				case SpatialAnchorTrackingState.Stopped:
					stateRenderer.material.SetFloat("_Cutoff", 1);  // Complete gone
					break;
				case SpatialAnchorTrackingState.Tracking:
					stateRenderer.material.SetFloat("_Cutoff", 0.5f);  // A solid circle
					break;
				case SpatialAnchorTrackingState.Paused:
					stateRenderer.material.SetFloat("_Cutoff", 0.15f);  // A cirle with black border
					break;
			}
		}
	}
}
