// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using Wave.Native;
using Wave.Essence.Events;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Wave.Essence.Extra;
using System.Text;
using UnityEngine.Profiling;
using UnityEngine.XR;
using Wave.XR;
using Wave.OpenXR;

namespace Wave.Essence.Hand.Model
{

	[DisallowMultipleComponent]
	public class HandMeshRenderer : MonoBehaviour
	{
		private const string TAG = "HandMeshRenderer";
		private InputDevice handDev = new InputDevice();
		private bool foundHandDev = false;

		private StringBuilder HSB
		{
			get
			{
				return Log.CSB.Append(IsLeft ? "Left, " : "Right, ");
			}
		}

		public class BoneMap
		{
			public BoneMap(HandManager.HandJoint b, string name, HandManager.HandJoint p)
			{
				BoneID = b;
				DisplayName = name;
				BoneParentID = p;
				BoneDiffFromParent = Vector3.zero;
			}

			public HandManager.HandJoint BoneID;
			public string DisplayName;
			public HandManager.HandJoint BoneParentID;
			public Vector3 BoneDiffFromParent;
		}

		private const int BONE_MAX_ID = 26;

		#region Name Definition
		// The order of joint name MUST align with runtime's definition
		private readonly string[] BNames = new string[]
		{
				"WaveBone_0",  // WVR_HandJoint_Palm = 0
				"WaveBone_1", // WVR_HandJoint_Wrist = 1
				"WaveBone_2", // WVR_HandJoint_Thumb_Joint0 = 2
				"WaveBone_3", // WVR_HandJoint_Thumb_Joint1 = 3
				"WaveBone_4", // WVR_HandJoint_Thumb_Joint2 = 4
				"WaveBone_5", // WVR_HandJoint_Thumb_Tip = 5
				"WaveBone_6", // WVR_HandJoint_Index_Joint0 = 6
				"WaveBone_7", // WVR_HandJoint_Index_Joint1 = 7
				"WaveBone_8", // WVR_HandJoint_Index_Joint2 = 8
				"WaveBone_9", // WVR_HandJoint_Index_Joint3 = 9
				"WaveBone_10", // WVR_HandJoint_Index_Tip = 10
				"WaveBone_11", // WVR_HandJoint_Middle_Joint0 = 11
				"WaveBone_12", // WVR_HandJoint_Middle_Joint1 = 12
				"WaveBone_13", // WVR_HandJoint_Middle_Joint2 = 13
				"WaveBone_14", // WVR_HandJoint_Middle_Joint3 = 14
				"WaveBone_15", // WVR_HandJoint_Middle_Tip = 15
				"WaveBone_16", // WVR_HandJoint_Ring_Joint0 = 16
				"WaveBone_17", // WVR_HandJoint_Ring_Joint1 = 17
				"WaveBone_18", // WVR_HandJoint_Ring_Joint2 = 18
				"WaveBone_19", // WVR_HandJoint_Ring_Joint3 = 19
				"WaveBone_20", // WVR_HandJoint_Ring_Tip = 20
				"WaveBone_21", // WVR_HandJoint_Pinky_Joint0 = 21
				"WaveBone_22", // WVR_HandJoint_Pinky_Joint0 = 22
				"WaveBone_23", // WVR_HandJoint_Pinky_Joint0 = 23
				"WaveBone_24", // WVR_HandJoint_Pinky_Joint0 = 24
				"WaveBone_25" // WVR_HandJoint_Pinky_Tip = 25
		};
		#endregion

		public BoneMap[] boneMap = new BoneMap[]
		{
			new BoneMap(HandManager.HandJoint.Palm, "Palm", HandManager.HandJoint.Wrist),  // 0
			new BoneMap(HandManager.HandJoint.Wrist, "Wrist", HandManager.HandJoint.Wrist),// 1
			new BoneMap(HandManager.HandJoint.Thumb_Joint0, "Thumb root", HandManager.HandJoint.Wrist), // 2
			new BoneMap(HandManager.HandJoint.Thumb_Joint1, "Thumb joint1", HandManager.HandJoint.Thumb_Joint0), // 3
			new BoneMap(HandManager.HandJoint.Thumb_Joint2, "Thumb joint2", HandManager.HandJoint.Thumb_Joint1), // 4
			new BoneMap(HandManager.HandJoint.Thumb_Tip, "Thumb tip", HandManager.HandJoint.Thumb_Joint2), // 5
			new BoneMap(HandManager.HandJoint.Index_Joint0, "Index root", HandManager.HandJoint.Wrist),  // 6
			new BoneMap(HandManager.HandJoint.Index_Joint1, "Index joint1", HandManager.HandJoint.Index_Joint0), // 7
			new BoneMap(HandManager.HandJoint.Index_Joint2, "Index joint2", HandManager.HandJoint.Index_Joint1), // 8
			new BoneMap(HandManager.HandJoint.Index_Joint3, "Index joint3", HandManager.HandJoint.Index_Joint2), // 9
			new BoneMap(HandManager.HandJoint.Index_Tip, "Index tip", HandManager.HandJoint.Index_Joint3), // 10
			new BoneMap(HandManager.HandJoint.Middle_Joint0, "Middle root", HandManager.HandJoint.Wrist), // 11
			new BoneMap(HandManager.HandJoint.Middle_Joint1, "Middle joint1", HandManager.HandJoint.Middle_Joint0), // 12
			new BoneMap(HandManager.HandJoint.Middle_Joint2, "Middle joint2", HandManager.HandJoint.Middle_Joint1), // 13
			new BoneMap(HandManager.HandJoint.Middle_Joint3, "Middle joint3", HandManager.HandJoint.Middle_Joint2), // 14
			new BoneMap(HandManager.HandJoint.Middle_Tip, "Middle tip", HandManager.HandJoint.Middle_Joint3), // 15
			new BoneMap(HandManager.HandJoint.Ring_Joint0, "Ring root", HandManager.HandJoint.Wrist), // 16
			new BoneMap(HandManager.HandJoint.Ring_Joint1, "Ring joint1", HandManager.HandJoint.Ring_Joint0), // 17
			new BoneMap(HandManager.HandJoint.Ring_Joint2, "Ring joint2", HandManager.HandJoint.Ring_Joint1), // 18
			new BoneMap(HandManager.HandJoint.Ring_Joint3, "Ring joint3", HandManager.HandJoint.Ring_Joint2), // 19
			new BoneMap(HandManager.HandJoint.Ring_Tip, "Ring tip", HandManager.HandJoint.Ring_Joint3), // 20
			new BoneMap(HandManager.HandJoint.Pinky_Joint0, "Pinky root", HandManager.HandJoint.Wrist), // 21
			new BoneMap(HandManager.HandJoint.Pinky_Joint1, "Pinky joint1", HandManager.HandJoint.Pinky_Joint0), // 22
			new BoneMap(HandManager.HandJoint.Pinky_Joint2, "Pinky joint2", HandManager.HandJoint.Pinky_Joint1), // 23
			new BoneMap(HandManager.HandJoint.Pinky_Joint3, "Pinky joint3", HandManager.HandJoint.Pinky_Joint2), // 24
			new BoneMap(HandManager.HandJoint.Pinky_Tip, "Pinky tip", HandManager.HandJoint.Pinky_Joint3), // 25
		};

		//public enum PreferTrackerType
		//{
		//	HandManagerDefined,
		//	Natural,
		//	Electronic
		//}

		private const float minAlpha = 0.2f;

		[Tooltip("Draw left hand if true, right hand otherwise")]
		public bool IsLeft = false;
		[Tooltip("Show electronic hand in controller mode")]
		public bool showElectronicHandInControllerMode = false;
		[Tooltip("Root object of skinned mesh")]
		public GameObject Hand;

		[Tooltip("Use skeleton, mesh and pose from runtime")]
		public bool useRuntimeModel = true;
		[Tooltip("Use scale from runtime")]
		public bool useScale = true;
		[Tooltip("Nodes of skinned mesh, must be size of 26 in same order as skeleton definition")]
		public Transform[] BonePoses = new Transform[BONE_MAX_ID];
		public Transform[] runtimeBonePoses = new Transform[BONE_MAX_ID];
		public Transform[] customizedBonePoses = new Transform[BONE_MAX_ID];
		[Tooltip("Use hand confidence as alpha, low confidence hand becomes transparent")]
		public bool showConfidenceAsAlpha = false;
		public bool alreadyDetect = false;
		[HideInInspector]
		public bool checkInteractionMode = false;

		[Tooltip("Use input device to update bones directly.  Will be faster but no Wave Essence feature support.")]
		public bool useInputDevice = true;

		private Vector3[] s_JointPosition = new Vector3[BONE_MAX_ID];
		private Quaternion[] s_JointRotation = new Quaternion[BONE_MAX_ID];

		private GameObject m_SystemHand = null;
		private GameObject m_SystemHandMesh = null;
		private GameObject[] handTran = new GameObject[BONE_MAX_ID];
		private Mesh mesh = null;
		private SkinnedMeshRenderer skinMeshRend = null;
		private SkinnedMeshRenderer customizedSkinMeshRend = null;
		IntPtr handModel = IntPtr.Zero;
		Quaternion rot;
		Vector3 pos;
		Vector3 scale;

		private JSON_HandModelDesc_Ext handModelDesc = null;
		private bool isHandStable = false;

		private Transform FindChildRecursive(Transform parent, string name)
		{
			foreach (Transform child in parent)
			{
				if (child.name.Contains(name))
					return child;

				var result = FindChildRecursive(child, name);
				if (result != null)
					return result;
			}
			return null;
		}

		public void AutoDetect()
		{
			Log.d(TAG, HSB.Append("AutoDetect()"));
			customizedSkinMeshRend = transform.GetComponentInChildren<SkinnedMeshRenderer>();
			if (customizedSkinMeshRend == null)
			{
				Log.d(TAG, HSB.Append("AutoDetect() Cannot find SkinnedMeshRenderer in ").Append(name));
				return;
			}

			for (int i = 0; i < boneMap.Length; i++)
			{
				string searchName = BNames[i];
				Transform t = FindChildRecursive(transform, searchName);

				if (t == null)
				{
					Log.d(TAG, HSB.Append("AutoDetect() ").Append(boneMap[i].DisplayName).Append(" not found!"));
					continue;
				}

				Log.d(TAG, HSB.Append("AutoDetect() ").Append(boneMap[i].DisplayName).Append(" found: ").Append(searchName));
				customizedBonePoses[i] = t;
			}
			alreadyDetect = true;
			Log.d(TAG, HSB.Append("AutoDetect--"));
		}


		private void ReadJson()
		{
			try
			{
				handModelDesc = new JSON_HandModelDesc_Ext(OEMConfig.getHandModelDesc());
			}
			catch (Exception e)
			{
				handModelDesc = null;
				Log.d(TAG, e.ToString());
			}

			if (handModelDesc == null)
			{
				Log.d(TAG, "Apply fixed OEM data.", true);
				handModelDesc = new JSON_HandModelDesc_Ext();
				handModelDesc.default_style = new JSON_HandStyleDesc_Ext();
				handModelDesc.default_style.gra_color_A = new Color(0.1058824f, 0.6901961f, 0.9019608f, 0);
				handModelDesc.default_style.gra_color_B = new Color(1, 1, 1, 0);
				handModelDesc.default_style.con_gra_color_A = new Color(0.1058824f, 0.6901961f, 0.9019608f, 0);
				handModelDesc.default_style.con_gra_color_B = new Color(1, 1, 1, 0);
				handModelDesc.default_style.thickness = 0.001f;
				handModelDesc.default_style.filling_opacity = 0.45f;
				handModelDesc.default_style.contouring_opacity = 0.5f;
				handModelDesc.fusion_style = new JSON_HandStyleDesc_Ext();
				handModelDesc.fusion_style.gra_color_A = new Color(0.1058824f, 0.6901961f, 0.9019608f, 0);
				handModelDesc.fusion_style.gra_color_B = new Color(1, 1, 1, 0);
				handModelDesc.fusion_style.con_gra_color_A = new Color(1, 1, 1, 0);
				handModelDesc.fusion_style.con_gra_color_B = new Color(0.1058824f, 0.6901961f, 0.9019608f, 0);
				handModelDesc.fusion_style.thickness = 0.002f;
				handModelDesc.fusion_style.filling_opacity = 0.45f;
				handModelDesc.fusion_style.contouring_opacity = 0.5f;
			}

			for (int i = 0; i < 2; i++)
			{
				var style = i == 0 ? handModelDesc.default_style : handModelDesc.fusion_style;
				var sb = Log.CSB.Append(i == 0 ? "default_style { " : "fusion_style { ");
				style.Dump(sb);
				sb.Append(" } ");
				Log.d(TAG, sb, true);
			}
		}

		public bool GetNaturalHandModel()
		{
			Log.d(TAG, HSB.Append("GetNaturalHandModel"));
			WVR_Result r = Interop.WVR_GetCurrentNaturalHandModel(ref handModel);
			Log.d(TAG, HSB.Append("WVR_GetCurrentNaturalHandModel, handModel IntPtr = ").Append(handModel.ToInt32()));
			Log.d(TAG, HSB.Append("sizeof(WVR_HandRenderModel) = ").Append(Marshal.SizeOf(typeof(WVR_HandRenderModel))));
			if (r == WVR_Result.WVR_Success)
			{
				if (handModel != IntPtr.Zero)
				{
					Log.d(TAG, HSB.Append("handModels"));
					WVR_HandRenderModel handModels = (WVR_HandRenderModel)Marshal.PtrToStructure(handModel, typeof(WVR_HandRenderModel));
					Log.d(TAG, HSB.Append("handModels--"));
					if (IsLeft)
						createHandMesh(handModels.left, handModels.handAlphaTex);
					else
						createHandMesh(handModels.right, handModels.handAlphaTex);
				}
			}
			else
			{
				Log.d(TAG, HSB.Append("GetCurrentNaturalHandModel failed: ").Append(r));
				return false;
			}

			skinMeshRend = m_SystemHand.GetComponentInChildren<SkinnedMeshRenderer>();
			if (skinMeshRend == null)
			{
				Log.d(TAG, HSB.Append("Cannot find SkinnedMeshRenderer in ").Append(name));
				return false;
			}
			for (int i = 0; i < boneMap.Length; i++)
			{
				string searchName = BNames[i];
				Transform t = FindChildRecursive(transform, searchName);

				if (t == null)
				{
					Log.d(TAG, HSB.Append("GetNaturalHandModel() ").Append(boneMap[i].DisplayName).Append(" not found!"));
					continue;
				}

				Log.d(TAG, HSB.Append("GetNaturalHandModel() ").Append(boneMap[i].DisplayName).Append(" found: ").Append(searchName));
			}
			Interop.WVR_ReleaseNaturalHandModel(ref handModel);
			Log.d(TAG, HSB.Append("GetNaturalHandModel--"));
			return true;
		}

		static void DebugMatrix(string name, WVR_Matrix4f_t m)
		{
			Debug.Log("QQQ " + name);
			Debug.LogFormat("QQQ / {0:F6} {1:F6} {2:F6} {3:F6} \\", m.m0, m.m1, m.m2, m.m3);
			Debug.LogFormat("QQQ | {0:F6} {1:F6} {2:F6} {3:F6} |", m.m4, m.m5, m.m6, m.m7);
			Debug.LogFormat("QQQ | {0:F6} {1:F6} {2:F6} {3:F6} |", m.m8, m.m9, m.m10, m.m11);
			Debug.LogFormat("QQQ \\ {0:F6} {1:F6} {2:F6} {3:F6} /", m.m12, m.m13, m.m14, m.m15);
		}

		static void DebugMatrix(string name, Matrix4x4 m)
		{
			Debug.Log("QQQ " + name);
			Debug.LogFormat("QQQ / {0:F6} {1:F6} {2:F6} {3:F6} \\", m.m00, m.m01, m.m02, m.m03);
			Debug.LogFormat("QQQ | {0:F6} {1:F6} {2:F6} {3:F6} |", m.m10, m.m11, m.m12, m.m13);
			Debug.LogFormat("QQQ | {0:F6} {1:F6} {2:F6} {3:F6} |", m.m20, m.m21, m.m22, m.m23);
			Debug.LogFormat("QQQ \\ {0:F6} {1:F6} {2:F6} {3:F6} /", m.m30, m.m31, m.m32, m.m33);
		}

		static WVR_Matrix4f_t Transpose(WVR_Matrix4f_t i)
		{
			return new WVR_Matrix4f_t() { 
				m0 = i.m0,
				m4 = i.m1,
				m8 = i.m2,
				m12 = i.m3,

				m1 = i.m4,
				m5 = i.m5,
				m9 = i.m6,
				m13 = i.m7,

				m2 = i.m8,
				m6 = i.m9,
				m10 = i.m10,
				m14 = i.m11,

				m3 = i.m12,
				m7 = i.m13,
				m11 = i.m14,
				m15 = i.m15
			};
		}

		public static Matrix4x4 FromToGL(Matrix4x4 i)
		{
			var m = Matrix4x4.identity;
			int sign = -1;

			m[0, 0] = i[0, 0];
			m[0, 1] = i[0, 1];
			m[0, 2] = i[0, 2] * sign;
			m[0, 3] = i[0, 3];

			m[1, 0] = i[1, 0];
			m[1, 1] = i[1, 1];
			m[1, 2] = i[1, 2] * sign;
			m[1, 3] = i[1, 3];

			m[2, 0] = i[2, 0] * sign;
			m[2, 1] = i[2, 1] * sign;
			m[2, 2] = i[2, 2];
			m[2, 3] = i[2, 3] * sign;

			m[3, 0] = i[3, 0];
			m[3, 1] = i[3, 1];
			m[3, 2] = i[3, 2];
			m[3, 3] = i[3, 3];

			return m;
		}

		private void createHandMesh(WVR_HandModel hand, WVR_CtrlerTexBitmap texBitmap)
		{
			Log.d(TAG, HSB.Append("createHandMesh"));

			ReadJson();

			m_SystemHand = new GameObject("SystemHand" + (IsLeft ? "Left" : "Right"));
			m_SystemHand.transform.SetParent(transform, false);
			m_SystemHandMesh = new GameObject("SystemHandMesh" + (IsLeft ? "Left" : "Right"));
			m_SystemHandMesh.transform.SetParent(m_SystemHand.transform, false);

			Log.d(TAG, HSB.Append("handTran create"));
			for (int i = 0; i < runtimeBonePoses.Length; i++)
			{
				handTran[i] = new GameObject("WaveBone_" + i);
				handTran[i].SetActive(true);
				runtimeBonePoses[i] = handTran[i].transform;
			}
			Log.d(TAG, HSB.Append("handTran create--"));
			for (int i = 0; i < runtimeBonePoses.Length; i++)
			{
				if (hand.jointParentTable[i] == 47)
				{
					handTran[i].transform.parent = m_SystemHand.transform;
				}
				else
				{
					handTran[i].transform.parent = handTran[hand.jointParentTable[i]].transform;
				}
			}

			if (m_SystemHand != null)
				Log.d(TAG, HSB.Append(m_SystemHand.name).Append(" parent: ").Append(m_SystemHand.transform.parent.name));
			if (m_SystemHandMesh != null)
				Log.d(TAG, HSB.Append(m_SystemHandMesh.name).Append(" parent: ").Append(m_SystemHandMesh.transform.parent.name));
			for (int i = 0; i < runtimeBonePoses.Length; i++)
			{
				Log.d(TAG, HSB.Append(handTran[i].name).Append(" parent: ").Append(handTran[i].transform.parent.name));
			}

			/*create basic mesh*/
			mesh = new Mesh();
			Vector3[] _vertices;
			Vector3[] _normals;
			Vector2[] _uv;
			Vector2[] _uv2;
			//vertices
			WVR_VertexBuffer vertices = hand.vertices;
			if (vertices.dimension == 3)
			{
				uint verticesCount = (vertices.size / vertices.dimension);

				Log.d(TAG, HSB.Append(" vertices size = ").Append(vertices.size).Append(", dimension = ").Append(vertices.dimension).Append(", count = ").Append(verticesCount));

				_vertices = new Vector3[verticesCount];
				float[] verticeArray = new float[vertices.size];

				Marshal.Copy(vertices.buffer, verticeArray, 0, verticeArray.Length);

				int verticeIndex = 0;
				int floatIndex = 0;

				while (verticeIndex < verticesCount)
				{
					_vertices[verticeIndex] = new Vector3();
					_vertices[verticeIndex].x = verticeArray[floatIndex++];
					_vertices[verticeIndex].y = verticeArray[floatIndex++];
					_vertices[verticeIndex].z = verticeArray[floatIndex++] * -1.0f;
					verticeIndex++;
				}
				mesh.vertices = _vertices;
			}
			else
			{
				Log.d(TAG, HSB.Append("vertices buffer's dimension incorrect!"));
			}
				// normals
				WVR_VertexBuffer normals = hand.normals;

			if (normals.dimension == 3)
			{
				uint normalsCount = (normals.size / normals.dimension);
				Log.d(TAG, HSB.Append(" normals size = ").Append(normals.size).Append(", dimension = ").Append(normals.dimension).Append(", count = ").Append(normalsCount));
				_normals = new Vector3[normalsCount];
				float[] normalArray = new float[normals.size];

				Marshal.Copy(normals.buffer, normalArray, 0, normalArray.Length);

				int normalsIndex = 0;
				int floatIndex = 0;

				while (normalsIndex < normalsCount)
				{
					_normals[normalsIndex] = new Vector3();
					_normals[normalsIndex].x = normalArray[floatIndex++];
					_normals[normalsIndex].y = normalArray[floatIndex++];
					_normals[normalsIndex].z = normalArray[floatIndex++]* -1.0f;

					normalsIndex++;
				}

				mesh.normals = _normals;
			}
			else
			{
				Log.d(TAG, HSB.Append("normals buffer's dimension incorrect!"));
			}

			// texCoord
			WVR_VertexBuffer texCoord = hand.texCoords;

			if (texCoord.dimension == 2)
			{
				uint uvCount = (texCoord.size / texCoord.dimension);
				Log.d(TAG, HSB.Append(" texCoord size = ").Append(texCoord.size).Append(", dimension = ").Append(texCoord.dimension).Append(", count = ").Append(uvCount));
				_uv = new Vector2[uvCount];
				float[] texCoordArray = new float[texCoord.size];

				Marshal.Copy(texCoord.buffer, texCoordArray, 0, texCoordArray.Length);

				int uvIndex = 0;
				int floatIndex = 0;

				while (uvIndex < uvCount)
				{
					_uv[uvIndex] = new Vector2();
					_uv[uvIndex].x = texCoordArray[floatIndex++];
					_uv[uvIndex].y = 1 - texCoordArray[floatIndex++];

					uvIndex++;
				}
				mesh.uv = _uv;
			}
			else
			{
				Log.d(TAG, HSB.Append("texCoord buffer's dimension incorrect!"));
			}

			// texCoord2s
			WVR_VertexBuffer texCoord2s = hand.texCoord2s;

			if (texCoord2s.dimension == 2)
			{
				uint uvCount = (texCoord2s.size / texCoord2s.dimension);
				Log.d(TAG, HSB.Append(" texCoord2s size = ").Append(texCoord2s.size).Append(", dimension = ").Append(texCoord2s.dimension).Append(", count = ").Append(uvCount));
				_uv2 = new Vector2[uvCount];
				float[] texCoord2sArray = new float[texCoord2s.size];

				Marshal.Copy(texCoord2s.buffer, texCoord2sArray, 0, texCoord2sArray.Length);

				int uv2Index = 0;
				int uv2floatIndex = 0;

				while (uv2Index < uvCount)
				{
					_uv2[uv2Index] = new Vector2();
					_uv2[uv2Index].x = texCoord2sArray[uv2floatIndex++];
					_uv2[uv2Index].y = 1 - texCoord2sArray[uv2floatIndex++];

					uv2Index++;
				}
				mesh.uv2 = _uv2;
			}
			else
			{
				Log.d(TAG, HSB.Append("texCoord2s buffer's dimension incorrect!"));
			}

			// indices
			WVR_IndexBuffer indices = hand.indices;
			Log.d(TAG, HSB.Append(" indices size = ").Append(indices.size));
			int[] indicesArray = new int[indices.size];
			Marshal.Copy(indices.buffer, indicesArray, 0, indicesArray.Length);

			uint indiceIndex = 0;

			while (indiceIndex < indices.size)
			{
				int tmp = indicesArray[indiceIndex];
				indicesArray[indiceIndex] = indicesArray[indiceIndex + 2];
				indicesArray[indiceIndex + 2] = tmp;
				indiceIndex += 3;
			}
			mesh.SetIndices(indicesArray, MeshTopology.Triangles, 0);

			/* assign bone weights to mesh*/
			//boneIDs
			if (hand.boneIDs.dimension == 4 && hand.boneWeights.dimension == 4)
			{
				uint boneIDsCount = (hand.boneIDs.size / hand.boneIDs.dimension);
				Log.d(TAG, HSB.Append(" boneIDs size = ").Append(hand.boneIDs.size).Append(", dimension = ").Append(hand.boneIDs.dimension).Append(", count = ").Append(boneIDsCount));
				int[] boneIDsArray = new int[hand.boneIDs.size];
				Marshal.Copy(hand.boneIDs.buffer, boneIDsArray, 0, boneIDsArray.Length);
				uint boneWeightsCount = (hand.boneWeights.size / hand.boneWeights.dimension);
				Log.d(TAG, HSB.Append(" boneWeights size = ").Append(hand.boneWeights.size).Append(", dimension = ").Append(hand.boneWeights.dimension).Append(", count = ").Append(boneWeightsCount));
				float[] boneWeightsArray = new float[hand.boneWeights.size];
				Marshal.Copy(hand.boneWeights.buffer, boneWeightsArray, 0, boneWeightsArray.Length);

				BoneWeight[] weights = new BoneWeight[boneIDsCount];

				int boneIndex = 0, IDIndex = 0, weightIndex = 0;
				while (boneIndex < boneIDsCount)
				{
					int[] currentBoneIDsArray = new int[4];
					float[] currentBoneWeightsArray = new float[4];

					currentBoneIDsArray[0] = boneIDCorrection(boneIDsArray[IDIndex++]);
					currentBoneIDsArray[1] = boneIDCorrection(boneIDsArray[IDIndex++]);
					currentBoneIDsArray[2] = boneIDCorrection(boneIDsArray[IDIndex++]);
					currentBoneIDsArray[3] = boneIDCorrection(boneIDsArray[IDIndex++]);
					currentBoneWeightsArray[0] = boneWeightsArray[weightIndex++];
					currentBoneWeightsArray[1] = boneWeightsArray[weightIndex++];
					currentBoneWeightsArray[2] = boneWeightsArray[weightIndex++];
					currentBoneWeightsArray[3] = boneWeightsArray[weightIndex++];

					Array.Sort(currentBoneWeightsArray, currentBoneIDsArray); //Sort bone ID by weight

					//Assign by bone weight ID by descending weight order
					weights[boneIndex].boneIndex0 = currentBoneIDsArray[3];
					weights[boneIndex].boneIndex1 = currentBoneIDsArray[2];
					weights[boneIndex].boneIndex2 = currentBoneIDsArray[1];
					weights[boneIndex].boneIndex3 = currentBoneIDsArray[0];
					weights[boneIndex].weight0 = currentBoneWeightsArray[3];
					weights[boneIndex].weight1 = currentBoneWeightsArray[2];
					weights[boneIndex].weight2 = currentBoneWeightsArray[1];
					weights[boneIndex].weight3 = currentBoneWeightsArray[0];

					boneIndex++;
				}
				mesh.boneWeights = weights;
			}
			else
			{
				Log.d(TAG, HSB.Append("boneIDs buffer dimension = ").Append(hand.boneIDs.dimension).Append("or boneWeights buffer dimension = ").Append(hand.boneWeights.dimension).Append("is incorrect!"));
			}

			// model texture section
			var rawImageSize = texBitmap.height * texBitmap.stride;
			byte[] modelTextureData = new byte[rawImageSize];
			Marshal.Copy(texBitmap.bitmap, modelTextureData, 0, modelTextureData.Length);

			Texture2D modelpng = new Texture2D((int)texBitmap.width, (int)texBitmap.height, TextureFormat.RGBA32, false);
			modelpng.LoadRawTextureData(modelTextureData);
			modelpng.Apply();

			for (int q = 0; q < 10240; q += 1024)
			{
				string textureContent = "";

				for (int c = 0; c < 64; c++)
				{
					if ((q * 64 + c) >= modelTextureData.Length)
						break;
					textureContent += modelTextureData.GetValue(q * 64 + c).ToString();
					textureContent += " ";
				}
			}

			/* Create Bone Transforms and Bind poses */
			Matrix4x4[] bindPoses = new Matrix4x4[BONE_MAX_ID];
			for (int i = 0; i < runtimeBonePoses.Length; i++)
			{
				bindPoses[i] = FromToGL(hand.jointInvTransMats[i]);
				var m = FromToGL(hand.jointLocalTransMats[i]);
				var pos = handTran[i].transform.localPosition = m.GetPosition();
				var rot = handTran[i].transform.localRotation = m.rotation;
				handTran[i].transform.localScale = Vector3.one;
			}

			m_SystemHand.SetActive(true);
			m_SystemHandMesh.SetActive(true);
			mesh.bindposes = bindPoses;
			Material ImgMaterial;
			if(IsLeft)
				ImgMaterial = Resources.Load("Materials/HandMatLeft", typeof(Material)) as Material;
			else
				ImgMaterial = Resources.Load("Materials/HandMatRight", typeof(Material)) as Material;
			skinMeshRend = m_SystemHandMesh.AddComponent<SkinnedMeshRenderer>();
			if(skinMeshRend != null)
			{
				skinMeshRend.bones = runtimeBonePoses;
				skinMeshRend.sharedMesh = mesh;
				skinMeshRend.rootBone = handTran[1].transform;
				if (ImgMaterial == null)
				{
					Log.d(TAG, HSB.Append("ImgMaterial is null"));
				}
				skinMeshRend.material = ImgMaterial;
				skinMeshRend.material.mainTexture = modelpng;
				skinMeshRend.enabled = true;
				isHandStable = false;  // default_style
				SetRuntimeModelMaterialStyle(isHandStable);
			}
			else
			{
				Log.d(TAG, HSB.Append("SkinnedMeshRenderer is null"));
			}
			Log.d(TAG, HSB.Append("createHandMesh--"));
		}

		public void ClearDetect()
		{
			for (int i = 0; i < BonePoses.Length; i++)
			{
				BonePoses[i] = null;
			}
			alreadyDetect = false;
		}

		private void SetRuntimeModelMaterialStyle(bool isStable)
		{
			if (handModelDesc == null || handModelDesc.fusion_style == null || handModelDesc.default_style == null)
			{
				Log.w(TAG, "no OEM config");
				return;
			}
			var style = isStable ? handModelDesc.fusion_style : handModelDesc.default_style;
			skinMeshRend.material.SetColor("_GraColorA", style.gra_color_A);
			skinMeshRend.material.SetColor("_GraColorB", style.gra_color_B);
			skinMeshRend.material.SetColor("_ConGraColorA", style.con_gra_color_A);
			skinMeshRend.material.SetColor("_ConGraColorB", style.con_gra_color_B);
			skinMeshRend.material.SetFloat("_OutlineThickness", style.thickness);
			skinMeshRend.material.SetFloat("_Opacity", style.filling_opacity);
			skinMeshRend.material.SetFloat("_line_opacity", style.contouring_opacity);
			// Only show variables which will be updated.
			Log.d(TAG, Log.CSB
				.Append("SetStyle=").Append(isStable ? "fusion " : "default ")
				//.Append("CA=").Append(skinMeshRend.material.GetColor("_GraColorA")).Append(", ")
				//.Append("CB=").Append(skinMeshRend.material.GetColor("_GraColorB")).Append(", ")
				.Append("CCA=").Append(skinMeshRend.material.GetColor("_ConGraColorA")).Append(", ")
				.Append("CCB=").Append(skinMeshRend.material.GetColor("_ConGraColorB")).Append(", ")
				//.Append("Op=").Append(skinMeshRend.material.GetFloat("_Opacity")).Append(", ")
				.Append("LOp=").Append(skinMeshRend.material.GetFloat("_line_opacity")).Append(", ")
				.Append("Th=").Append(skinMeshRend.material.GetFloat("_OutlineThickness")));
		}

		// This is only used for OEM config. Customized hand didn't need this.
		private void CheckMaterial()
		{
			var hm = HandManager.Instance;
			if (hm == null)
				return;
			bool st = hm.IsWristPositionFused();
			if (st == isHandStable)
				return;
			isHandStable = st;
			SetRuntimeModelMaterialStyle(isHandStable);
		}

		void UpdateBonePose()
		{
			if (BonePoses.Length < 2 || BonePoses[1] == null)
				return;

			Vector3 wristScale = Vector3.one;
			if (useRuntimeModel || (!useRuntimeModel && useScale))
			{
				if (GetHandScale(ref scale, IsLeft))
				{
					wristScale = scale;
				}
				else
				{
					if (Log.gpl.Print)
						Log.d(TAG, HSB.Append("Invalid scale"));
				}
			}

			// Get wrist's parent
			var parentTransform = BonePoses[1].parent;
			var parentMatrix = parentTransform.localToWorldMatrix;
			var parentRotation = Matrix4x4.Rotate(parentTransform.rotation);

			// 1. Updates the wrist scale.
			BonePoses[(int)HandManager.HandJoint.Wrist].localScale = wristScale;

			// 2. Updates the wrist local position
			if (useInputDevice)
			{
				if (!GetWaveBones())
				{
					if (Log.gpl.Print)
						Log.d(TAG, "Fail to get bones data");
					return;
				}
			}
			else
			{
				for (int i = 0; i < BonePoses.Length; i++)
				{
					GetJointPosition(boneMap[i].BoneID, ref s_JointPosition[(int)boneMap[i].BoneID], IsLeft);
					GetJointRotation(boneMap[i].BoneID, ref s_JointRotation[(int)boneMap[i].BoneID], IsLeft);
				}
			}

			BonePoses[(int)HandManager.HandJoint.Wrist].localPosition = s_JointPosition[(int)HandManager.HandJoint.Wrist];
			BonePoses[(int)HandManager.HandJoint.Wrist].localRotation = s_JointRotation[(int)HandManager.HandJoint.Wrist];

			// 3. Updates the bone local position (exclude wrist) and rotation.
			for (int i = 0; i < BonePoses.Length; i++)
			{
				if (boneMap[i].BoneID != HandManager.HandJoint.Wrist && boneMap[i].BoneID != boneMap[i].BoneParentID)
				{
					var mParentLocalToWorld = parentMatrix.inverse * BonePoses[i].parent.localToWorldMatrix;
					Vector3 diffDirection = s_JointPosition[(int)boneMap[i].BoneID] - s_JointPosition[(int)boneMap[i].BoneParentID];
					BonePoses[i].localPosition = mParentLocalToWorld.inverse * diffDirection;

					if (s_JointRotation[(int)boneMap[i].BoneID].IsValid())
					{
						var m = Matrix4x4.Rotate(s_JointRotation[(int)boneMap[i].BoneID]);
						BonePoses[i].rotation = (parentRotation * m).rotation;
					}
				}
			}

			/*
			// Only apply pos to wrist's parent, which could be the FBX object.
			if (GetJointPosition(HandManager.HandJoint.Wrist, ref pos, IsLeft))
				BonePoses[1].localPosition = pos;

			// The pos can't put in child. because the scale will effect them.
			BonePoses[1].localScale = wristScale;

			for (int i = 0; i < BonePoses.Length; i++) //  0 is palm, 1 is wrist
			{
				if (BonePoses[i])
				{
					if (GetJointRotation(boneMap[i].BoneID, ref rot, IsLeft))
					{
						var m = Matrix4x4.Rotate(rot);
						BonePoses[i].rotation = (parentRotation * m).rotation;
					}
					else
					{
						// use translate to simulate rotation
						//Log.gpl.d(TAG, BonePoses[i].transform.name + " no rotation");
					}
				}
			}
			*/
		}

		static List<Bone>[] fingersXR = new List<Bone>[5]
		{
			new List<Bone>(),
			new List<Bone>(),
			new List<Bone>(),
			new List<Bone>(),
			new List<Bone>()
		};
		Bone palmXR;
		Bone WristXR;

		private bool GetXRBones()
		{
			if (!foundHandDev || !handDev.isValid) return false;
			if (handDev.TryGetFeatureValue(CommonUsages.handData, out UnityEngine.XR.Hand hand))
			{
				bool ret = true;
				ret &= hand.TryGetRootBone(out palmXR);
				ret &= palmXR.TryGetParentBone(out WristXR);

				for (int i = 0; i < 5; i++)
					ret &= hand.TryGetFingerBones((HandFinger)i, fingersXR[i]);
				return ret;
			}
			return false;
		}

		private bool GetWaveBones()
		{
			if (!GetXRBones()) return false;
			bool ret = true;
			ret &= palmXR.TryGetPosition(out s_JointPosition[0]);
			ret &= palmXR.TryGetRotation(out s_JointRotation[0]);
			ret &= WristXR.TryGetPosition(out s_JointPosition[1]);
			ret &= WristXR.TryGetRotation(out s_JointRotation[1]);
			int shiftId = 2;
			for (int i = 0; i < 4; i++)
			{
				ret &= fingersXR[(int)HandFinger.Thumb][i].TryGetPosition(out s_JointPosition[i + shiftId]);
				ret &= fingersXR[(int)HandFinger.Thumb][i].TryGetRotation(out s_JointRotation[i + shiftId]);
			}
			shiftId += 4;
			for (int i = 0; i < 5; i++)
			{
				ret &= fingersXR[(int)HandFinger.Index][i].TryGetPosition(out s_JointPosition[i + shiftId]);
				ret &= fingersXR[(int)HandFinger.Index][i].TryGetRotation(out s_JointRotation[i + shiftId]);
			}
			shiftId += 5;
			for (int i = 0; i < 5; i++)
			{
				ret &= fingersXR[(int)HandFinger.Middle][i].TryGetPosition(out s_JointPosition[i + shiftId]);
				ret &= fingersXR[(int)HandFinger.Middle][i].TryGetRotation(out s_JointRotation[i + shiftId]);
			}
			shiftId += 5;
			for (int i = 0; i < 5; i++)
			{
				ret &= fingersXR[(int)HandFinger.Ring][i].TryGetPosition(out s_JointPosition[i + shiftId]);
				ret &= fingersXR[(int)HandFinger.Ring][i].TryGetRotation(out s_JointRotation[i + shiftId]);
			}
			shiftId += 5;
			for (int i = 0; i < 5; i++)
			{
				ret &= fingersXR[(int)HandFinger.Pinky][i].TryGetPosition(out s_JointPosition[i + shiftId]);
				ret &= fingersXR[(int)HandFinger.Pinky][i].TryGetRotation(out s_JointRotation[i + shiftId]);
			}
			return true;
		}


		private bool GetJointPosition(HandManager.HandJoint joint, ref Vector3 position, bool isLeft)
		{
			if (HandManager.Instance == null) { return false; }
			return HandManager.Instance.GetJointPosition(joint, ref position, isLeft);
		}

		private bool GetJointRotation(HandManager.HandJoint joint, ref Quaternion rotation, bool isLeft)
		{
			if (HandManager.Instance == null) { return false; }
			return HandManager.Instance.GetJointRotation(joint, ref rotation, isLeft);
		}

		private bool GetHandScale(ref Vector3 scale, bool isLeft)
		{
			if (HandManager.Instance == null) { return false; }
			return HandManager.Instance.GetHandScale(ref scale, isLeft);
		}

		private float GetHandConfidence(bool isLeft)
		{
			if (HandManager.Instance == null) { return 0; }
			return HandManager.Instance.GetHandConfidence(isLeft);
		}

		private bool IsPoseValid()
		{
			bool isPoseValid = false;
			if (hasIMManager)
			{
				currInteractionMode = ClientInterface.InteractionMode;

				if (currInteractionMode != preInteractionMode)
				{
					Log.d(TAG, HSB.Append("Interaction mode changed to ").Append(currInteractionMode));
					preInteractionMode = currInteractionMode;

					if (currInteractionMode == XR_InteractionMode.Controller)
					{
						// show electronic hand?
						bool isSupported = Interop.WVR_ControllerSupportElectronicHand();

						showECHand = (isSupported && showElectronicHandInControllerMode);

						Log.d(TAG, HSB.Append("Device support electronic hand? ").Append(isSupported).Append(", show electronic hand in controller mode? ").Append(showElectronicHandInControllerMode));
					}
				}

				if (ClientInterface.InteractionMode == XR_InteractionMode.Hand)
				{
					if (useInputDevice)
						handDev.TryGetFeatureValue(CommonUsages.isTracked, out isPoseValid);
					else
						isPoseValid = (HandManager.Instance != null) &&
							(HandManager.Instance.IsHandPoseValid(HandManager.TrackerType.Natural, IsLeft));
				}

				if (ClientInterface.InteractionMode == XR_InteractionMode.Controller)
					isPoseValid = showECHand && (HandManager.Instance != null) &&
						(HandManager.Instance.IsHandPoseValid(HandManager.TrackerType.Electronic, IsLeft));
			}
			else
			{
				if (useInputDevice)
					handDev.TryGetFeatureValue(CommonUsages.isTracked, out isPoseValid);
				else
					isPoseValid = (HandManager.Instance != null) &&
						(HandManager.Instance.IsHandPoseValid(HandManager.TrackerType.Natural, IsLeft));
			}
			return isPoseValid;
		}

		XR_InteractionMode preInteractionMode = XR_InteractionMode.Default;
		XR_InteractionMode currInteractionMode;

		bool showECHand = false;
		void Update()
		{
			if (useRuntimeModel)
			{
				CheckLoadModel();
				CheckMaterial();
			}

			if (Hand == null)
				return;

			bool isFocused = ClientInterface.IsFocused;
			bool isPoseValid = false;
			if (isFocused)
				isPoseValid = IsPoseValid();

			bool showHand = isPoseValid && isFocused;

			if (Log.gpl.Print)
				Log.d(TAG, HSB
					.Append("Pose isValid: ").Append(isPoseValid)
					.Append(", isFocused").Append(isFocused)
					.Append(", showHand: ").Append(showHand)
					.Append(", Interaction Mode: ").Append(hasIMManager));

			if (Hand.activeInHierarchy != showHand)
				Hand.SetActive(showHand);
			if (!showHand)
				return;

			Profiler.BeginSample("UpdateBonePose");
			UpdateBonePose();
			Profiler.EndSample();

			if (showConfidenceAsAlpha)
			{
				float conValue = GetHandConfidence(IsLeft);

				if (Log.gpl.Print)
					Log.d(TAG, HSB.Append("Confidence value: ").Append(conValue));

				var color = Hand.GetComponent<Renderer>().material.color;
				color.a = conValue > minAlpha ? conValue : minAlpha;
				Hand.GetComponent<Renderer>().material.color = color;
			}
		}

		private void CheckLoadModel()
		{
			if (!useRuntimeModel || skinMeshRend != null)
			{
				// If already loaded, skinMeshRend will not be null.
				return;
			}

			if (!Interop.WVR_IsDeviceConnected(
					IsLeft ?
					WVR_DeviceType.WVR_DeviceType_NaturalHand_Left :
					WVR_DeviceType.WVR_DeviceType_NaturalHand_Right)
				)
			{
				return;
			}

			useRuntimeModel = GetNaturalHandModel();
			Log.d(TAG, HSB.Append("CheckLoadModel() useRuntimeModel: ").Append(useRuntimeModel));
			if (useRuntimeModel)
			{
				if (customizedSkinMeshRend != null)
				{
					customizedSkinMeshRend.gameObject.SetActive(false);
					Log.d(TAG, HSB.Append("CheckLoadModel() disable ").Append(customizedSkinMeshRend.gameObject.name));
				}
				if (skinMeshRend != null)
				{
					skinMeshRend.gameObject.SetActive(true);
					Log.d(TAG, HSB.Append("CheckLoadModel() enable ").Append(skinMeshRend.gameObject.name));
					Hand = skinMeshRend.gameObject;
					Log.d(TAG, HSB.Append("CheckLoadModel() Set Hand to ").Append(Hand.name));
				}

				BonePoses = runtimeBonePoses;
				for (int i = 0; i < boneMap.Length; i++)
				{
					Log.d(TAG, HSB.Append("CheckLoadModel() ").Append(boneMap[i].DisplayName).Append(" --> ").Append(BonePoses[i].name));
				}
			}
		}

		void OnOEMConfigChanged()
		{
			Log.d(TAG, HSB.Append("OnOEMConfigChanged()"));
			ReadJson();
			SetRuntimeModelMaterialStyle(isHandStable);
		}

		void OnEnable()
		{
			if (BonePoses.Length != boneMap.Length)
			{
				Log.d(TAG, HSB.Append("OnEnable() Length of BonePoses is not equal to length of boneMap, skip!"));
				return;
			}

			skinMeshRend = null;
			customizedSkinMeshRend = null;

			ClearDetect();
			AutoDetect();

			BonePoses = customizedBonePoses;
			if (customizedSkinMeshRend != null)
			{
				customizedSkinMeshRend.gameObject.SetActive(true);
				Hand = customizedSkinMeshRend.gameObject;
				Log.d(TAG, HSB.Append("OnEnable() Set Hand to ").Append(Hand.name));
			}

			for (int i = 0; i < boneMap.Length; i++)
			{
				Log.d(TAG, HSB.Append("OnEnable() ").Append(boneMap[i].DisplayName).Append(" --> ").Append(BonePoses[i].name));
			}

			GeneralEvent.Listen(GeneralEvent.INTERACTION_MODE_MANAGER_READY, OnInteractionModeManagerReady);
			OEMConfig.onOEMConfigChanged += OnOEMConfigChanged;

			if (Hand != null) { Hand.SetActive(false); } // hidden in AP start.
		}

		void OnDisable()
		{
			OEMConfig.onOEMConfigChanged -= OnOEMConfigChanged;
			GeneralEvent.Remove(GeneralEvent.INTERACTION_MODE_MANAGER_READY, OnInteractionModeManagerReady);
		}

		// Start is called before the first frame update
		void Start()
		{
			StartCoroutine(CheckInputDevice());
		}

		IEnumerator CheckInputDevice()
		{
			while (Utils.InputSubsystem == null) yield return null;
			List<InputDevice> devices = new List<InputDevice>();

			var wfs = new WaitForSeconds(0.5f);
			Log.d(TAG, HSB.Append("Looking for input device..."));
			while (true)
			{
				if (!foundHandDev)
				{
					InputDevices.GetDevices(devices);
					for (int i = 0; i < devices.Count; i++)
					{
						if (InputDeviceHand.IsHandDevice(devices[i], IsLeft))
						{
							foundHandDev = true;
							handDev = devices[i];
							Log.d(TAG, HSB.Append("Found input device"));
							break;
						}
					}
					yield return wfs;
					if (Log.gpl.Print)
						Log.d(TAG, HSB.Append("Still looking for input device..."));
				}
				else if (!handDev.isValid)
				{
					Log.d(TAG, HSB.Append("handDev.is not valid"));
					yield return wfs;
				}
				else
				{
					// Found devices
					yield return wfs;
				}
			}
		}

		private bool hasIMManager = false;
		private void OnInteractionModeManagerReady(params object[] args)
		{
			hasIMManager = true;
		}

		private int boneIDCorrection(int ID)
		{
			if (ID == 47)
			{
				return 0;
			}

			return ID;
		}
	}
}
