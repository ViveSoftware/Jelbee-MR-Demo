using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Essence.ScenePerception.Sample;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
    public class ScenePerceptionMeshFacade
    {
        private readonly ScenePerceptionHelper scenePerceptionHelper;
        private readonly GeneratedPlaneContainer generatedPlaneContainer;
		private readonly GeneratedSceneMeshContainer generatedSceneMeshContainer;

		private const string LOG_TAG = "ScenePerceptionMeshFacade";

		public ScenePerceptionMeshFacade(ScenePerceptionHelper scenePerceptionHelper,GameObject anchorDisplayPrefab,Material generatedMeshMaterialTranslucent, Material generatedMeshMaterialWireframe)
        {
            this.scenePerceptionHelper = scenePerceptionHelper ?? throw new ArgumentNullException(nameof(scenePerceptionHelper));
            if(generatedMeshMaterialTranslucent == null) throw new ArgumentNullException(nameof(generatedMeshMaterialTranslucent)); 
            generatedPlaneContainer = new GeneratedPlaneContainer(scenePerceptionHelper.scenePerceptionManager,generatedMeshMaterialTranslucent,anchorDisplayPrefab);
			generatedSceneMeshContainer = new GeneratedSceneMeshContainer(scenePerceptionHelper.scenePerceptionManager,generatedMeshMaterialWireframe);

		}
	    
        public void UpdateScenePerceptionMesh()
        {
            if (!scenePerceptionHelper.CurrentPerceptionTargetIsCompleted())
            {
				if (scenePerceptionHelper.CurrentPerceptionTargetIsEmpty())
				{
					switch (scenePerceptionHelper.currentPerceptionTarget)
					{
						case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane:
							generatedPlaneContainer.Dispose();
							break;
						case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh:
							generatedSceneMeshContainer.Dispose();
							break;
						case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject:
						default:
							break;
					}
				}

				Log.e(LOG_TAG, "UpdateScenePerceptionMesh: Perception state not complete, cannot generate mesh.");
				return;
            }

            switch(scenePerceptionHelper.currentPerceptionTarget)
            {
                case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane:
                    generatedPlaneContainer.UpdateAssumingThePerceptionTargetIsCompleted();
                    break;
                case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh:
					generatedSceneMeshContainer.UpdateAssumingThePerceptionTargetIsCompleted();
					break;
				case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject:
				default:
                    break;
            }
        }

		public void ChangeSceneMeshType(WVR_SceneMeshType sceneMeshType)
		{
			generatedSceneMeshContainer.currentSceneMeshType = sceneMeshType;
		}

        public void DestroyGeneratedMeshes()
        {
            generatedPlaneContainer.Dispose();
			generatedSceneMeshContainer.Dispose();
        }
    }
}
