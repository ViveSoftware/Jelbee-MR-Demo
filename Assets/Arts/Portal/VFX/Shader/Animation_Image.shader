// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Animation_Image"
{
	Properties
	{
		_ExpressionImage("Expression Image", 2D) = "white" {}
		_ImageRow("Image Row", Float) = 0
		_ImageColumns("Image Columns", Float) = 0
		_StartFrame("Start Frame", Float) = 0
		_Speed("Speed", Float) = 0
		_Opacity("Opacity", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		UNITY_DECLARE_TEX2D_NOSAMPLER(_ExpressionImage);
		uniform float _ImageColumns;
		uniform float _ImageRow;
		uniform float _Speed;
		uniform float _StartFrame;
		SamplerState sampler_ExpressionImage;
		uniform float _Opacity;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			// *** BEGIN Flipbook UV Animation vars ***
			// Total tiles of Flipbook Texture
			float fbtotaltiles2 = _ImageColumns * _ImageRow;
			// Offsets for cols and rows of Flipbook Texture
			float fbcolsoffset2 = 1.0f / _ImageColumns;
			float fbrowsoffset2 = 1.0f / _ImageRow;
			// Speed of animation
			float fbspeed2 = _Time.y * _Speed;
			// UV Tiling (col and row offset)
			float2 fbtiling2 = float2(fbcolsoffset2, fbrowsoffset2);
			// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
			// Calculate current tile linear index
			float fbcurrenttileindex2 = round( fmod( fbspeed2 + _StartFrame, fbtotaltiles2) );
			fbcurrenttileindex2 += ( fbcurrenttileindex2 < 0) ? fbtotaltiles2 : 0;
			// Obtain Offset X coordinate from current tile linear index
			float fblinearindextox2 = round ( fmod ( fbcurrenttileindex2, _ImageColumns ) );
			// Multiply Offset X by coloffset
			float fboffsetx2 = fblinearindextox2 * fbcolsoffset2;
			// Obtain Offset Y coordinate from current tile linear index
			float fblinearindextoy2 = round( fmod( ( fbcurrenttileindex2 - fblinearindextox2 ) / _ImageColumns, _ImageRow ) );
			// Reverse Y to get tiles from Top to Bottom
			fblinearindextoy2 = (int)(_ImageRow-1) - fblinearindextoy2;
			// Multiply Offset Y by rowoffset
			float fboffsety2 = fblinearindextoy2 * fbrowsoffset2;
			// UV Offset
			float2 fboffset2 = float2(fboffsetx2, fboffsety2);
			// Flipbook UV
			half2 fbuv2 = i.uv_texcoord * fbtiling2 + fboffset2;
			// *** END Flipbook UV Animation vars ***
			float4 tex2DNode1 = SAMPLE_TEXTURE2D( _ExpressionImage, sampler_ExpressionImage, fbuv2 );
			o.Emission = tex2DNode1.rgb;
			o.Alpha = ( tex2DNode1.a * _Opacity );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
0;151;1378;846;1680.191;660.5153;1.3;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;31;-744.4017,61.57169;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-804.9847,-42.13322;Inherit;False;Property;_StartFrame;Start Frame;3;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1012.158,-325.7035;Inherit;False;Property;_ImageColumns;Image Columns;2;0;Create;True;0;0;False;0;False;0;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-985.0138,-215.2233;Inherit;False;Property;_ImageRow;Image Row;1;0;Create;True;0;0;False;0;False;0;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-726.3765,-394.777;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;9;-751.1291,-150.9589;Inherit;False;Property;_Speed;Speed;4;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;2;-391.0245,-279.3813;Inherit;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;1;-100.9057,-302.4214;Inherit;True;Property;_ExpressionImage;Expression Image;0;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;29;-69.69662,-55.27625;Inherit;False;Property;_Opacity;Opacity;5;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;265.9095,-133.3653;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;38;484.2059,-321.3188;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Animation_Image;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;4;0
WireConnection;2;1;7;0
WireConnection;2;2;8;0
WireConnection;2;3;9;0
WireConnection;2;4;34;0
WireConnection;2;5;31;0
WireConnection;1;1;2;0
WireConnection;39;0;1;4
WireConnection;39;1;29;0
WireConnection;38;2;1;0
WireConnection;38;9;39;0
ASEEND*/
//CHKSM=89CCD63D348598A009515546FA7BE52E20AAC56B