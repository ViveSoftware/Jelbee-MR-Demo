using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Essence.ScenePerception.Sample;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
    [Serializable]
    public class SpatialAnchorHelper
    {
        private bool needAnchorEnumeration = false;
        
        private ulong[] anchorHandles = null;
		private Dictionary<ulong, GameObject> AnchorDictionary = new Dictionary<ulong, GameObject>();

		private ScenePerceptionManager scenePerceptionManager;
        private readonly GameObject anchorPrefab;

        public SpatialAnchorHelper(ScenePerceptionManager scenePerceptionManager,GameObject anchorPrefab)
        {
            this.scenePerceptionManager = scenePerceptionManager;
            this.anchorPrefab = anchorPrefab;
        }

        public void SetAnchorsShouldBeUpdated()
        {
            needAnchorEnumeration = true;
        }
        
        public void UpdateAnchorDictionary()
        {
            WVR_Result result;

            if (anchorHandles == null || needAnchorEnumeration)
            {
                result = scenePerceptionManager.GetSpatialAnchors(out anchorHandles);
                if (result != WVR_Result.WVR_Success)
                {
                    Debug.LogError("Failed to get spatial anchors");
                    return;
                }
                needAnchorEnumeration = false;
            }

            if (anchorHandles == null) return;
			
            foreach (ulong anchorHandle in anchorHandles)
            {
                WVR_SpatialAnchorState currentAnchorState = default(WVR_SpatialAnchorState);
                result = scenePerceptionManager.GetSpatialAnchorState(anchorHandle, ScenePerceptionManager.GetCurrentPoseOriginModel(), out currentAnchorState);
                if (result == WVR_Result.WVR_Success)
                {
                    Debug.Log("Anchor Tracking State: " + currentAnchorState.trackingState.ToString());
                    switch(currentAnchorState.trackingState)
                    {
                        case WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Tracking:
                        {
                            if (!AnchorDictionary.ContainsKey(anchorHandle)) //Create Anchor Object
                            {
                                GameObject newAnchorGameObject = CreateNewAnchor(anchorHandle, currentAnchorState);

                                AnchorDictionary.Add(anchorHandle, newAnchorGameObject);
                            }
                            else //Anchor is already in dictionary
                            {
                                CheckAnchorPose(anchorHandle,currentAnchorState);
                            }

                            break;
                        }
                        case WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Paused:
                        case WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Stopped:
                        default:
                        {
                            //Remove from dictionary if exists
                            if (AnchorDictionary.ContainsKey(anchorHandle))
                            {
                                UnityEngine.Object.Destroy(AnchorDictionary[anchorHandle]); //Destroy Anchor GO
                                AnchorDictionary.Remove(anchorHandle);
                            }
                            break;
                        }
                    }
                }
            }
        }
        public void HandleAnchorUpdateDestroy(RaycastHit raycastHit)
        {
            if (raycastHit.collider == null) return;
            if (raycastHit.collider.transform.GetComponent<AnchorPrefab>() == null) return;
            
            ulong targetAnchorHandle = raycastHit.collider.transform.GetComponent<AnchorPrefab>().anchorHandle;

            var result = scenePerceptionManager.DestroySpatialAnchor(targetAnchorHandle);
            if (result == WVR_Result.WVR_Success)
            {
                UnityEngine.Object.Destroy(AnchorDictionary[targetAnchorHandle]);
                AnchorDictionary.Remove(targetAnchorHandle);

                needAnchorEnumeration = true;

                UpdateAnchorDictionary();
            }
        }

		public void HandleAnchorUpdateCreate(RaycastHit raycastHit, Quaternion anchorRotationUnity)
		{
			WVR_Result result;

			if (raycastHit.collider != null)
			{
				if (raycastHit.collider.transform.GetComponent<AnchorPrefab>() == null) //Collider hit is not an anchor (Create)
				{
					Vector3 anchorWorldPositionUnity = raycastHit.point;

					string anchorNameString = "SpatialAnchor_" + (AnchorDictionary.Count + 1);
					char[] anchorNameArray = anchorNameString.ToCharArray();

					result = scenePerceptionManager.CreateSpatialAnchor(anchorNameArray, anchorWorldPositionUnity, anchorRotationUnity, ScenePerceptionManager.GetCurrentPoseOriginModel(), out ulong newAnchorHandle, true);
					if (result == WVR_Result.WVR_Success)
					{
						needAnchorEnumeration = true;

						UpdateAnchorDictionary();
					}
				}
			}
		}

		private GameObject CreateNewAnchor(ulong anchorHandle,WVR_SpatialAnchorState anchorState)
        {
			GameObject newAnchorGameObject = UnityEngine.Object.Instantiate(anchorPrefab);
			AnchorPrefab newAnchorPrefabInstance = newAnchorGameObject.GetComponent<AnchorPrefab>();

			newAnchorPrefabInstance.anchorHandle = anchorHandle;
            newAnchorPrefabInstance.currentAnchorState = anchorState;

            SetAnchorPoseInScene(newAnchorPrefabInstance, anchorState);
            return newAnchorGameObject;
        }

        private void CheckAnchorPose(ulong anchorHandle,WVR_SpatialAnchorState anchorState)
        {
            //Check anchor pose
            AnchorPrefab currentAnchorPrefabInstance = AnchorDictionary[anchorHandle].GetComponent<AnchorPrefab>();
            if (currentAnchorPrefabInstance == null)
            {
                Debug.LogError("Anchor prefab gameobject deleted but the internals are still hanging out");
                needAnchorEnumeration = true;
                return;
            }

            if (!ScenePerceptionManager.AnchorStatePoseEqual(currentAnchorPrefabInstance.currentAnchorState, anchorState)) //Pose different, update
            {
                SetAnchorPoseInScene(currentAnchorPrefabInstance, anchorState);
				
                currentAnchorPrefabInstance.currentAnchorState = anchorState;
            }
        }

        private void SetAnchorPoseInScene(AnchorPrefab anchorPrefab, WVR_SpatialAnchorState anchorState)
        {
            scenePerceptionManager.ApplyTrackingOriginCorrectionToAnchorPose(anchorState, out Vector3 currentAnchorPosition, out Quaternion currentAnchorRotation);
            anchorPrefab.transform.SetPositionAndRotation(currentAnchorPosition,currentAnchorRotation);
        }
		
        public void ClearAnchors()
        {
            foreach (KeyValuePair<ulong, GameObject> anchorPair in AnchorDictionary)
            {
                var anchor = anchorPair.Value;
                if (anchor == null)
                {
                    Debug.LogWarning("Anchor deleted without being cleaned up in anchor manager");
                    continue;
                }
                UnityEngine.Object.Destroy(anchor);
            }

            AnchorDictionary.Clear();
        }
    }
}
