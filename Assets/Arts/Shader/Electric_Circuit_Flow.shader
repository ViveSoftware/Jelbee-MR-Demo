// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Electric_Circuit_Flow"
{
	Properties
	{
		_BaseColor("Base Color", 2D) = "white" {}
		[HDR]_MainColor("Main Color", Color) = (1,1,1,1)
		_EmissionMap("Emission Map", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (1,1,1,1)
		_AO("AO", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalLevel("Normal Level", Float) = 1
		_SpecGlossMap("Spec Gloss Map", 2D) = "white" {}
		_SpecLevel("Spec Level", Float) = 0
		_GlossLevel("Gloss Level", Float) = 0
		_FlowMask("Flow Mask", 2D) = "white" {}
		[Toggle(_FLOWLIGHT_ON)] _FlowLight("Flow Light", Float) = 0
		_FlowSpeed("Flow Speed", Float) = 0
		[HDR]_FlowColor("Flow Color", Color) = (1,1,1,1)
		[Toggle(_BREATHLIGHT_ON)] _BreathLight("Breath Light", Float) = 0
		_BreathSpeed("Breath Speed", Float) = 0
		[HDR]_BreatheColor("Breathe Color", Color) = (1,1,1,1)
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
		#pragma shader_feature_local _FLOWLIGHT_ON
		#pragma shader_feature_local _BREATHLIGHT_ON
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _NormalLevel;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform sampler2D _BaseColor;
		uniform float4 _BaseColor_ST;
		uniform float4 _MainColor;
		uniform float4 _EmissionColor;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform float4 _FlowColor;
		uniform sampler2D _FlowMask;
		uniform float _FlowSpeed;
		uniform float4 _BreatheColor;
		uniform float _BreathSpeed;
		uniform float _SpecLevel;
		uniform sampler2D _SpecGlossMap;
		uniform float4 _SpecGlossMap_ST;
		uniform float _GlossLevel;
		uniform sampler2D _AO;
		uniform float4 _AO_ST;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _NormalLevel );
			float2 uv_BaseColor = i.uv_texcoord * _BaseColor_ST.xy + _BaseColor_ST.zw;
			o.Albedo = ( tex2D( _BaseColor, uv_BaseColor ) * _MainColor ).rgb;
			float2 uv_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			float4 temp_output_60_0 = ( _EmissionColor * tex2D( _EmissionMap, uv_EmissionMap ) );
			float mulTime27 = _Time.y * _FlowSpeed;
			float2 temp_cast_1 = (mulTime27).xx;
			float2 uv_TexCoord45 = i.uv_texcoord + temp_cast_1;
			#ifdef _FLOWLIGHT_ON
				float4 staticSwitch77 = ( _FlowColor * tex2D( _FlowMask, uv_TexCoord45 ).a );
			#else
				float4 staticSwitch77 = float4( 0,0,0,0 );
			#endif
			float mulTime52 = _Time.y * _BreathSpeed;
			#ifdef _BREATHLIGHT_ON
				float4 staticSwitch72 = ( _BreatheColor * (0.4 + (sin( mulTime52 ) - -1.0) * (1.0 - 0.4) / (1.0 - -1.0)) );
			#else
				float4 staticSwitch72 = float4( 0,0,0,0 );
			#endif
			o.Emission = ( ( float4( 0,0,0,0 ) * temp_output_60_0 ) + ( temp_output_60_0 * staticSwitch77 ) + ( temp_output_60_0 * staticSwitch72 ) ).rgb;
			float2 uv_SpecGlossMap = i.uv_texcoord * _SpecGlossMap_ST.xy + _SpecGlossMap_ST.zw;
			o.Specular = ( _SpecLevel * tex2D( _SpecGlossMap, uv_SpecGlossMap ) ).rgb;
			o.Smoothness = _GlossLevel;
			float2 uv_AO = i.uv_texcoord * _AO_ST.xy + _AO_ST.zw;
			o.Occlusion = tex2D( _AO, uv_AO ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17900
-1856;95;1247;811;1271.465;528.2711;1.3;True;True
Node;AmplifyShaderEditor.RangedFloatNode;28;-3048.202,690.9968;Float;False;Property;_FlowSpeed;Flow Speed;12;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2008.14,943.4401;Float;False;Property;_BreathSpeed;Breath Speed;15;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;27;-2841.004,696.4969;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;52;-1803.821,947.5;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;-2608.233,646.9722;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;53;-1593.791,956.307;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;-2164.429,265.2569;Float;False;Property;_FlowColor;Flow Color;13;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0,1.519586,2.118547,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;44;-2323.336,507.6211;Inherit;True;Property;_FlowMask;Flow Mask;10;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;54;-1425.75,878.917;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0.4;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;69;-1479.418,658.9409;Float;False;Property;_BreatheColor;Breathe Color;16;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0,1.137255,2,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;8;-1695.646,-23.24567;Inherit;True;Property;_EmissionMap;Emission Map;2;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1233.099,706.1918;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1891.612,420.1937;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;67;-1647.076,-216.5266;Inherit;False;Property;_EmissionColor;Emission Color;3;1;[HDR];Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1266.09,53.44094;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;72;-1063.928,552.1091;Inherit;False;Property;_BreathLight;Breath Light;14;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;77;-1700.357,365.6065;Inherit;False;Property;_FlowLight;Flow Light;11;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-396.7405,232.2051;Float;False;Property;_SpecLevel;Spec Level;8;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;57;-555.9509,-319.0905;Inherit;False;Property;_MainColor;Main Color;1;1;[HDR];Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-789.342,508.019;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-791.3005,249.0031;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-655.942,-540.8305;Inherit;True;Property;_BaseColor;Base Color;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-793.3005,382.0034;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-575.2368,359.7281;Inherit;True;Property;_SpecGlossMap;Spec Gloss Map;7;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;83;-632.5649,-9.971135;Float;False;Property;_NormalLevel;Normal Level;6;0;Create;True;0;0;False;0;1;0.65;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-235.4751,468.164;Float;False;Property;_GlossLevel;Gloss Level;9;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-541.949,168.9467;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-223.0659,230.3911;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-207.7728,-390.8448;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;81;-415.2279,623.4873;Inherit;True;Property;_AO;AO;4;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;84;-433.5655,-55.9711;Inherit;True;Property;_NormalMap;Normal Map;5;0;Create;True;0;0;False;0;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;105.6803,-19.7031;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Electric_Circuit_Flow;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;27;0;28;0
WireConnection;52;0;51;0
WireConnection;45;1;27;0
WireConnection;53;0;52;0
WireConnection;44;1;45;0
WireConnection;54;0;53;0
WireConnection;18;0;69;0
WireConnection;18;1;54;0
WireConnection;71;0;17;0
WireConnection;71;1;44;4
WireConnection;60;0;67;0
WireConnection;60;1;8;0
WireConnection;72;0;18;0
WireConnection;77;0;71;0
WireConnection;20;0;60;0
WireConnection;20;1;72;0
WireConnection;22;1;60;0
WireConnection;23;0;60;0
WireConnection;23;1;77;0
WireConnection;26;0;22;0
WireConnection;26;1;23;0
WireConnection;26;2;20;0
WireConnection;66;0;65;0
WireConnection;66;1;2;0
WireConnection;58;0;1;0
WireConnection;58;1;57;0
WireConnection;84;5;83;0
WireConnection;0;0;58;0
WireConnection;0;1;84;0
WireConnection;0;2;26;0
WireConnection;0;3;66;0
WireConnection;0;4;78;0
WireConnection;0;5;81;0
ASEEND*/
//CHKSM=2EF9623DAD51586DAB2EB0F86237315C3FBB188E