using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
	public class ScenePerceptionDemo : MonoBehaviour
	{
		private const string TAG = "ScenePerceptionDemo";

		[SerializeField] private LogPanel logPanel;
		[SerializeField] private ScenePerceptionHelper _scenePerceptionHelper;
		[SerializeField] private PassThroughHelper passThroughHelper;
		[SerializeField] private GameObject anchorPrefab, anchorDisplayPrefab;

		[SerializeField] private SpatialAnchorHelper _spatialAnchorHelper;
		private ScenePerceptionMeshFacade _scenePerceptionMeshFacade;
		private bool hideMeshAndAnchors = false;

		private RaycastHit leftControllerRaycastHitInfo = new RaycastHit();
		private RaycastHit rightControllerRaycastHitInfo = new RaycastHit();
		private GameObject AnchorDisplayRight = null;
		private AnchorGenerateMode anchorGenerateMode = AnchorGenerateMode.Raycast;

		[SerializeField] private Material GeneratedMeshMaterialTranslucent, GeneratedMeshMaterialWireframe;
		[SerializeField] private GameObject leftController = null, rightController = null;

		[SerializeField] private Text modeText;
		[SerializeField] private Text statusText;

		[SerializeField] private SwipeFunction verticalSwipe = SwipeFunction.AnchorGeneration;
		[SerializeField] private SwipeFunction horizontalSwipe = SwipeFunction.AnchorMode;

		private enum AnchorGenerateMode
		{
			Raycast,
			Touch,
		}

		public enum SwipeFunction
		{
			SceneTarget,
			AnchorMode,
			AnchorGeneration,
		}

		bool isSwipeLeft;
		bool isSwipeRight;
		bool isSwipeUp;
		bool isSwipeDown;

		private void OnEnable()
		{
			_scenePerceptionHelper.Init(this, logPanel);
			_spatialAnchorHelper.Init(this, logPanel, _scenePerceptionHelper.scenePerceptionManager, anchorPrefab);

			_scenePerceptionHelper.OnEnable();
			_spatialAnchorHelper.OnEnable();

			if (_scenePerceptionHelper.IsSceneStarted)
			{
				_spatialAnchorHelper.SetAnchorsShouldBeUpdated();
			}
			_scenePerceptionMeshFacade = new ScenePerceptionMeshFacade(_scenePerceptionHelper, anchorDisplayPrefab, GeneratedMeshMaterialTranslucent, GeneratedMeshMaterialWireframe);
		}

		private void OnDisable()
		{
			_scenePerceptionHelper.OnDisable();
			_spatialAnchorHelper.OnDisable();
		}

		private void OnApplicationPause(bool pause)
		{
			if (!pause)
			{
				_spatialAnchorHelper.SetAnchorsShouldBeUpdated(); //Anchors will have moved since the program was previously running - re-update during On Resume in case of a tracking map change
			}
		}

		private void Start()
		{
			UpdateSceneAnchorModeText();
			if (logPanel)
			{
				logPanel.Clear();
				logPanel.AddLog("ScenePerceptionDemo started.");
			}
		}

		private void UpdateSceneAnchorModeText()
		{
			if (modeText == null) return;
			string sceneTarget;
			switch (_scenePerceptionHelper.target)
			{
				case ScenePerceptionHelper.SceneTarget.TwoDimensionPlane:
					sceneTarget = "Scene Plane";
					break;
				case ScenePerceptionHelper.SceneTarget.SceneMesh:
					sceneTarget = "Scene Mesh";
					break;
				default:
					sceneTarget = "Unknown";
					break;
			}
			string anchorMode;
			switch (_spatialAnchorHelper.anchorMode)
			{
				case SpatialAnchorHelper.AnchorMode.Spatial:
					anchorMode = "Spatial Anchor";
					break;
				case SpatialAnchorHelper.AnchorMode.Persisted:
					anchorMode = "Persisted Anchor";
					anchorGenerateMode = AnchorGenerateMode.Touch;  // Force use touch mode
					break;
				case SpatialAnchorHelper.AnchorMode.Cached:
					anchorMode = "Cached Anchor";
					break;
				default:
					anchorMode = "Unknown";
					break;
			}
			modeText.text = sceneTarget + " + " + anchorMode;
		}

		float menuDoubleClickTime = 0;
		float timeAccForAnchorUpdate = 0;

		private void Update()
		{
			// All operation need wait Scene started.
			if (!_scenePerceptionHelper.IsSceneStarted)
				return;

			if (ButtonFacade.BButtonPressed)
			{
				statusText.text = "Togggle objects visiblility";
				hideMeshAndAnchors = !hideMeshAndAnchors;
				Log.d(TAG, Log.CSB.Append("hideMeshAndAnchors: ").Append(hideMeshAndAnchors).Append(hideMeshAndAnchors ? ", All ScenePerception update is stopped." : ""));
				if (hideMeshAndAnchors)
				{
					_scenePerceptionMeshFacade.DestroyGeneratedMeshes();
					_spatialAnchorHelper.ClearAnchorObjects();
					modeText.text = "All Scene Perception paused";
				}
				else
				{
					UpdateSceneAnchorModeText();
				}
			}

			if (ButtonFacade.YButtonPressed)
			{
				statusText.text = "Togggle Passthrough Underlay";
				passThroughHelper.ShowPassthroughUnderlay(!Interop.WVR_IsPassthroughOverlayVisible());
			}

			if (hideMeshAndAnchors)
				return;

			var target = _scenePerceptionHelper.target;
			//Handle Scene Perception
			if (!_scenePerceptionHelper.IsStarted(target))
			{
				_scenePerceptionHelper.StartScenePerception(target);
			}
			else
			{
				_scenePerceptionHelper.ScenePerceptionGetState(target); //Update state of scene perception every frame
				_scenePerceptionMeshFacade.UpdateScenePerceptionMesh();
			}

			// Update Spatial Anchor's pose / state every 0.35 second.
			timeAccForAnchorUpdate += Time.deltaTime;
			if (timeAccForAnchorUpdate > 0.35f)
			{
				timeAccForAnchorUpdate = 0;
				_spatialAnchorHelper.UpdateAnchorDictionary();
			}

			if (ButtonFacade.XButtonPressed)
			{
				statusText.text = "Destroy hitted anchor object";
				_spatialAnchorHelper.HandleAnchorUpdateDestroy(leftControllerRaycastHitInfo);
			}
			if (ButtonFacade.AButtonPressed)
			{
				if (anchorGenerateMode == AnchorGenerateMode.Raycast)
				{
					statusText.text = "Create Anchor at hitted place";
					_spatialAnchorHelper.HandleAnchorUpdateCreate(rightControllerRaycastHitInfo, rightController.transform.rotation);
				}
				else
				{
					statusText.text = "Create Anchor at controller position";
					_spatialAnchorHelper.HandleAnchorUpdateCreate(rightController.transform.position, rightController.transform.rotation);
				}
			}

			if (ButtonFacade.MenuButtonPressed)
			{
				if (Time.unscaledTime - menuDoubleClickTime >= 1.5f)
				{
					Log.d(TAG, "Destroy all anchors");
					statusText.text = "Destroy all anchors";
					_spatialAnchorHelper.ClearAnchors();
				}
				else
				{
					Log.d(TAG, "Destroy all kind of anchors");
					statusText.text = "Destroy all kind of anchors";
					_spatialAnchorHelper.ClearAnchors(true);
				}
				menuDoubleClickTime = Time.unscaledTime;
			}

			if (ButtonFacade.RThumbstickButtonPressed)
			{
				Log.d(TAG, "Export all persist anchor");
				statusText.text = "Export all persist anchor";
				_spatialAnchorHelper.ExportPersistAnchors();
			}

			if (ButtonFacade.LThumbstickButtonPressed)
			{
				Log.d(TAG, "Import all persist anchor");
				statusText.text = "Import all persist anchor";
				_spatialAnchorHelper.ImportPersistAnchors();
			}

			SwipeActionDetect();
		}

		private bool SwipeActionJudge(float axisL, float axisR, bool isAct, bool dir, float threshold)
		{
			if (!isAct)
			{
				if (dir)
				{
					if (axisL > threshold && axisR > threshold)
						return true;
				}
				else
				{
					if (axisL < threshold && axisR < threshold)
						return true;
				}
			}
			else
			{
				if (dir)
				{
					if (axisL <= threshold || axisR <= threshold)
						return false;
				}
				else
				{
					if (axisL >= threshold || axisR >= threshold)
						return false;
				}
			}
			return isAct;  // no update
		}


		private void SwipeActionDetect()
		{
			{
				float axisL = ButtonFacade.LThumbstickAxis.y;
				float axisR = ButtonFacade.RThumbstickAxis.y;
				bool isUpTemp = isSwipeUp;
				bool isDownTemp = isSwipeDown;
				isSwipeUp = SwipeActionJudge(axisL, axisR, isSwipeUp, true, 0.5f);
				isSwipeDown = SwipeActionJudge(axisL, axisR, isSwipeDown, false, -0.5f);
				bool upUpdated = isSwipeUp != isUpTemp;
				bool downUpdated = isSwipeDown != isDownTemp;

				if (upUpdated && isSwipeUp)
				{
					if (verticalSwipe == SwipeFunction.SceneTarget)
					{
						Log.d(TAG, "Scene Mode ++");
						statusText.text = "Scene Mode ++";
						_scenePerceptionHelper.target = (ScenePerceptionHelper.SceneTarget)(_scenePerceptionHelper.target == 0 ? 2 : 0);
						UpdateSceneAnchorModeText();
					}
					else if (verticalSwipe == SwipeFunction.AnchorGeneration)
					{
						if (_spatialAnchorHelper.anchorMode == SpatialAnchorHelper.AnchorMode.Persisted)
							anchorGenerateMode = AnchorGenerateMode.Touch;
						else
							anchorGenerateMode = (AnchorGenerateMode)(((int)anchorGenerateMode + 1) % 2);
						Log.d(TAG, "Generate Anchor By " + anchorGenerateMode);
						statusText.text = "Anchor By " + anchorGenerateMode;
					}
				}
				if (downUpdated && isSwipeDown)
				{
					if (verticalSwipe == SwipeFunction.SceneTarget)
					{
						Log.d(TAG, "Scene Mode --");
						statusText.text = "Scene Mode --";
						_scenePerceptionHelper.target = (ScenePerceptionHelper.SceneTarget)(_scenePerceptionHelper.target == 0 ? 2 : 0);
						UpdateSceneAnchorModeText();
					}
					else if (verticalSwipe == SwipeFunction.AnchorGeneration)
					{
						if (_spatialAnchorHelper.anchorMode == SpatialAnchorHelper.AnchorMode.Persisted)
							anchorGenerateMode = AnchorGenerateMode.Touch;
						else
							anchorGenerateMode = (AnchorGenerateMode)(((int)anchorGenerateMode + 1) % 2);
						Log.d(TAG, "Generate Anchor By " + anchorGenerateMode);
						statusText.text = "Anchor By " + anchorGenerateMode;
					}
				}
			}

			{
				float axisL = ButtonFacade.LThumbstickAxis.x;
				float axisR = ButtonFacade.RThumbstickAxis.x;
				bool isLeftTemp = isSwipeLeft;
				bool isRightTemp = isSwipeRight;
				isSwipeLeft = SwipeActionJudge(axisL, axisR, isSwipeLeft, false, -0.5f);
				isSwipeRight = SwipeActionJudge(axisL, axisR, isSwipeRight, true, 0.5f);
				bool leftUpdated = isSwipeLeft != isLeftTemp;
				bool rightUpdated = isSwipeRight != isRightTemp; ;

				if (leftUpdated && isSwipeLeft)
				{
					if (horizontalSwipe == SwipeFunction.AnchorMode)
					{
						Log.d(TAG, "Anchor Mode --");
						statusText.text = "Anchor Mode --";
						_spatialAnchorHelper.anchorMode = (SpatialAnchorHelper.AnchorMode)(((int)_spatialAnchorHelper.anchorMode + 2) % 3);
						_spatialAnchorHelper.SetAnchorsShouldBeUpdated();
						UpdateSceneAnchorModeText();
					}
				}
				if (rightUpdated && isSwipeRight)
				{
					if (horizontalSwipe == SwipeFunction.AnchorMode)
					{
						Log.d(TAG, "Anchor Mode ++");
						statusText.text = "Anchor Mode ++";
						_spatialAnchorHelper.anchorMode = (SpatialAnchorHelper.AnchorMode)(((int)_spatialAnchorHelper.anchorMode + 1) % 3);
						_spatialAnchorHelper.SetAnchorsShouldBeUpdated();
						UpdateSceneAnchorModeText();
					}
				}
			}
		}

		private void FixedUpdate()
		{
			if (AnchorDisplayRight == null)
			{
				AnchorDisplayRight = Instantiate(anchorDisplayPrefab);
			}

			// Only right hand have touch mode.
			Physics.Raycast(leftController.transform.position, leftController.transform.forward, out leftControllerRaycastHitInfo);

			if (anchorGenerateMode == AnchorGenerateMode.Raycast)
			{
				Physics.Raycast(rightController.transform.position, rightController.transform.forward, out rightControllerRaycastHitInfo);
				if (rightControllerRaycastHitInfo.collider != null &&
					rightControllerRaycastHitInfo.collider.transform.GetComponent<AnchorPrefab>() == null) //Not hitting an anchor
				{
					AnchorDisplayRight.SetActive(true);
					AnchorDisplayRight.transform.SetPositionAndRotation(rightControllerRaycastHitInfo.point, rightController.transform.rotation);
				}
				else
				{
					AnchorDisplayRight.SetActive(false);
				}
			}
			else if (anchorGenerateMode == AnchorGenerateMode.Touch)
			{
				AnchorDisplayRight.SetActive(true);
				AnchorDisplayRight.transform.SetPositionAndRotation(rightController.transform.position, rightController.transform.rotation);
			}
		}

		public void ChangeSceneMeshTypeToVisual()
		{
			_scenePerceptionMeshFacade.ChangeSceneMeshType(WVR_SceneMeshType.WVR_SceneMeshType_VisualMesh);
		}

		public void ChangeSceneMeshTypeToCollider()
		{
			_scenePerceptionMeshFacade.ChangeSceneMeshType(WVR_SceneMeshType.WVR_SceneMeshType_ColliderMesh);
		}

		private static class ButtonFacade
		{
			public static bool AButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Right,WVR_InputId.WVR_InputId_Alias1_A);
			public static bool BButtonPressed => 
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Right,WVR_InputId.WVR_InputId_Alias1_B);
			public static bool RThumbstickButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Right, WVR_InputId.WVR_InputId_Alias1_Thumbstick);
			public static bool XButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left,WVR_InputId.WVR_InputId_Alias1_X);
			public static bool YButtonPressed => 
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left,WVR_InputId.WVR_InputId_Alias1_Y);
			public static bool LThumbstickButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_Thumbstick);
			public static bool MenuButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_Menu);
			public static Vector2 LThumbstickAxis =>
				WXRDevice.ButtonAxis(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_Thumbstick);
			public static Vector2 RThumbstickAxis =>
				WXRDevice.ButtonAxis(WVR_DeviceType.WVR_DeviceType_Controller_Right, WVR_InputId.WVR_InputId_Alias1_Thumbstick);
		}
	}
}
