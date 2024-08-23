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

namespace Wave.Essence.Hand.Model.Demo
{
	[DisallowMultipleComponent]
	public class NaturalHandGrasp : MonoBehaviour
	{
		[SerializeField]
		private HandManager.HandType m_Hand = HandManager.HandType.Right;
		public HandManager.HandType Hand { get { return m_Hand; } set { m_Hand = value; } }
		private bool IsLeft => m_Hand == HandManager.HandType.Left;

		private Vector3 palmPosition = Vector3.zero;
		private Quaternion palmRotation = Quaternion.identity;
		private GameObject graspObject = null;
		private Vector3 graspObjectPosition = Vector3.zero;
		private Quaternion graspObjectRoation = Quaternion.identity;

		private void Update()
		{
			UpdateHandJoint();
			if ((graspObject || IsHandTouchCollider()) && HandManager.Instance.IsHandGrasping(IsLeft))
			{
				Vector3 pos = palmPosition - graspObjectPosition;
				Quaternion rot = graspObjectRoation * palmRotation;
				graspObject.transform.SetPositionAndRotation(pos, rot);
				graspObject.transform.LookAt(palmPosition);
			}
			else
			{
				graspObject = null;
			}
		}

		private void UpdateHandJoint()
		{
			HandManager.Instance.GetJointPosition(HandManager.HandJoint.Palm, ref palmPosition, IsLeft);
			HandManager.Instance.GetJointRotation(HandManager.HandJoint.Palm, ref palmRotation, IsLeft);
		}

		private bool IsHandTouchCollider()
		{
			Vector3 center = palmPosition + new Vector3(palmPosition.x, 0.0f, palmPosition.z) / 10;
			Collider[] hitColliders = Physics.OverlapSphere(palmPosition, 0.05f);
			foreach (var hitCollider in hitColliders)
			{
				if (hitCollider.name == "SphereR" || hitCollider.name == "SphereL")
				{
					graspObject = hitCollider.gameObject;
					graspObjectPosition = palmPosition - graspObject.transform.position;
					graspObjectRoation = Quaternion.Inverse(graspObjectRoation);
					return true;
				}
			}
			return false;
		}
	}
}
