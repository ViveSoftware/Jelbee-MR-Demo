Shader "HTC/Stencil/Viewport"
{
    Properties
    {
        _StencilRef("Stencil Ref", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comp", Int) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpt("Stencil Opt", Int) = 2

        _MainTex("Mask", 2D) = "white" {}
    }

    SubShader
    {               
        Tags { "RenderType"="Geometry-1" }

        Pass
        {
            ColorMask 0
            ZWrite Off
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilComp]
                Pass [_StencilOpt]
            }
        }
    }
}
