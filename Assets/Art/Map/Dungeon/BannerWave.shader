// A hanging cloth cut out of a painted map: the cloth pixels sway, the top edge stays pinned,
// and the borders of the cut-out never move, so it always lines up with the painting underneath.
Shader "FollowTheCat/Banner Wave"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Amplitude("Sway (share of the width)", Range(0, 0.2)) = 0.06
        _Speed("Speed", Float) = 1.6
        _Frequency("Waves along the cloth", Float) = 5
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

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

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Amplitude;
                float _Speed;
                float _Frequency;
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

            // Cloth is strongly coloured (magenta, gold); the stone behind it is nearly grey.
            float ClothAmount(float2 uv)
            {
                half3 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
                float high = max(c.r, max(c.g, c.b));
                float low = min(c.r, min(c.g, c.b));
                return smoothstep(0.30, 0.50, (high - low) / max(high, 0.02));
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                // 0 at the top (pinned to the pole), growing towards the free bottom edge.
                float hang = smoothstep(0.88, 0.25, uv.y);
                // 0 on the borders of the cut-out, so its edges never shift against the painting.
                float border = smoothstep(0.0, 0.12, uv.x) * smoothstep(1.0, 0.88, uv.x) * smoothstep(0.0, 0.08, uv.y);
                // Each banner starts its wave somewhere else (its world position).
                float start = unity_ObjectToWorld._m03 * 1.7 + unity_ObjectToWorld._m13;
                float wave = sin(_Time.y * _Speed + uv.y * _Frequency + start)
                           + 0.4 * sin(_Time.y * _Speed * 2.3 + uv.y * _Frequency * 1.9 + start);
                float2 shift = float2(_Amplitude * wave, _Amplitude * 0.25 * wave) * hang * border;
                float cloth = max(ClothAmount(uv), ClothAmount(uv + shift));
                input.uv = uv + shift * cloth;
                return CommonLitFragment(input, input.color);
            }
            ENDHLSL
        }
    }
}
