Shader "HTC/hand_new_transparent"
{
	Properties
	{
		
		[Header(Finger Tip Color)]

		[HDR]_fingerTipColor("Color", Color) = (1,0.25,0,0)
		_fingerGlow_thumb("Thumb Color", Range( 0 , 1)) = 0.0		
		_fingerGlow_index("Index Color", Range( 0 , 1)) = 1.0
		_fingerGlow_middle("Middle Color", Range( 0 , 1)) = 0.0
		_fingerGlow_ring("Ring Color", Range( 0 , 1)) = 0.0
		_fingerGlow_pinky("Pinky Color", Range( 0 , 1)) = 0.0
		
		[Space]
		
		[Header(Hand Color)]

		_HandBaseColor("Hand Base Color", Color) = (0.2,0.4,0.7,0)
		_HandAOColor("Hand AO Color", Color) = (1,0,0,0)
		_handOpacity("Hand Opacity", Range( 0 , 1)) = 0.5
		_wristFade("Wrist Fade", Range( 0 , 1)) = 0.0		
		_XrayOpacity("Xray Opacity", Range( 0 , 1)) = 0.5
		
		[Space]

		
		[Header(Rim Light)]
		
		_RimColor("Rim Color", Color) = (0,0.5,0.8,0)
		_RimPower("Rim Power", Float) = 3
		_RimScale("Rim Scale", Float) = 3
		
		[Space]	
		
		[Header(Intersection Effect)]
		_intersectionColor("Intersection Color", Color) = (1,0.5,0,0)
		_intersectionEdge("Intersection Edge Width", Range( 0 , 0.1)) = 0.01
		_edgeNoiseScale("Noise Scale", Float) = 20
		
		[Space]

		[Header(Textures (details in shader code comments))]
				
		[NoScaleOffset]_handTexture1("Finger Mask Texture 1", 2D) = "white" {}  /* Defines finger tip glow.  Thumb(R)  Index(G)  Middle(B) */
		[NoScaleOffset]_handTexture2("Finger Mask Texture 2", 2D) = "white" {}	/* Defines finger tip glow & Ambient Occlusion Texture.  Ring(R)  Pinky(G)  AO(B) */
		[NoScaleOffset]_intersectionNoiseTex("Intersection Noise Texture (RG)", 2D) = "white" {}  /* Noise texture for edge intersection effect */
		//[NoScaleOffset]_opacityTex("Opacity Texture", 2D) = "white" {} 


	}

	SubShader
	{
 		Pass
		{
			ColorMask 0
			ZWrite On
		}
 
		Tags{ "RenderType" = "Transparent"  "Queue" = "Overlay"}
		ZWrite Off
		ZTest Always
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog alpha:fade  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd 
		
		
		
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float2 uv_handTexture1;
			INTERNAL_DATA
		};
		
		uniform float _OutlineWidth;
		uniform float _RimScale;
		uniform float _RimPower;
		uniform float _wristFade;
		uniform float _XrayOpacity;
		uniform float4 _RimColor;
		uniform sampler2D _intersectionNoiseTex;
		
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		
		{
			float4 opacityMask1 = tex2D( _intersectionNoiseTex, i.uv_handTexture1 );
			float3 worldPos = i.worldPos;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
			float3 worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelDot = dot( worldNormal, worldViewDir );
			float fresnelRim = saturate( 0.0 + _RimScale * pow( 1.0 - fresnelDot, _RimPower ) );
			o.Emission = _RimColor.rgb;
			o.Alpha = saturate( fresnelRim * _XrayOpacity * (pow (opacityMask1.b, _wristFade)) );
			o.Normal = float3(0,0,-1);
		}
		ENDCG
		

		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+1" "IsEmissive" = "true"  }
		Cull Back
		ZWrite On
		ZTest LEqual
		//Blend Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 

		struct Input
		{
			float2 uv_handTexture1;
			float2 uv_intersectionNoiseTex;
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
		};

		uniform float _RimScale;
		uniform float _RimPower;
		uniform float _fingerGlow_thumb;
		uniform float _fingerGlow_index;
		uniform float _fingerGlow_middle;
		uniform float _fingerGlow_ring;
		uniform float _fingerGlow_pinky;
		uniform float _intersectionEdge;
		uniform float _edgeNoiseScale;
		uniform float _handOpacity;
		uniform float _wristFade;
		
		uniform float4 _HandAOColor;
		uniform float4 _HandBaseColor;
		uniform float4 _RimColor;
		uniform float4 _fingerTipColor;
		uniform float4 _intersectionColor;
		uniform float4 _CameraDepthTexture_TexelSize;
		
		uniform sampler2D _handTexture2;
		uniform sampler2D _handTexture1;
		uniform sampler2D _intersectionNoiseTex;
		//uniform sampler2D _opacityTex;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
	
		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{

			float4 fingerMask1 = tex2D( _handTexture1, i.uv_handTexture1 );
			float4 fingerMask2 = tex2D( _handTexture2, i.uv_handTexture1 );
			float4 opacityMask = tex2D( _intersectionNoiseTex, i.uv_handTexture1 );
			
			float ao = fingerMask2.b;
			float4 handColor = lerp( _HandAOColor , _HandBaseColor , ao);
			
			float3 worldPos = i.worldPos;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
			float3 worldNormal = i.worldNormal;
			float fresnelDot = dot( worldNormal, worldViewDir );
			float fresnelRim = saturate( 0.0 + _RimScale * pow( 1.0 - fresnelDot, _RimPower ) );
			
			float4 handFinalColor = lerp( handColor , _RimColor , fresnelRim);


			float thumbGlow = ( fingerMask1.r * _fingerGlow_thumb );
			float indexGlow = ( fingerMask1.g * _fingerGlow_index );
			float middleGlow = ( fingerMask1.b * _fingerGlow_middle );
			float ringGlow = ( fingerMask2.r * _fingerGlow_ring );
			float pinkyGlow = ( fingerMask2.g * _fingerGlow_pinky );
			float fingerGlowMask = ( thumbGlow + indexGlow + middleGlow + ringGlow + pinkyGlow );
			
			
			float4 handReallyFinalColor = lerp( handFinalColor, _fingerTipColor, fingerGlowMask);			
			
			float4 screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 screenPosNorm = screenPos / screenPos.w;
			screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPosNorm.z : screenPosNorm.z * 0.5 + 0.5;
			float screenDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, screenPosNorm.xy ));
			float distanceDepth = abs( ( screenDepth - LinearEyeDepth( screenPosNorm.z ) ) / ( _intersectionEdge ) );
			
			float2 uv2_noiseTexCoord = i.uv_intersectionNoiseTex * _edgeNoiseScale;
			float2 panner1 = ( 1.0 * _Time.y * float2( 0.123,0.123 ) + uv2_noiseTexCoord);
			float intersectionEdge = ( 1.0 - saturate( floor( ( distanceDepth - ( tex2D( _intersectionNoiseTex, panner1 ).r - tex2D( _intersectionNoiseTex, (1-panner1) ).b ) ) ) ) );
			
			
			//o.Emission = ( handFinalColor + ( saturate( fingerGlowMask ) * _fingerTipColor ) + ( _intersectionColor * intersectionEdge ) ).rgb;
			o.Emission = ( handReallyFinalColor + ( _intersectionColor * intersectionEdge ) ).rgb;
			o.Alpha = saturate( ( fresnelRim + fingerGlowMask + intersectionEdge + _handOpacity ) * (pow (opacityMask.b, _wristFade)));

			
		}

		ENDCG
	}

}
