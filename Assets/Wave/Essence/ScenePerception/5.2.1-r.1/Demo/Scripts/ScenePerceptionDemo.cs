using UnityEngine;
using Wave.Essence;
using Wave.Essence.ScenePerception.Sample;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
    public class ScenePerceptionDemo : MonoBehaviour
    {
		[SerializeField] private ScenePerceptionHelper _scenePerceptionHelper;
		[SerializeField] private PassThroughHelper passThroughHelper;
	    [SerializeField] private GameObject anchorPrefab, anchorDisplayPrefab;
		
        private SpatialAnchorHelper _spacialAnchorHelper;
        private ScenePerceptionMeshFacade _scenePerceptionMeshFacade;
        private bool hideMeshAndAnchors = false;
        
        private RaycastHit leftControllerRaycastHitInfo = new RaycastHit(), rightControllerRaycastHitInfo = new RaycastHit();
        private GameObject AnchorDisplayRight = null; 
        
        [SerializeField] private Material GeneratedMeshMaterialTranslucent, GeneratedMeshMaterialWireframe;
        [SerializeField] private GameObject leftController = null, rightController = null;

		private const string LOG_TAG = "ScenePerceptionDemo";

		private void OnEnable()
        {
			if (_scenePerceptionHelper == null)
			{
				_scenePerceptionHelper = new ScenePerceptionHelper();
			}

			if (_scenePerceptionHelper != null)
			{
				_scenePerceptionHelper.OnEnable();
			}

			_spacialAnchorHelper = new SpatialAnchorHelper(_scenePerceptionHelper.scenePerceptionManager, anchorPrefab);
            if (_scenePerceptionHelper.isSceneComponentRunning)
            {
	            _spacialAnchorHelper.SetAnchorsShouldBeUpdated();
            }
	        _scenePerceptionMeshFacade = new ScenePerceptionMeshFacade(_scenePerceptionHelper, anchorDisplayPrefab, GeneratedMeshMaterialTranslucent, GeneratedMeshMaterialWireframe);
            
        }
        private void OnDisable()
        {
            if (_scenePerceptionHelper != null)
            {
                _scenePerceptionHelper.OnDisable();
            }
        }
        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                _spacialAnchorHelper.SetAnchorsShouldBeUpdated(); //Anchors will have moved since the program was previously running - re-update during On Resume in case of a tracking map change
            }
        }

        private void Update()
        {
			if (_scenePerceptionHelper.isSceneComponentRunning && !hideMeshAndAnchors)
	        {
				//Handle Scene Perception
				if (!_scenePerceptionHelper.isScenePerceptionStarted)
				{
					_scenePerceptionHelper.StartScenePerception();
				}
				else
				{
					_scenePerceptionHelper.ScenePerceptionGetState(); //Update state of scene perception every frame
					_scenePerceptionMeshFacade.UpdateScenePerceptionMesh();
				}

		        //Handle Spatial Anchor
		        _spacialAnchorHelper.UpdateAnchorDictionary();
	        }

	        if (_scenePerceptionHelper.isSceneComponentRunning)
	        {
		        if (ButtonFacade.XButtonPressed)
		        {
			        _spacialAnchorHelper.HandleAnchorUpdateDestroy(leftControllerRaycastHitInfo);
		        }
		        if (ButtonFacade.AButtonPressed)
		        {
			        _spacialAnchorHelper.HandleAnchorUpdateCreate(rightControllerRaycastHitInfo, rightController.transform.rotation);
		        }
	        }
	        if (ButtonFacade.YButtonPressed)
	        {
		        passThroughHelper.ShowPassthroughUnderlay(!Interop.WVR_IsPassthroughOverlayVisible());
	        }
	        if (ButtonFacade.BButtonPressed)
	        {
		        hideMeshAndAnchors = !hideMeshAndAnchors;
				Log.d(LOG_TAG, "hideMeshAndAnchors: " + hideMeshAndAnchors);
		        if (hideMeshAndAnchors)
		        {
			        _scenePerceptionMeshFacade.DestroyGeneratedMeshes();
			        _spacialAnchorHelper.ClearAnchors();
		        }
	        }

        }
        private void FixedUpdate()
        {
	        if (AnchorDisplayRight == null)
	        {
		        AnchorDisplayRight = UnityEngine.GameObject.Instantiate(anchorDisplayPrefab);
	        }

	        Physics.Raycast(leftController.transform.position, leftController.transform.forward, out leftControllerRaycastHitInfo);

	        Physics.Raycast(rightController.transform.position, rightController.transform.forward, out rightControllerRaycastHitInfo);
	        if (rightControllerRaycastHitInfo.collider != null && rightControllerRaycastHitInfo.collider.transform.GetComponent<AnchorPrefab>() == null) //Not hitting an anchor
	        {
		        AnchorDisplayRight.gameObject.SetActive(true);
		        AnchorDisplayRight.transform.SetPositionAndRotation(rightControllerRaycastHitInfo.point,rightController.transform.rotation);
	        }
	        else
	        {
		        AnchorDisplayRight.gameObject.SetActive(false);
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
	        public static bool XButtonPressed =>
		        WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left,WVR_InputId.WVR_InputId_Alias1_X);
	        public static bool YButtonPressed => 
		        WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left,WVR_InputId.WVR_InputId_Alias1_Y);
        }
	}
}
