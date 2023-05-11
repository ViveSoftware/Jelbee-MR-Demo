// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SLab/GuideRobotElectronicScreen"
{
	Properties
	{
		_NoiseMap("NoiseMap", 2D) = "white" {}
		_MainTex("MainTex", 2D) = "white" {}
		_NoiseStrength("NoiseStrength", Range( 0 , 1)) = 0
		_NoiseMapMaxValue("NoiseMapMaxValue", Float) = 1
		[HDR]_EmissionColor("EmissionColor", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _EmissionColor;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _NoiseMap;
		uniform float4 _NoiseMap_ST;
		uniform float _NoiseMapMaxValue;
		uniform float _NoiseStrength;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_TexCoord21 = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv_NoiseMap = i.uv_texcoord * _NoiseMap_ST.xy + _NoiseMap_ST.zw;
			float2 appendResult8 = (float2(( uv_TexCoord21.x - ( (_MainTex_ST.xy).x * (-0.5 + (tex2D( _NoiseMap, uv_NoiseMap ).r - 0.0) * (0.5 - -0.5) / (_NoiseMapMaxValue - 0.0)) * _NoiseStrength ) ) , uv_TexCoord21.y));
			float4 tex2DNode1 = tex2D( _MainTex, appendResult8 );
			float4 temp_output_17_0 = ( _EmissionColor * tex2DNode1 );
			o.Emission = temp_output_17_0.rgb;
			o.Alpha = tex2DNode1.a;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15700
-1913;31;1906;1004;794.4751;645.4661;1;True;False
Node;AmplifyShaderEditor.TexturePropertyNode;7;-1644.348,143.0183;Float;True;Property;_NoiseMap;NoiseMap;0;0;Create;True;0;0;False;0;None;3b805d5244b69dd409e7c7fb86ee5c61;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-1361.658,207.5561;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;4;-1204.116,-164.4219;Float;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;None;600707fc19637dc4fb056327461601cc;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-1053.386,352.8782;Float;False;Property;_NoiseMapMaxValue;NoiseMapMaxValue;4;0;Create;True;0;0;False;0;1;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-1115.202,140.7562;Float;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureTransformNode;20;-907.1187,-69.28928;Float;False;-1;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.ComponentMaskNode;24;-656.4727,49.52692;Float;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;25;-771.0879,154.1687;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.5;False;4;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1105.423,441.9997;Float;False;Property;_NoiseStrength;NoiseStrength;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;21;-665.1187,-94.28928;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-433.3501,96.02692;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;26;-407.6885,-105.8044;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-252.2657,-73.56339;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;1;-59.88013,-167.1839;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;28;30.5249,-577.4661;Float;False;Property;_EmissionColor;EmissionColor;5;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,1.135552,1.296497,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;364.5249,-417.4661;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;590.5249,-344.4661;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;16;24.46069,-380.16;Float;False;Property;_TintColor;TintColor;2;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0,0.006896734,1,0.4705882;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;362.4607,-303.16;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;894.3532,-254.2843;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;SLab/GuideRobotElectronicScreen;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;2;7;0
WireConnection;6;0;7;0
WireConnection;6;1;10;0
WireConnection;20;0;4;0
WireConnection;24;0;20;0
WireConnection;25;0;6;1
WireConnection;25;2;27;0
WireConnection;21;0;20;0
WireConnection;21;1;20;1
WireConnection;19;0;24;0
WireConnection;19;1;25;0
WireConnection;19;2;18;0
WireConnection;26;0;21;1
WireConnection;26;1;19;0
WireConnection;8;0;26;0
WireConnection;8;1;21;2
WireConnection;1;0;4;0
WireConnection;1;1;8;0
WireConnection;29;0;28;0
WireConnection;29;1;1;4
WireConnection;30;0;29;0
WireConnection;30;1;17;0
WireConnection;17;0;28;0
WireConnection;17;1;1;0
WireConnection;0;2;17;0
WireConnection;0;9;1;4
ASEEND*/
//CHKSM=5CFF26781B72FEC3A13D1EAFE089025BF7CFD6A4