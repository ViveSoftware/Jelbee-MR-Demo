using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.ScenePerception.Sample
{
    [Serializable]
    public class PassThroughHelper
    {
        [SerializeField] private List<MeshRenderer> RoomPanelMeshRenderers = new List<MeshRenderer>(5);
        [SerializeField] private Material DemoRoomMaterialOpaque = null, DemoRoomMaterialTransparent = null;
        [SerializeField] private Camera hmd;
	    
        public void ShowPassthroughUnderlay(bool show)
        {
            if (show)
            {
                //Set Demo Room Material to transparent and clear skybox to transparent black
                foreach(MeshRenderer meshRender in RoomPanelMeshRenderers)
                {
                    meshRender.material = DemoRoomMaterialTransparent;
                }

                hmd.clearFlags = CameraClearFlags.SolidColor;
                hmd.backgroundColor = new Color(0, 0, 0, 0);
            }
            else
            {
                foreach (MeshRenderer meshRender in RoomPanelMeshRenderers)
                {
                    meshRender.material = DemoRoomMaterialOpaque;
                }

                hmd.clearFlags = CameraClearFlags.Skybox;
            }

            Interop.WVR_ShowPassthroughUnderlay(show);
        }
    }
}
