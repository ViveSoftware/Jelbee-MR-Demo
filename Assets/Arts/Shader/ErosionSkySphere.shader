Shader "Custom/ErosionSkySphere"
{
    Properties{
        [NoScaleOffset] _MainTex("Skybox", CUBE) = "white" {}
        _NoiseMap("Noise", 2D) = "white" {}
        _ErosionRatio("Max Erosion Ratio ", Range(0, 1)) = 0.2
        _ErosionColor("Erosion Color", Color) = (1, 1, 1, 1)
        _ErosionValue("Erosion Value", Range(0, 1)) = 0
        _ErosionDir("Erosion Direction", Vector) = (0, -1, 0)
        _RotateAxis("Rotate Axis", Vector) = (0, 1, 0)
        _RotateAngularSpeed("Rotate Anglular Speed", float) = 0
        _AlbedoColor("Color Offset", Color) = (1, 1, 1, 1)
        [Header(Stencil)]
        _StencilRef("Stencil Ref", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comp", Int) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpt("Stencil Opt", Int) = 2
    }
    Category
    {
    SubShader{

            Stencil
            {
                Ref [_StencilRef]
                CompBack [_StencilComp]
                PassBack [_StencilOpt]
            }

            Tags {"RenderType" = "Background"}

            Lighting Off
            Cull Off
            CGPROGRAM

            #pragma surface surf NoLighting vertex:vert
            #pragma target 3.0

            samplerCUBE _MainTex;
            sampler2D _NoiseMap;
            float _ErosionRatio;
            float3 _ErosionColor;
            float _ErosionValue;
            float3 _ErosionDir;
            float3 _RotateAxis;
            float _RotateAngularSpeed;
            float PI = 3.1415926;
            float3 _AlbedoColor;

            struct Input {
                half3 worldRefl : TEXCOORD0;
                float3 worldPos;
                float2 uv_MainTex;
                float4 color : COLOR;
            };

            //for flipping normals
            void vert(inout appdata_full v) {
                v.normal.xyz = v.normal * -1;
            }

            float randomRange(float2 Seed, float Min, float Max)
            {
                float randomno = frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453);
                return lerp(Min, Max, randomno);
            }

            void Unity_RotateAboutAxis_Degrees_float(float3 In, float3 Axis, float Rotation, out float3 Out)
            {
                Rotation = radians(Rotation);
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;

                Axis = normalize(Axis);
                float3x3 rot_mat =
                { one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
                };
                Out = mul(rot_mat, In);
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                float3 erosionDir = normalize(_ErosionDir).xyz;

                float d = dot(erosionDir, -o.Normal);

                float offsetRange = (-4 * pow(_ErosionValue, 2) + 4 * _ErosionValue) * _ErosionRatio;

                float3 noise = tex2D(_NoiseMap, IN.uv_MainTex).rgb;

                clip((_ErosionValue * 2 - 1) - d + offsetRange * noise);

                d = (d + 1) / 2;
                o.Emission = pow(saturate(d - _ErosionValue), 1.3) * _ErosionColor;

                Unity_RotateAboutAxis_Degrees_float(IN.worldRefl, _RotateAxis, _RotateAngularSpeed * _Time, IN.worldRefl);
                o.Albedo = texCUBE(_MainTex, WorldReflectionVector(IN, -o.normal)).rgb;
                o.Alpha = 1;
            }

            fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
            {
                fixed4 c;
                c.rgb = s.Albedo * _AlbedoColor;
                c.a = s.Alpha;
                return c;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}