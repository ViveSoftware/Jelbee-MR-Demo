using System;
using UnityEngine;
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
			var target = scenePerceptionHelper.target;

			switch (target)
			{
				case ScenePerceptionHelper.SceneTarget.TwoDimensionPlane:
					if (!scenePerceptionHelper.StateIsCompleted(target))
					{
						if (scenePerceptionHelper.StateIsEmpty(target))
							generatedPlaneContainer.Dispose();
						if (Log.gpl.Print)
							Log.w(LOG_TAG, "UpdateScenePerceptionMesh: Perception state is not complete, cannot generate mesh.");
					}
					break;
				case ScenePerceptionHelper.SceneTarget.SceneMesh:
					if (!scenePerceptionHelper.StateIsCompleted(target))
					{
						if (scenePerceptionHelper.StateIsEmpty(target))
							generatedSceneMeshContainer.Dispose();
						if (Log.gpl.Print)
							Log.w(LOG_TAG, "UpdateScenePerceptionMesh: Perception state is not complete, cannot generate mesh.");
					}
					break;
				case ScenePerceptionHelper.SceneTarget.ThreeDimensionObject:
				default:
					break;
			}

            switch (target)
            {
                case ScenePerceptionHelper.SceneTarget.TwoDimensionPlane:
                    generatedPlaneContainer.UpdateAssumingThePerceptionTargetIsCompleted();
                    break;
                case ScenePerceptionHelper.SceneTarget.SceneMesh:
					generatedSceneMeshContainer.UpdateAssumingThePerceptionTargetIsCompleted();
					break;
				case ScenePerceptionHelper.SceneTarget.ThreeDimensionObject:
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
