Shader "FollowTheCat/Dungeon Root Ripple"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex LitVertex
            #pragma fragment LitFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color        : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color        : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            Varyings LitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonLitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;

                return o;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                half4 surface = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float2 uv = input.uv;
                // Relative chroma picks up dark violet bark in linear lighting.
                float violet = min(surface.r, surface.b) - surface.g;
                float bark = smoothstep(0.045, 0.22, violet / max(0.008, max(surface.r, surface.b)));
                float2 fromMouth = uv - float2(0.50, 0.53);
                float radius = length(fromMouth);
                float phase = 18.0 * radius + 2.5 * uv.x - 2.15 * _Time.y;
                float2 tangent = float2(-fromMouth.y, fromMouth.x) / max(radius, 0.08);
                float weight = bark * surface.a * smoothstep(0.045, 0.16, radius);
                input.uv += weight * (0.0065 * tangent * sin(phase)
                                     + float2(0.0015, 0.002) * sin(phase * 1.5));
                half4 lit = CommonLitFragment(input, input.color);
                float crest = pow(saturate(0.5 + 0.5 * cos(phase)), 5.0);
                lit.rgb += half3(0.065, 0.008, 0.10) * (weight * crest);
                return lit;
            }
            ENDHLSL
        }

    }
}
