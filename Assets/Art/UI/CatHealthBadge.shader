Shader "FollowTheCat/UI/Cat Health Badge"
{
    Properties
    {
        [PerRendererData] _MainTex ("HUD sprite", 2D) = "white" {}
        _CatPoses ("Cat poses", 2D) = "black" {}
        _WaterLevel ("Water level", Range(0,1)) = 1
        _Fatigue ("Fatigue phase", Range(0,4)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct Vertex { float4 position : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Varying { float4 position : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex, _CatPoses;
            float _WaterLevel, _Fatigue;

            Varying vert(Vertex v)
            {
                Varying o;
                o.position = UnityObjectToClipPos(v.position);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            // Badge coordinates are in pixels on the 320 x 44 standalone frame.
            float2 frameUV(float2 pixel) { return pixel / float2(320, -44) + float2(0, 1); }

            fixed4 frag(Varying i) : SV_Target
            {
                fixed4 frame = tex2D(_MainTex, i.uv);
                float2 p = (i.uv - float2(0, 1)) * float2(320, -44);
                float2 q = abs(p - float2(21.5, 21.5));
                if (max(q.x, q.y) > 13.5 || q.x + q.y > 22.5)
                    return frame * i.color;

                fixed3 base = tex2D(_MainTex, frameUV(float2(21.5, 9.5))).rgb;
                float wave = sin(p.x * 0.28 + _Time.y * 3.6) * 0.65;
                float surface = lerp(35.5, 7.5, _WaterLevel) + wave * sin(_WaterLevel * UNITY_PI);
                float wet = smoothstep(-0.5, 0.5, p.y - surface) * step(0.0001, _WaterLevel);
                float glint = saturate(1 - abs(p.y - surface)) * wet;
                fixed3 red = fixed3(0.43, 0.083, 0.12);
                fixed3 shine = fixed3(0.84, 0.28, 0.28);
                #ifndef UNITY_COLORSPACE_GAMMA
                red = GammaToLinearSpace(red);
                shine = GammaToLinearSpace(shine);
                #endif
                fixed3 water = lerp(base, red, wet * (0.95 - q.y * 0.014));
                water = lerp(water, shine, glint * 0.85);

                // The middle phase uses the original seated inventory crest on the frame.
                if (abs(_Fatigue - 2) < 0.5)
                {
                    float ink = saturate((frame.r - base.r) / max(0.001, base.r * 0.45));
                    return fixed4(lerp(water, frame.rgb, ink), frame.a) * i.color;
                }

                // The four cells contain run, walk, prone and belly-up silhouettes.
                float cell = _Fatigue < 2 ? _Fatigue : _Fatigue - 1;
                float2 local = saturate((p - float2(7.5, 7.5)) / 28);
                float2 poseUV = float2((cell + local.x) * 0.25, 1 - local.y);
                fixed4 cat = tex2D(_CatPoses, poseUV);
                return fixed4(lerp(water, cat.rgb, cat.a), frame.a) * i.color;
            }
            ENDCG
        }
    }
}
