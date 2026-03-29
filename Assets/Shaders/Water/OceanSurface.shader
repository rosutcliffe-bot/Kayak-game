// KayakSimulator – Gerstner Ocean Surface Shader
// Compatible with Unity's Universal Render Pipeline (URP).
//
// Implements:
//   • Up to 4 Gerstner waves (direction, amplitude, wavelength, speed, steepness)
//   • Depth-based colour blending (shallow → deep)
//   • Screen-space reflections approximation via reflection probe sampling
//   • Refraction using GrabPass / camera opaque texture
//   • Foam at wave crests based on Jacobian determinant
//   • Normal-map animation for fine surface detail

Shader "KayakSimulator/OceanSurface"
{
    Properties
    {
        // ----------------------------------------------------------------
        // Colour
        // ----------------------------------------------------------------
        _ShallowColor  ("Shallow Water Colour", Color)  = (0.1, 0.6, 0.7, 0.85)
        _DeepColor     ("Deep Water Colour",    Color)  = (0.02, 0.1, 0.3, 1.0)
        _DepthDistance ("Depth Distance",       Float)  = 5.0

        // ----------------------------------------------------------------
        // Foam
        // ----------------------------------------------------------------
        _FoamTex       ("Foam Texture",         2D)     = "white" {}
        _FoamThreshold ("Foam Threshold",       Range(0,1)) = 0.6
        _FoamColor     ("Foam Colour",          Color)  = (1,1,1,1)

        // ----------------------------------------------------------------
        // Normal maps
        // ----------------------------------------------------------------
        _NormalMap1    ("Normal Map 1",         2D)     = "bump" {}
        _NormalMap2    ("Normal Map 2",         2D)     = "bump" {}
        _NormalStrength("Normal Strength",      Range(0, 3)) = 1.2
        _NormalSpeed1  ("Normal Scroll Speed 1",Vector) = (0.02, 0.01, 0, 0)
        _NormalSpeed2  ("Normal Scroll Speed 2",Vector) = (-0.01, 0.03, 0, 0)

        // ----------------------------------------------------------------
        // Specular / Fresnel
        // ----------------------------------------------------------------
        _Smoothness    ("Smoothness",           Range(0, 1)) = 0.92
        _FresnelPower  ("Fresnel Power",        Range(0.5, 8)) = 3.0
        _ReflectionColor("Reflection Tint",     Color)  = (0.8, 0.9, 1.0, 1.0)

        // ----------------------------------------------------------------
        // Wave parameters (set from GerstnerWaveSystem.cs each frame)
        // ----------------------------------------------------------------
        // _WaveA[i] = (dirX, dirZ, amplitude, wavelength)
        // _WaveB[i] = (speed, steepness, 0, 0)
        [HideInInspector] _WaveCount ("Wave Count", Int) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent-10"
            "RenderPipeline"  = "UniversalPipeline"
        }

        // Grab the scene behind us for refraction
        GrabPass { "_GrabTexture" }

        Pass
        {
            Name "OceanSurface"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex   OceanVert
            #pragma fragment OceanFrag
            #pragma target   3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ----------------------------------------------------------------
            // Properties
            // ----------------------------------------------------------------
            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float  _DepthDistance;

                float4 _FoamColor;
                float  _FoamThreshold;

                float  _NormalStrength;
                float4 _NormalSpeed1;
                float4 _NormalSpeed2;
                float  _Smoothness;
                float  _FresnelPower;
                float4 _ReflectionColor;

                int    _WaveCount;

                // Gerstner wave arrays (up to 4)
                float4 _WaveA[4];   // dirX, dirZ, amplitude, wavelength
                float4 _WaveB[4];   // speed, steepness, -, -
            CBUFFER_END

            TEXTURE2D(_FoamTex);        SAMPLER(sampler_FoamTex);
            TEXTURE2D(_NormalMap1);     SAMPLER(sampler_NormalMap1);
            TEXTURE2D(_NormalMap2);     SAMPLER(sampler_NormalMap2);
            TEXTURE2D(_GrabTexture);    SAMPLER(sampler_GrabTexture);
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

            // ----------------------------------------------------------------
            // Vertex data
            // ----------------------------------------------------------------
            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                float4 screenPos   : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
                float  foamFactor  : TEXCOORD4;
            };

            // ----------------------------------------------------------------
            // Gerstner wave displacement (vertex stage)
            // ----------------------------------------------------------------
            float3 GerstnerDisplacement(float3 posWS)
            {
                float3 displacement = float3(0, 0, 0);
                float  t            = _Time.y;
                const  float g      = 9.81;

                for (int i = 0; i < _WaveCount; i++)
                {
                    float2 dir    = normalize(_WaveA[i].xy);
                    float  amp    = _WaveA[i].z;
                    float  wl     = _WaveA[i].w;
                    float  speed  = _WaveB[i].x;
                    float  steep  = _WaveB[i].y;

                    float  k      = 6.28318 / wl;          // 2π / λ
                    float  omega  = sqrt(g * k);
                    float  phase  = k * dot(dir, posWS.xz) - omega * t;
                    float  sinP   = sin(phase);
                    float  cosP   = cos(phase);
                    float  Qi     = steep / (k * amp * max(_WaveCount, 1));

                    displacement.x += Qi * amp * dir.x * cosP;
                    displacement.z += Qi * amp * dir.y * cosP;
                    displacement.y += amp * sinP;
                }
                return displacement;
            }

            // Approximate foam from wave steepness (Jacobian sign flip)
            float ComputeFoam(float3 posWS)
            {
                float foam = 0;
                float t    = _Time.y;
                const float g = 9.81;

                for (int i = 0; i < _WaveCount; i++)
                {
                    float2 dir  = normalize(_WaveA[i].xy);
                    float  amp  = _WaveA[i].z;
                    float  wl   = _WaveA[i].w;
                    float  k    = 6.28318 / wl;
                    float  omega = sqrt(g * k);
                    float  phase = k * dot(dir, posWS.xz) - omega * t;
                    float  steep = _WaveB[i].y;
                    float  Qi    = steep / (k * amp * max(_WaveCount, 1));

                    // Jacobian of wave function
                    foam += Qi * k * amp * cos(phase);
                }
                return saturate(foam - _FoamThreshold) / (1.0 - _FoamThreshold);
            }

            // ----------------------------------------------------------------
            // Vertex shader
            // ----------------------------------------------------------------
            Varyings OceanVert(Attributes IN)
            {
                Varyings OUT;

                float3 posWS = TransformObjectToWorld(IN.positionOS);
                float3 disp  = GerstnerDisplacement(posWS);
                posWS       += disp;

                OUT.positionCS = TransformWorldToHClip(posWS);
                OUT.positionWS = posWS;
                OUT.uv         = IN.uv;
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                OUT.viewDirWS  = normalize(GetCameraPositionWS() - posWS);
                OUT.foamFactor = ComputeFoam(posWS);

                return OUT;
            }

            // ----------------------------------------------------------------
            // Fragment shader
            // ----------------------------------------------------------------
            half4 OceanFrag(Varyings IN) : SV_Target
            {
                // ----- Normal maps (scroll at different speeds) -----
                float2 uv1 = IN.positionWS.xz * 0.15 + _NormalSpeed1.xy * _Time.y;
                float2 uv2 = IN.positionWS.xz * 0.08 + _NormalSpeed2.xy * _Time.y;
                float3 n1  = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap1, sampler_NormalMap1, uv1));
                float3 n2  = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap2, sampler_NormalMap2, uv2));
                float3 nm  = normalize(float3(
                    (n1.xy + n2.xy) * _NormalStrength * 0.5,
                    n1.z));

                // World-space normal (ocean surface faces up by default)
                float3 N = normalize(float3(nm.x, 1.0, nm.y));

                // ----- Depth fade -----
                float2 screenUV  = IN.screenPos.xy / IN.screenPos.w;
                float  sceneZ    = LinearEyeDepth(
                    SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV).r,
                    _ZBufferParams);
                float  surfaceZ  = IN.screenPos.w;
                float  depthDiff = saturate((sceneZ - surfaceZ) / _DepthDistance);
                half4  baseColor = lerp(_ShallowColor, _DeepColor, depthDiff);

                // ----- Refraction -----
                float2 refrOffset = nm.xy * 0.03;
                half4  refracted  = SAMPLE_TEXTURE2D(_GrabTexture, sampler_GrabTexture,
                                        screenUV + refrOffset);

                // ----- Fresnel reflection -----
                float  fresnel   = pow(1.0 - saturate(dot(N, IN.viewDirWS)), _FresnelPower);
                half4  reflected = _ReflectionColor * fresnel;

                // ----- Specular highlight -----
                Light  mainLight   = GetMainLight();
                float3 halfDir     = normalize(mainLight.direction + IN.viewDirWS);
                float  spec        = pow(saturate(dot(N, halfDir)), 256.0 * _Smoothness);

                // ----- Foam -----
                float2 foamUV  = IN.positionWS.xz * 0.3 + _Time.y * 0.05;
                half   foamTex = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, foamUV).r;
                half   foam    = IN.foamFactor * foamTex;

                // ----- Composite -----
                half4 color = lerp(refracted, baseColor, baseColor.a);
                color.rgb  += reflected.rgb;
                color.rgb  += spec * mainLight.color;
                color.rgb   = lerp(color.rgb, _FoamColor.rgb, foam);
                color.a     = lerp(_ShallowColor.a, 1.0, depthDiff);

                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
