// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SLab/GuideRobotBody"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_OcclusionMap("OcclusionMap", 2D) = "white" {}
		_EmissionMap("EmissionMap", 2D) = "black" {}
		_SpecGlossMap("SpecGlossMap", 2D) = "white" {}
		_BumpMap("BumpMap", 2D) = "bump" {}
		_BumpScale("BumpScale", Float) = 1
		_GlossMapScale("GlossMapScale", Float) = 0
		_ColorMask("ColorMask", 2D) = "white" {}
		[HDR]_FlowLight("FlowLight", Color) = (0,0,0,0)
		_FlowMask("FlowMask", 2D) = "white" {}
		_VoiceVolume("VoiceVolume", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _BumpScale;
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _VoiceVolume;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform sampler2D _ColorMask;
		uniform float4 _ColorMask_ST;
		uniform float4 _FlowLight;
		uniform sampler2D _FlowMask;
		uniform sampler2D _SpecGlossMap;
		uniform float4 _SpecGlossMap_ST;
		uniform float _GlossMapScale;
		uniform sampler2D _OcclusionMap;
		uniform float4 _OcclusionMap_ST;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			o.Albedo = tex2D( _MainTex, uv_MainTex ).rgb;
			float2 uv_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			float4 tex2DNode8 = tex2D( _EmissionMap, uv_EmissionMap );
			float2 uv_ColorMask = i.uv_texcoord * _ColorMask_ST.xy + _ColorMask_ST.zw;
			float4 tex2DNode12 = tex2D( _ColorMask, uv_ColorMask );
			float mulTime27 = _Time.y * 0.3;
			float2 temp_cast_1 = (mulTime27).xx;
			float2 uv_TexCoord45 = i.uv_texcoord + temp_cast_1;
			float mulTime52 = _Time.y * 2.0;
			o.Emission = ( ( _VoiceVolume * tex2DNode8 * tex2DNode12.r ) + ( tex2DNode8 * ( ( _FlowLight * tex2D( _FlowMask, uv_TexCoord45 ).a ) * tex2DNode12.g ) ) + ( tex2DNode8 * tex2DNode12.b * (0.4 + (sin( mulTime52 ) - -1.0) * (1.0 - 0.4) / (1.0 - -1.0)) ) ).rgb;
			float2 uv_SpecGlossMap = i.uv_texcoord * _SpecGlossMap_ST.xy + _SpecGlossMap_ST.zw;
			o.Specular = tex2D( _SpecGlossMap, uv_SpecGlossMap ).rgb;
			o.Smoothness = _GlossMapScale;
			float2 uv_OcclusionMap = i.uv_texcoord * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
			o.Occlusion = tex2D( _OcclusionMap, uv_OcclusionMap ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15700
-1904;120;1604;788;2221.853;-85.27208;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;28;-2895.885,621.0639;Float;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;False;0;0.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;27;-2688.687,626.564;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2322.729,885.5149;Float;False;Constant;_Float2;Float 2;11;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;-2455.916,577.0393;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;17;-1807.886,416.3682;Float;False;Property;_FlowLight;FlowLight;9;1;[HDR];Create;True;0;0;False;0;0,0,0,0;1.5,1.5,1.5,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;52;-2115.531,891.0149;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;44;-2117.282,517.0557;Float;True;Property;_FlowMask;FlowMask;10;0;Create;True;0;0;False;0;None;752185f37af79374e812ad2f39fbac04;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;53;-1905.5,899.8216;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-1564.021,518.451;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;12;-1793.124,657.304;Float;True;Property;_ColorMask;ColorMask;7;0;Create;True;0;0;False;0;None;e43d1c1a4fa5e2e4989344b9e7b3b75b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;54;-1678.579,873.8118;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0.4;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;-1426.875,215.9637;Float;True;Property;_EmissionMap;EmissionMap;2;0;Create;True;0;0;False;0;None;444c02463199fad4bbf76e4bb520e469;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-1306.581,575.6229;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-1017.107,133.4685;Float;False;Property;_VoiceVolume;VoiceVolume;11;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-791.3005,249.0031;Float;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-793.3005,382.0034;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-789.342,508.019;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-913.8535,0.8090973;Float;False;Property;_BumpScale;BumpScale;5;0;Create;True;0;0;False;0;1;0.65;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-390.8536,-174.1909;Float;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;None;25c0ddb75e94a3a42a2ab49d0df28feb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-714.854,-45.19086;Float;True;Property;_BumpMap;BumpMap;4;0;Create;True;0;0;False;0;None;b744b93693dc3b44cb6c114e27a07f85;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-422.9243,592.944;Float;True;Property;_OcclusionMap;OcclusionMap;1;0;Create;True;0;0;False;0;None;1ff3e01a6f351d2438ca69b2590b06c4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;-324.4715,493.5851;Float;False;Property;_GlossMapScale;GlossMapScale;6;0;Create;True;0;0;False;0;0;0.7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1306.581,461.6229;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;16;-1810.586,239.0589;Float;False;Property;_BreatheLight_B;BreatheLight_B;8;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0.9634514,1.985294,1.900728,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-541.949,168.9467;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-423.5538,297.0092;Float;True;Property;_SpecGlossMap;SpecGlossMap;3;0;Create;True;0;0;False;0;None;e664aeb01a43f7a498af04b7fbeb3b94;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;StandardSpecular;SLab/GuideRobotBody;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;27;0;28;0
WireConnection;45;1;27;0
WireConnection;52;0;51;0
WireConnection;44;1;45;0
WireConnection;53;0;52;0
WireConnection;35;0;17;0
WireConnection;35;1;44;4
WireConnection;54;0;53;0
WireConnection;19;0;35;0
WireConnection;19;1;12;2
WireConnection;22;0;55;0
WireConnection;22;1;8;0
WireConnection;22;2;12;1
WireConnection;23;0;8;0
WireConnection;23;1;19;0
WireConnection;20;0;8;0
WireConnection;20;1;12;3
WireConnection;20;2;54;0
WireConnection;3;5;6;0
WireConnection;18;0;16;0
WireConnection;18;1;12;1
WireConnection;26;0;22;0
WireConnection;26;1;23;0
WireConnection;26;2;20;0
WireConnection;0;0;1;0
WireConnection;0;1;3;0
WireConnection;0;2;26;0
WireConnection;0;3;2;0
WireConnection;0;4;7;0
WireConnection;0;5;4;0
ASEEND*/
//CHKSM=8F3B753D6EE4FDB752A3E6C59B72F25AB3485FED