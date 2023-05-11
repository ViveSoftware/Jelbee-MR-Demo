Shader "HTC/Stencil/Hole"
{
	Properties
	{
		_StencilRef("Stencil Ref", Int) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comp", Int) = 8
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOpt("Stencil Opt", Int) = 2
		[Enum(Off, 0, On, 1)] _ZWriteMode("ZWriteMode", float) = 1
		_VisibleRangeLeft("Visivle Range(Left)(meter)", float) = 1
		_VisibleRangeRight("Visivle Range(Right)(meter)", float) = 1
	}

	SubShader
	{
		Tags { "Queue" = "Geometry-1" }
		Pass
		{
			Stencil
			{
				Ref[_StencilRef]
				Comp[_StencilComp]
				Pass[_StencilOpt]
			}
		
			ZWrite[_ZWriteMode]
			ColorMask 0
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
		
			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
	
			struct v2f
			{
				float4 position : SV_POSITION;
				float3 localPos : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};
				
			float3 calculateWorldScale()
			{
				return float3(
					length(unity_ObjectToWorld._m00_m10_m20),
					length(unity_ObjectToWorld._m01_m11_m21),
					length(unity_ObjectToWorld._m02_m12_m22)
					);
			}
			
			v2f vert(appdata input)
			{
				v2f output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.localPos = calculateWorldScale() * input.vertex.xyz;
				output.position = UnityObjectToClipPos(input.vertex);
				return output;
			}
			
			float _VisibleRangeLeft;
			float _VisibleRangeRight;
			
			fixed4 frag(v2f input) : SV_Target
			{
				clip(input.localPos.x + _VisibleRangeLeft);
				clip(_VisibleRangeRight - input.localPos.x);
				return fixed4(0.0, 0.0, 0.0, 0.0);
			}
			ENDCG
		}
	}
}