Shader "HTC/SkyboxViewer"
{
    Properties
    {
        _MainTex("Skybox", CUBE) = "white" {}
        [Enum(Off, 0, On, 1)] _ZWriteMode("ZWriteMode", float) = 1
    }

        SubShader
    {
        Tags { "Queue" = "Geometry-1" }

        Pass
        {
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.position = UnityObjectToClipPos(input.vertex);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                return fixed4(0.0, 0.0, 0.0, 0.0);
            }
            ENDCG
        }
    }
}