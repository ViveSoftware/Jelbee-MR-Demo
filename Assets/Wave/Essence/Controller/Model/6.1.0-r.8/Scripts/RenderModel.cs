// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;
using System.Runtime.InteropServices;
using UnityEngine.XR;
using Wave.Essence.Extra;
using UnityEditor;
using UnityEngine.Profiling;
using System.Text;

namespace Wave.Essence.Controller.Model
{
	public class RenderModel : MonoBehaviour
	{
		#region Log
		private static string LOG_TAG = "Wave.Essence.Controller.Model.RenderModel";
		private StringBuilder sbi = new StringBuilder();
		private StringBuilder RMCSB { get { return sbi.Clear(); } }

		private void PrintDebugLog(StringBuilder msg) {
			Log.d(LOG_TAG, RMCSB.Append("Hand: ").Append(WhichHand == XR_Hand.Dominant ? "R, " : "L, ").Append(msg), true);
		}
		private void PrintDebugLog(string msg)
		{
			Log.d(LOG_TAG, RMCSB.Append("Hand: ").Append(WhichHand == XR_Hand.Dominant ? "R, " : "L, ").Append(msg), true);
		}
		bool printIntervalLog = false;
		int logFrame = 0;

		private void PrintInfoLog(string msg)
		{
			Log.i(LOG_TAG, RMCSB.Append("Hand: ").Append(WhichHand == XR_Hand.Dominant ? "R, " : "L, ").Append(msg), true);
		}

		private void PrintErrorLog(string msg)
		{
			Log.e(LOG_TAG, RMCSB.Append("Hand: ").Append(WhichHand == XR_Hand.Dominant ? "R, " : "L, ").Append(msg), true);
		}
		#endregion Log

		public enum LoadingState
		{
			LoadingState_NOT_LOADED,
			LoadingState_LOADING,
			LoadingState_LOADED
		}

		public XR_Hand WhichHand = XR_Hand.Dominant;
		public GameObject defaultModel = null;
		public bool updateDynamically = true;
		public bool mergeToOneBone = false;
		[Tooltip("If true, the model will use the built-in texture for controller. This is only used for VIVE's internal app.  It will fallback to use data from native if built-in texture is not found.")]
		public bool useTextureInProject = false;
		[HideInInspector]
		public bool useCompressedTexture = false;

		public delegate void RenderModelDelegate(XR_Hand hand);
		public static event RenderModelDelegate onRenderModelReady = null;
		public static event RenderModelDelegate onRenderModelRemoved = null;

		private GameObject controllerSpawned = null;

		private bool isConnected = false;
		private bool isTracked = false;
		private string renderModelName = "";

		private List<Color32> colors = new List<Color32>();
		private Component[] childArray = null;
		private Material bodyMaterial = null;
		private Material batteryMaterial = null;
		private readonly WaitForEndOfFrame wfef = new WaitForEndOfFrame();
		private readonly WaitForSeconds wfs = new WaitForSeconds(1.0f);
		private bool showBatterIndicator = true;
		private bool isBatteryIndicatorReady = false;
		private int batteryIdx = -1;
		private float batteryUpdateDelay = 200;

		private ModelResource modelResource = null;
		private LoadingState mLoadingState = LoadingState.LoadingState_NOT_LOADED;


		[HideInInspector]
		public bool checkInteractionMode = false;

		// Default is not to show model.  When all model is completed loaded. Check pose valid first.
		private bool showModel = false;

		private bool EnableDirectPreview = false;

		class Component
		{
			private GameObject obj = null;
			private MeshRenderer renderer = null;
			private bool isVisible = false;
			private bool showState = false;

			public Component(GameObject obj, MeshRenderer renderer, bool isVisible) {
				this.obj = obj;
				this.renderer = renderer;
				this.isVisible = isVisible;
				if (obj == null || renderer == null)
					throw new System.ArgumentNullException("Controller's component didn't exist.");
			}

			public void Update(GameObject obj, MeshRenderer renderer, bool isVisible)
			{
				if (this.obj != null) { Destroy(this.obj); }
				this.obj = obj;
				this.renderer = renderer;
				this.isVisible = isVisible;
			}
			public void Clear()
			{
				if (renderer != null && renderer.material != null)
				{
					if (renderer.material.mainTexture != null)
					{
						//Log.i(LOG_TAG, "Component.Clear() texture: " + renderer.material.mainTexture.name, true);
						//Destroy(renderer.material.mainTexture);  // The texture is shared and keeped in ResourceHolder
						renderer.material.mainTexture = null;
					}
					Log.i(LOG_TAG, "Component.Clear() material: " + renderer.material.name, true);
					renderer.material = null;
					Destroy(renderer.material);
				}

				if (obj != null)
				{
					Log.i(LOG_TAG, "Component.Clear(): " + obj.name, true);
					Destroy(obj);
					obj = null;
				}
			}

			// If visible but not show, keep hide.
			public void SetVisibility(bool isVisible)
			{
				this.isVisible = isVisible;
				if (renderer != null) { renderer.enabled = showState && isVisible; }
			}

			public void SetShowState(bool show)
			{
				this.showState = show;
				if (renderer != null) { renderer.enabled = showState && isVisible; }
			}

			public GameObject GetObject() { return obj; }
			public MeshRenderer GetRenderer() { return renderer; }
			public bool IsVisibility() { return isVisible; }
			public bool IsShow() { return showState && isVisible; }
		}

		void OnEnable()
		{
			PrintDebugLog("OnEnable");
			spawnIsRunning = false;
			showModel = false;

#if UNITY_EDITOR
			EnableDirectPreview = EditorPrefs.GetBool("Wave/DirectPreview/EnableDirectPreview", false);
			PrintDebugLog("OnEnterPlayModeMethod: " + EnableDirectPreview);
#endif
			bodyMaterial = new Material(Shader.Find("Unlit/Texture"));

			if (mLoadingState == LoadingState.LoadingState_LOADING)
			{
				DestroyRenderModel("RenderModel doesn't expect model is in loading, delete all children");
			}

			isConnected = WXRDevice.IsConnected((XR_Device)WhichHand);

			if (isConnected)
			{
				WVR_DeviceType type = CheckDeviceType();

				if (mLoadingState == LoadingState.LoadingState_LOADED)
				{
					if (isRenderModelNameSameAsPrevious())
					{
						PrintDebugLog("OnEnable - Controller connected, model was loaded!");
					}
					else
					{
						DestroyRenderModel("Controller load when OnEnable, render model is different!");
						onLoadController(type);
					}
				}
				else
				{
					PrintDebugLog("Controller load when OnEnable!");
					onLoadController(type);
				}
			}

			OEMConfig.onOEMConfigChanged += onOEMConfigChanged;
		}


		void OnDisable()
		{
			PrintDebugLog("OnDisable");

			if (bodyMaterial != null)
			{
				Destroy(bodyMaterial);
				bodyMaterial = null;
			}

			if (batteryMaterial != null)
			{
				Destroy(batteryMaterial);
				batteryMaterial = null;
			}

			if (mLoadingState == LoadingState.LoadingState_LOADING)
			{
				// If spawnIsRunning == true, the coroutine should be stopped.  However, it will be auto stopped.
				if (spawnIsRunning)
					StopCoroutine("SpawnRenderModel");
				DestroyRenderModel("RenderModel doesn't complete creating meshes before OnDisable, delete all children");
			}

			mLoadingState = LoadingState.LoadingState_NOT_LOADED;
			spawnIsRunning = false;
			showModel = false;

			OEMConfig.onOEMConfigChanged -= onOEMConfigChanged;
		}

		void OnDestroy()
		{
			PrintDebugLog("OnDestroy");
		}

		private void onOEMConfigChanged()
		{
			PrintDebugLog("onOEMConfigChanged");
			ReadJsonValues();
		}

		private void ReadJsonValues()
		{
#if UNITY_EDITOR
			if (EnableDirectPreview)
				showBatterIndicator = true;
#else
			showBatterIndicator = false;
#endif
			JSON_BatteryPolicy batteryP = OEMConfig.getBatteryPolicy();

			if (batteryP != null)
			{
				if (batteryP.show == 2)
					showBatterIndicator = true;
			} else
			{
				PrintDebugLog("There is no system policy!");
			}

			PrintDebugLog("showBatterIndicator: " + showBatterIndicator);
		}

		private bool isRenderModelNameSameAsPrevious()
		{
			bool _same = false;
			if (!isConnected) { return _same; }

			WVR_DeviceType type = CheckDeviceType();

			string tmprenderModelName = ClientInterface.GetCurrentRenderModelName(type);

			PrintDebugLog("previous render model: " + renderModelName + ", current " + type + " render model name: " + tmprenderModelName);

			if (tmprenderModelName == renderModelName)
			{
				_same = true;
			}

			return _same;
		}

		void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus) // pause
			{
				PrintInfoLog("Pause(" + pauseStatus + ") and check loading");
				if (mLoadingState == LoadingState.LoadingState_LOADING)
				{
					DestroyRenderModel("Destroy controller prefeb because of spawn process is not completed and app is going to pause.");
				}
			} else
			{
				PrintDebugLog("Resume");
			}
		}

		// Use this for initialization
		void Start()
		{
			PrintDebugLog("start() connect: " + isConnected + " Which hand: " + WhichHand);
			ReadJsonValues();

			if (updateDynamically)
			{
				PrintDebugLog("updateDynamically, start a coroutine to check connection and render model name periodly");
				StartCoroutine(checkRenderModelAndDelete());
			}

			if (this.transform.parent != null)
			{
				PrintDebugLog("start() parent is " + this.transform.parent.name);
			}
		}

		bool checkShowModel()
		{
			bool hasFocus = !Interop.WVR_IsInputFocusCapturedBySystem();
			bool interacable = (!checkInteractionMode || ClientInterface.InteractionMode == XR_InteractionMode.Controller);

			bool show = hasFocus && isConnected && interacable && isTracked;
			if (printIntervalLog)
				PrintDebugLog(Log.CSB
					.Append("checkShowModel() show: ").Append(show)
					.Append(", hasFocus: ").Append(hasFocus)
					.Append(", connected: ").Append(isConnected)
					.Append(", interacable: ").Append(interacable).Append(", checkInteractionMode: ").Append(checkInteractionMode)
					.Append(", tracked: ").Append(isTracked));

			return show;
		}

		void Update()
		{
			logFrame++;
			logFrame %= 300;
			printIntervalLog = (logFrame == 0);

			isConnected = WXRDevice.IsConnected((XR_Device)WhichHand);
			isTracked = WXRDevice.IsTracked((XR_Device)WhichHand);

			if (mLoadingState == LoadingState.LoadingState_NOT_LOADED)
			{
				// Because the controller render model will be destroyed if disconnect, we should load if only connected to avoid load when disconnected.
				// if (isTracked)
				if (isConnected && isTracked)
				{
					WVR_DeviceType type = CheckDeviceType();

					PrintDebugLog("spawn render model");
					onLoadController(type);
				}
			}

			if (mLoadingState == LoadingState.LoadingState_LOADED)
			{
				bool preShowModel = showModel;
				showModel = checkShowModel();

				if (showModel != preShowModel)
				{
					Profiler.BeginSample("ShowHide");
					PrintDebugLog("show model change");

					if (showModel)
					{
						PrintDebugLog("Show render model to previous state");
						if (childArray != null)
						{
							for (int i = 0; i < childArray.Length; i++)
								childArray[i].SetShowState(true);
							Profiler.BeginSample("UpdateBatteryLevel");
							updateBatteryLevel();
							Profiler.EndSample();
						}
					}
					else
					{
						PrintDebugLog("Save render model state and force show to false");

						if (childArray != null)
						{
							for (int i = 0; i < childArray.Length; i++)
								childArray[i].SetShowState(false);
						}
					}
					Profiler.EndSample();
				}

				if (showModel && (batteryUpdateDelay-- < 0))
				{
					Profiler.BeginSample("UpdateBatteryLevel");
					updateBatteryLevel();
					Profiler.EndSample();
					batteryUpdateDelay = 200;
				}
			}

			if (printIntervalLog)
			{
				Profiler.BeginSample("RM Update interval");
				var p = transform.position;
				var sb = Log.CSB
					.Append("Update() Hand: ").Append(WhichHand == XR_Hand.Dominant ? "R" : "L")
					.Append(", connect=").Append(isConnected)
					.Append(", child=").Append(transform.childCount)
					.Append(", showBattery=").Append(showBatterIndicator)
					.Append(", hasBattery=").Append(isBatteryIndicatorReady)
					.Append(", ShowModel=").Append(showModel)
					.Append(", state=").Append(mLoadingState)
					.AppendVector3(", position", p);
				Log.d(LOG_TAG, sb, true);

				if (showModel)
				{
					if (childArray != null)
					{
						for (int i = 0; i < childArray.Length; i++)
						{
							if (childArray[i] != null)
							{
								var obj = childArray[i].GetObject();
								if (obj.name.Equals("__CM__Body"))
								{
									var p2 = obj.transform.position;
									var sb2= Log.CSB
										.Append("Update() Hand: ").Append(WhichHand == XR_Hand.Dominant ? "R" : "L")
										.Append(", model name=").Append(obj.name)
										.Append(", visible=").Append(childArray[i].IsVisibility())
										.AppendVector3(", position", p2);
									Log.d(LOG_TAG, sb2);
								}
							}
						}
					}
				}
				Profiler.EndSample();
			}
		}

		public void applyChange()
		{
			DestroyRenderModel("Setting is changed.");
			WVR_DeviceType type = CheckDeviceType();
			onLoadController(type);
		}

		/// <summary>
		/// Load the <see cref="defaultModel" />.  If defaultModel is null, Load the predefined model from resource folder.
		/// </summary>
		private void LoadDefaultModel(WVR_DeviceType type, bool forceFinch = false)
		{
			if (defaultModel != null)
			{
				PrintInfoLog("Load default model");
				DestroySpawnedController();
				controllerSpawned = Instantiate(defaultModel, this.transform);
				controllerSpawned.transform.parent = this.transform;
				mLoadingState = LoadingState.LoadingState_LOADED;
				return;
			}

			bool rot180 = false;

			string resourceName = "DefaultController/WaveFinchController";
			if (!forceFinch)
			{
				if (type == WVR_DeviceType.WVR_DeviceType_Controller_Left)
					resourceName = "DefaultController/Focus3ControllerModel/Focus3_Left";
				else if (type == WVR_DeviceType.WVR_DeviceType_Controller_Right)
					resourceName = "DefaultController/Focus3ControllerModel/Focus3_Right";

				if (type == WVR_DeviceType.WVR_DeviceType_Controller_Right ||
					type == WVR_DeviceType.WVR_DeviceType_Controller_Left)
					rot180 = true;
			}
			PrintInfoLog("Can't load controller model from DS, and default model is null.  Thus load " + resourceName);

			var prefab = Resources.Load(resourceName) as GameObject;
			if (prefab == null)
			{
				PrintErrorLog("Can't find controller model " + resourceName + " in resources.");
				return;
			}

			DestroySpawnedController();
			var obj = gameObject;
			if (rot180)
			{
				obj = new GameObject("rot180");
				obj.transform.parent = transform;
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			}

			Instantiate(prefab, obj.transform);
			controllerSpawned = obj;
			mLoadingState = LoadingState.LoadingState_LOADED;
		}


		private void onLoadController(WVR_DeviceType type)
		{
			mLoadingState = LoadingState.LoadingState_LOADING;
			PrintDebugLog(Log.CSB
				.Append("onLoadController()").AppendLine()
				.Append("  ").AppendVector3("Pos: ", transform.localPosition).AppendLine()
				.Append("  ").AppendVector3("Rot: ", transform.localEulerAngles).AppendLine()
				.Append("  ").Append("MergeToOneBone: ").Append(mergeToOneBone).AppendLine()
				.Append("  ").Append("type: ").Append(type));

			if (Interop.WVR_GetWaveRuntimeVersion() < 2 && !EnableDirectPreview)
			{
				PrintDebugLog("onLoadController in old service");
				LoadDefaultModel(type, true);
				return;
			}

			renderModelName = ClientInterface.GetCurrentRenderModelName(type);
			PrintDebugLog("RenderModelName=" + renderModelName);

			if (renderModelName.Equals(""))
			{
				PrintDebugLog("Cannot find " + type + " render model name.");
				LoadDefaultModel(type);
				return;
			}

			modelResource = null;

			if (ResourceHolder.Instance.addRenderModel(renderModelName, WhichHand, mergeToOneBone))
			{
				PrintDebugLog("Add " + renderModelName + " model sucessfully!");
			}

			modelResource = ResourceHolder.Instance.getRenderModelResource(renderModelName, WhichHand, mergeToOneBone);

#if UNITY_EDITOR
			// In current DP version, the resouce is not null.
			if (EnableDirectPreview)
				modelResource = null;
#endif

			if (modelResource != null)
			{
				if (spawnIsRunning)
				{
					PrintDebugLog("Spawning.  Not to start again.");
					return;
				}
				mLoadingState = LoadingState.LoadingState_LOADING;

				PrintDebugLog("Starting load " + renderModelName + " model!");

				DestroySpawnedController();
				StartCoroutine(SpawnRenderModel());
			}
			else
			{
				PrintDebugLog("Model is null!");
				LoadDefaultModel(type);
			}
		}

		// Note: if the gameobject is deactivated, the coroutine will be terminated after next yield.
		string emitterMeshName = "__CM__Emitter";
		bool spawnIsRunning = false;
		IEnumerator SpawnRenderModel()
		{
			spawnIsRunning = true;
			{
				float timeoutCountDown = 5.0f;
				while (true)
				{
					if (modelResource != null)
					{
						if (modelResource.parserReady) break;
					}
					PrintDebugLog("SpawnRenderModel is waiting");

					if (timeoutCountDown < 0)
					{
						PrintErrorLog("SpawnRenderModel timeout");
						mLoadingState = LoadingState.LoadingState_NOT_LOADED;
						spawnIsRunning = false;
						yield break;
					}
					yield return wfef;
					timeoutCountDown -= Time.unscaledDeltaTime;
				}
			}

			PrintDebugLog("Start to spawn all meshes!");

			if (modelResource == null)
			{
				PrintDebugLog("modelResource is null, skipping spawn objects");
				mLoadingState = LoadingState.LoadingState_NOT_LOADED;
				yield return null;
			}

			PrintDebugLog("modelResource texture count = " + modelResource.modelTextureCount);

			for (int t = 0; t < modelResource.modelTextureCount; t++)
			{
				TextureInfo mainTexture = modelResource.modelTextureInfos[t];
				if (modelResource.modelTextures[t] == null)
				{
					Profiler.BeginSample("Create Texture");
					Texture2D modelpng = null;
					if (useTextureInProject && (renderModelName.Equals("WVR_CR_Right_001") || renderModelName.Equals("WVR_CR_Left_001")))
					{
						PrintDebugLog("Try use texture in project");
						if (ResourceHolder.Instance.controllerTextureInProject == null)
							ResourceHolder.Instance.controllerTextureInProject =
								Resources.Load<Texture2D>("DefaultController/Focus3ControllerModel/Texture/overlay");
						modelpng = ResourceHolder.Instance.controllerTextureInProject;
						if (modelpng != null)
							modelResource.textureUpsideDowns[t] = true;
					}
					if (modelpng == null && useCompressedTexture)
					{
						PrintDebugLog("Try use compressed texture");
						//TextAsset binaryFile = Resources.Load<TextAsset>("controller00_compressed.ASTC_8x8");
						//if (binaryFile != null)
						{
							//mainTexture.modelTextureData = binaryFile.bytes;
							modelpng = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.ASTC_8x8, false, false);
							modelpng.LoadRawTextureData(mainTexture.modelTextureData);
							modelpng.Apply();
							modelResource.textureUpsideDowns[t] = true;
						}
					}
					if (modelpng == null)
					{
						modelpng = new Texture2D((int)mainTexture.width, (int)mainTexture.height, TextureFormat.RGBA32, false);
						modelpng.LoadRawTextureData(mainTexture.modelTextureData);
						modelpng.Apply();
					}

					// The texture is shared and keeped in ResourceHolder.  And will not create again.
					//if (modelResource.modelTexture[t] != null) { Destroy(modelResource.modelTexture[t]); }
					modelResource.modelTextures[t] = modelpng;
					Profiler.EndSample();
					yield return null;

					Profiler.BeginSample("Texture Dump");
					var sb = new StringBuilder();
					for (int q = 0; q < 10240; q+=1024) {
						sb.Clear().Append("T(").Append(t).Append(") L(").Append(q).Append(")=");
						for (int c = 0; c < 64; c++)
						{
							if ((q * 64 + c) >= mainTexture.modelTextureData.Length)
								break;
							sb.Append(mainTexture.modelTextureData.GetValue(q*64+c).ToString()).Append(" ");
						}
						PrintDebugLog(sb);
					}
					PrintDebugLog("Add [" + t + "] to texture2D");
					Profiler.EndSample();

					mainTexture.modelTextureData = null;
				}
				yield return null;
			}

			if (childArray == null || childArray.Length != modelResource.sectionCount)
			{
				if (childArray == null)
				{
					childArray = new Component[modelResource.sectionCount];
					PrintDebugLog("SpawnRenderModel() initialize the childArray size: " + childArray.Length);
				}
				else // childArray.Length != modelResource.sectionCount
				{
					DestroyRenderModel("SpawnRenderModel() Creates a new child array.");
					childArray = new Component[modelResource.sectionCount];
					PrintDebugLog("SpawnRenderModel() realloc the childArray size: " + childArray.Length);
				}
				for (int i = 0; i < childArray.Length; i++)
				{
					childArray[i] = null;
				}
			}

			string meshName = "";
			for (int i = 0; i < modelResource.sectionCount; i++)
			{
				Profiler.BeginSample("Create Mesh");
				meshName = Marshal.PtrToStringAnsi(modelResource.FBXInfo[i].meshName);

				for (uint j = 0; j < i; j++)
				{
					string tmp = Marshal.PtrToStringAnsi(modelResource.FBXInfo[j].meshName);

					if (tmp.Equals(meshName))
					{
						PrintDebugLog(meshName + " is created! skip.");
						Profiler.EndSample();
						continue;
					}
				}

				Mesh updateMesh = new Mesh();
				GameObject meshGO = new GameObject();
				MeshRenderer meshRenderer = meshGO.AddComponent<MeshRenderer>();
				MeshFilter meshfilter = meshGO.AddComponent<MeshFilter>();
				meshGO.transform.parent = this.transform;
				meshGO.name = meshName;
				Matrix4x4 t = TransformConverter.RigidTransform.toMatrix44(modelResource.FBXInfo[i].matrix, false);

				Vector3 pos = TransformConverter.GetPosition(t);
				pos.z = -pos.z;
				meshGO.transform.localPosition = pos;

				meshGO.transform.localRotation = TransformConverter.GetRotation(t);
				Vector3 angle = meshGO.transform.localEulerAngles;
				angle.x = -angle.x;
				meshGO.transform.localEulerAngles = angle;
				meshGO.transform.localScale = TransformConverter.GetScale(t);
				PrintDebugLog(Log.CSB.Append("i=").Append(i).Append(" MeshGO=").Append(meshName).AppendLine()
					.AppendVector3("  localPosition", meshGO.transform.localPosition).AppendLine()
					.AppendVector3("  localRotation", meshGO.transform.localEulerAngles).AppendLine()
					.AppendVector3("  localScale", meshGO.transform.localScale));

				updateMesh.Clear();
				updateMesh.vertices = modelResource.SectionInfo[i]._vectice;
				Vector2[] uv = modelResource.SectionInfo[i]._uv;
				if (modelResource.textureUpsideDowns[0] && !meshName.Equals("__CM__Battery"))
				{
					uv = new Vector2[modelResource.SectionInfo[i]._uv.Length];
					for (int j = 0; j < uv.Length; j++)
					{
						uv[j].x = modelResource.SectionInfo[i]._uv[j].x;
						uv[j].y = 1.0f - modelResource.SectionInfo[i]._uv[j].y;
					}
				}
				updateMesh.uv = uv;
				updateMesh.uv2 = uv;
				updateMesh.colors32 = colors.ToArray();
				updateMesh.normals = modelResource.SectionInfo[i]._normal;
				updateMesh.SetIndices(modelResource.SectionInfo[i]._indice, MeshTopology.Triangles, 0);
				updateMesh.name = meshName;
				if (meshfilter != null)
				{
					//We just create it.  should not have mesh inside
					//if (meshfilter.mesh != null) { Destroy(meshfilter.mesh); }
					meshfilter.mesh = updateMesh;
				}
				if (meshRenderer != null)
				{
					if (bodyMaterial == null)
					{
						PrintDebugLog("ImgMaterial is null");
					}
					meshRenderer.material = bodyMaterial;
					// The texture is shared and keeped in ResourceHolder
					//if (meshRenderer.material.mainTexture != null) { Destroy(meshRenderer.material.mainTexture); }
					meshRenderer.material.mainTexture = modelResource.modelTextures[0];
					meshRenderer.enabled = false;  // Wait all component is loaded.
				}

				if (meshName.Equals(emitterMeshName))
				{
					PrintDebugLog(meshName + " is found, set " + meshName + " visible: true");
					if (childArray[i] == null)
						childArray[i] = new Component(meshGO, meshRenderer, true);
					else
						childArray[i].Update(meshGO, meshRenderer, true);
				}
				else if (meshName.Equals("__CM__Battery"))
				{
					isBatteryIndicatorReady = false;
					if (modelResource.isBatterySetting)
					{
						if (modelResource.batteryTextureList != null)
						{
							// The Resources cannot be Destroy(), only Resources.UnloadAsset().
							// batteryMaterial = Resources.Load("Materials/WaveBatteryMatR") as Material;
							if (batteryMaterial == null)
								batteryMaterial = new Material(Shader.Find("Unlit/Transparent"));

							//Should not destroy this material.
							//if (meshRenderer.material != null) { Destroy(meshRenderer.material); }
							meshRenderer.material = batteryMaterial;

							foreach (BatteryIndicator bi in modelResource.batteryTextureList)
							{
								TextureInfo ti = bi.batteryTextureInfo;

								if (bi.batteryTexture == null)
								{
									// The texture is shared and keeped in ResourceHolder and will not create it again.
									//if (bi.batteryTexture != null) { Destroy(bi.batteryTexture); }
									bi.batteryTexture = new Texture2D((int)ti.width, (int)ti.height, TextureFormat.RGBA32, false);
									bi.batteryTexture.LoadRawTextureData(ti.modelTextureData);
									bi.batteryTexture.Apply();
									PrintInfoLog("Battery LoadRawTextureData() min: " + bi.min + " max: " + bi.max + " loaded: " + bi.textureLoaded + " w: " + ti.width + " h: " + ti.height + " size: " + ti.size + " array length: " + ti.modelTextureData.Length);
									ti.modelTextureData = null;
								}
								else
								{
									PrintInfoLog("Battery use existed texture"); 
								}
							}

							// The texture is shared and keeped in ResourceHolder and will not create it again.
							//if (meshRenderer.material.mainTexture != null) { Destroy(meshRenderer.material.mainTexture); }
							meshRenderer.material.mainTexture = modelResource.batteryTextureList[0].batteryTexture;
							isBatteryIndicatorReady = true;
						}
					}
					batteryIdx = i;

					if (childArray[i] == null)
						childArray[i] = new Component(meshGO, meshRenderer, false);
					else
						childArray[i].Update(meshGO, meshRenderer, false);

					PrintDebugLog(meshName + " is found, set " + meshName + " visible: False (waiting for update");
				}
				else if (meshName == "__CM__TouchPad_Touch")
				{
					if (childArray[i] == null)
						childArray[i] = new Component(meshGO, meshRenderer, false);
					else
						childArray[i].Update(meshGO, meshRenderer, false);

					PrintDebugLog(meshName + " is found, set " + meshName + " visible: False");
				}
				else
				{
					if (childArray[i] == null)
						childArray[i] = new Component(meshGO, meshRenderer, modelResource.SectionInfo[i]._active);
					else
						childArray[i].Update(meshGO, meshRenderer, modelResource.SectionInfo[i]._active);

					PrintDebugLog("set " + meshName + " visible: " + modelResource.SectionInfo[i]._active);
				}

				Profiler.EndSample();
				yield return wfef;
			}
			PrintDebugLog("send " + WhichHand + " RENDER_MODEL_READY ");

			Profiler.BeginSample("onRenderModelReady");
			onRenderModelReady?.Invoke(WhichHand);
			Profiler.EndSample();

			// This will cause significant GC event in a scene with complex design.
			//Resources.UnloadUnusedAssets();
			mLoadingState = LoadingState.LoadingState_LOADED;
			spawnIsRunning = false;
		}

		void updateBatteryLevel()
		{
			if (batteryIdx >= 0 && childArray[batteryIdx] != null)
			{
				if (showBatterIndicator && isBatteryIndicatorReady && showModel)
				{
					if (modelResource == null || modelResource.batteryTextureList == null)
						return;

					bool found = false;
					WVR_DeviceType type = CheckDeviceType();
					float batteryP = WXRDevice.GetBatteryLevel((XR_Device)type);
					if (batteryP < 0)
					{
						PrintDebugLog("updateBatteryLevel BatteryPercentage is negative, return");
						childArray[batteryIdx].SetVisibility(false);
						return;
					}
					BatteryIndicator currentBattery = null;
					foreach (BatteryIndicator bi in modelResource.batteryTextureList)
					{
						if (batteryP >= bi.min / 100 && batteryP <= bi.max / 100)
						{
							currentBattery = bi;
							found = true;
							break;
						}
					}
					if (found && currentBattery != null)
					{
						childArray[batteryIdx].GetRenderer().material.mainTexture = currentBattery.batteryTexture;
						childArray[batteryIdx].SetVisibility(true);
						PrintDebugLog(Log.CSB.Append("updateBatteryLevel battery level to ").Append(currentBattery.level).Append(", battery percent: ").Append(batteryP));
					}
					else
					{
						childArray[batteryIdx].SetVisibility(false);
					}
				}
				else
				{
					childArray[batteryIdx].SetVisibility(false);
				}
			}
		}

		IEnumerator checkRenderModelAndDelete()
		{
			while (true)
			{
				DeleteControllerWhenDisconnect();
				yield return wfs;
			}
		}

		public void showRenderModel(bool isControllerMode)
		{
			Profiler.BeginSample("ShowRenderModel");
			if (childArray != null)
			{
				for (int i = 0; i < childArray.Length; i++)
				{
					if (childArray[i] != null)
					{
						childArray[i].SetShowState(isControllerMode);
					}
				}
			}
			if (controllerSpawned != null)
				ShowSpawnedController(isControllerMode);
			Profiler.EndSample();
		}

		private void ShowSpawnedController(bool isShow)
		{
			if (controllerSpawned != null)
				controllerSpawned.SetActive(isShow);
		}

		private void DestroyRenderModel(string reason)
		{
			Profiler.BeginSample("DestroyRenderModel");
			PrintDebugLog("DestroyRenderModel() " + reason);

			DeleteChild();
			DestroySpawnedController();

			mLoadingState = LoadingState.LoadingState_NOT_LOADED;
			onRenderModelRemoved?.Invoke(WhichHand);
			//ResourceHolder.Instance.Release(modelResource);
			modelResource = null;

			Profiler.EndSample();
		}

		private void DestroySpawnedController()
		{
			if (controllerSpawned != null)
			{
				Destroy(controllerSpawned);
				controllerSpawned = null;
			}
		}

		private void DeleteChild()
		{
			Profiler.BeginSample("DeleteChild");

			if (childArray == null || childArray.Length == 0)
				return;

			int ca = childArray.Length;
			PrintInfoLog("deleteChild count: " + ca);

			for (int i = 0; i < ca; i++)
			{
				if (childArray[i] != null)
				{
					childArray[i].Clear();
					childArray[i] = null;
				}
			}
			childArray = null;

			Profiler.EndSample();
		}

		private void DeleteControllerWhenDisconnect()
		{
			if (mLoadingState != LoadingState.LoadingState_LOADED)
				return;

			if (isConnected)
			{
				WVR_DeviceType type = CheckDeviceType();

				string tmprenderModelName = ClientInterface.GetCurrentRenderModelName(type);

				if (tmprenderModelName != renderModelName)
				{
					DestroyRenderModel("Destroy controller prefeb because " + type + " render model is different");
				}
			}
			else
			{
				DestroyRenderModel("Destroy controller prefeb because it is disconnect");
			}
			return;
		}

		private WVR_DeviceType CheckDeviceType()
		{
			return (WhichHand == XR_Hand.Right ? WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left);
		}
	}
}
