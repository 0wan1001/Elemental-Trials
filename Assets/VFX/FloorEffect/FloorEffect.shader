// 바닥 장판 연출: 40x22 상태 마스크를 부드럽게 늘리고 노이즈로 잘라 유기적인 윤곽을 만든다.
// 안쪽은 종류별(_FX_*)로 노이즈를 이용해 직접 그린다. 결과는 _GridPixels 격자(640x352)에 맞춰 픽셀 단위로 그린다.
// _MainTex = 상태 마스크(R), SpriteRenderer가 넣어 준다.
Shader "ElementalTrials/FloorEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Mask", 2D) = "black" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Threshold ("Threshold", Range(0, 1)) = 0.5
        _NoiseStrength ("Edge Noise Strength", Range(0, 0.6)) = 0.4
        _GridPixels ("Grid Pixels", Vector) = (640, 352, 0, 0)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local _FX_WATER _FX_FIRE _FX_STEAM _FX_STEAMTOP _FX_ELECTRIC _FX_WARNING
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_linear_clamp);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Threshold;
                float _NoiseStrength;
                float4 _GridPixels;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash(i);
                float b = Hash(i + float2(1, 0));
                float c = Hash(i + float2(0, 1));
                float d = Hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // 0~1 범위의 3옥타브 노이즈
            float Fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 3; i++)
                {
                    value += amplitude * Noise(p);
                    p = p * 2.03 + 17.1;
                    amplitude *= 0.5;
                }
                return value / 0.875;
            }

            // 4x4 정렬 디더링 기준값 (0~1)
            float Bayer(float2 pixel)
            {
                const float matrix4[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
                int2 i = int2(fmod(pixel, 4.0));
                return (matrix4[i.y * 4 + i.x] + 0.5) / 16.0;
            }

            // 0~1 값을 steps 단계로 끊되 단계 사이는 디더링으로 섞는다
            float DitherSteps(float value, float steps, float2 pixel)
            {
                float level = saturate(value) * steps;
                float base = floor(level);
                if (frac(level) > Bayer(pixel)) base += 1.0;
                return base / steps;
            }

            float Mask(float2 pixel)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_linear_clamp, (pixel + 0.5) / _GridPixels.xy).r;
            }

            // 마스크 + 가장자리 노이즈. _Threshold 이상이면 장판 안쪽이다.
            float Shape(float2 pixel, float edgeSpeed)
            {
                float t = _Time.y * edgeSpeed;
                float noise = (Noise((pixel + float2(t, t * 0.7)) / 8.0) - 0.5)
                    + (Noise((pixel - float2(t * 0.6, t)) / 4.0 + 31.0) - 0.5) * 0.5;
                return Mask(pixel) + noise * _NoiseStrength;
            }

            half4 Water(float2 pixel, float t)
            {
                float value = Shape(pixel, 1.5);
                clip(value - _Threshold);
                float depth = DitherSteps((value - _Threshold) / 0.3, 2.0, pixel);

                float3 shallow = float3(0.24, 0.42, 0.52);
                float3 deep = float3(0.10, 0.22, 0.34);
                float3 light = float3(0.55, 0.75, 0.82);
                float3 foam = float3(0.78, 0.88, 0.90);
                float3 color = lerp(shallow, deep, depth);

                // 일렁이는 물결: 비튼 노이즈의 등고선
                float2 q = pixel / 28.0;
                float2 warp = float2(Fbm(q + t * 0.07), Fbm(q + 5.2 - t * 0.06));
                float wave = Fbm(q * 1.6 + warp * 1.8 + t * 0.05);
                float band = frac(wave * 5.0 + t * 0.15);
                if (band < 0.07) color = lerp(color, light, 0.55);

                // 드문드문 반짝임
                if (depth > 0.0 && Hash(pixel + floor(t * 3.0) * 13.1) > 0.996) color = foam;

                // 위쪽 가장자리 안쪽은 그늘(움푹 들어간 느낌), 아래쪽 가장자리는 거품
                if (Shape(pixel + float2(0, 2), 1.5) < _Threshold) color *= 0.6;
                if (Shape(pixel - float2(0, 1), 1.5) < _Threshold || value < _Threshold + 0.04) color = lerp(color, foam, 0.6);

                return half4(color, 0.85);
            }

            half4 Fire(float2 pixel, float t)
            {
                float value = Shape(pixel, 3.0);
                float glowWidth = 0.16;
                if (value < _Threshold)
                {
                    // 가장자리 바깥 빛 번짐 (디더링)
                    float glow = 1.0 - (_Threshold - value) / glowWidth;
                    clip(glow * 0.5 - Bayer(pixel));
                    return half4(1.0, 0.45, 0.12, 0.35);
                }
                float depth = saturate((value - _Threshold) / 0.35);

                // 위로 흐르는 불꽃
                float2 q = float2(pixel.x / 14.0, pixel.y / 10.0 - t * 1.8);
                float warp = Fbm(q * 0.6 + float2(t * 0.2, 0));
                float flame = Fbm(q + warp * 1.6);
                float flicker = 0.05 * sin(t * 13.0 + pixel.x * 0.3);
                float heat = DitherSteps(depth * 0.5 + flame * 0.65 - 0.1 + flicker, 4.0, pixel);

                float3 color;
                float alpha = 1.0;
                if (heat < 0.3) { color = float3(0.42, 0.08, 0.06); alpha = 0.8; }
                else if (heat < 0.55) color = float3(0.80, 0.24, 0.07);
                else if (heat < 0.8) color = float3(0.98, 0.52, 0.12);
                else color = float3(1.0, 0.86, 0.45);

                // 솟아오르는 불씨
                if (Hash(float2(pixel.x, floor(pixel.y - t * 24.0))) > 0.993) color = float3(1.0, 0.95, 0.7);

                return half4(color, alpha);
            }

            half4 Steam(float2 pixel, float t)
            {
                // 영역 밖으로도 옅게 번진다
                float value = Shape(pixel, 1.0);
                float density = saturate((value - (_Threshold - 0.2)) / 0.5);

                float2 q = float2(pixel.x / 22.0, pixel.y / 16.0 - t * 0.5);
                float warp = Fbm(q * 0.7 + t * 0.1);
                float fog = Fbm(q + warp * 2.0);
                float d = density * saturate(fog * 1.4 - 0.15);

                float alpha = DitherSteps(d, 4.0, pixel) * 0.45;
                clip(alpha - 0.001);
                float3 color = lerp(float3(0.66, 0.71, 0.76), float3(0.92, 0.94, 0.96), saturate(d));
                return half4(color, alpha);
            }

            half4 SteamTop(float2 pixel, float t)
            {
                // 캐릭터 위로 피어오르는 자락: 마스크를 여러 높이만큼 아래로 밀어 위로 갈수록 옅어지는 기둥을 만든다
                float density = 0.0;
                for (int k = 0; k < 4; k++)
                {
                    float value = Shape(pixel - float2(0, 6 + k * 7), 1.0);
                    density = max(density, saturate((value - (_Threshold - 0.15)) / 0.5) * (1.0 - k * 0.22));
                }

                float2 q = float2(pixel.x / 10.0, pixel.y / 26.0 - t * 0.9);
                float wisp = Fbm(q + Fbm(q * 0.5 + t * 0.15) * 1.5);
                float d = density * saturate((wisp - 0.35) * 2.5);

                float alpha = DitherSteps(d, 3.0, pixel) * 0.5;
                clip(alpha - 0.001);
                return half4(0.9, 0.93, 0.95, alpha);
            }

            half4 Electric(float2 pixel, float t)
            {
                float value = Shape(pixel, 2.0);
                clip(value - _Threshold);

                // 전류 선: 노이즈 능선. 위치가 프레임 단위로 튄다
                float tick = floor(t * 14.0);
                float2 offset = float2(Hash(float2(tick, 1.3)), Hash(float2(tick, 7.9))) * 50.0;
                float a = abs(Fbm(pixel / 16.0 + offset) - 0.5);
                float b = abs(Fbm(pixel / 11.0 + offset * 1.7 + 9.0) - 0.5);
                float arc = min(a, b);
                bool flickerOff = Hash(float2(tick, 3.1)) < 0.12;

                if (!flickerOff && arc < 0.016) return half4(0.95, 0.97, 1.0, 1.0);
                if (!flickerOff && arc < 0.04) return half4(0.45, 0.65, 1.0, 0.9);
                // 아래 물을 푸르게 비추는 옅은 빛
                clip(0.22 - Bayer(pixel));
                return half4(0.4, 0.55, 1.0, 0.35);
            }

            half4 Warning(float2 pixel, float t)
            {
                float value = Shape(pixel, 2.0);
                clip(value - _Threshold);
                float alpha = _Color.a;
                // 흘러가는 사선 줄무늬
                if (frac((pixel.x + pixel.y - t * 20.0) / 8.0) < 0.5) alpha *= 1.4;
                if (value < _Threshold + 0.06) return half4(_Color.rgb * 1.3, saturate(_Color.a * 2.2));
                return half4(_Color.rgb, saturate(alpha));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 pixel = floor(input.uv * _GridPixels.xy);
                float t = _Time.y;
                half4 color;
                #if defined(_FX_WATER)
                    color = Water(pixel, t);
                #elif defined(_FX_FIRE)
                    color = Fire(pixel, t);
                #elif defined(_FX_STEAM)
                    color = Steam(pixel, t);
                #elif defined(_FX_STEAMTOP)
                    color = SteamTop(pixel, t);
                #elif defined(_FX_ELECTRIC)
                    color = Electric(pixel, t);
                #else
                    return Warning(pixel, t);
                #endif
                // 색 상수는 sRGB 값으로 적었다. 리니어 색 공간이면 변환한다.
                #if !defined(UNITY_COLORSPACE_GAMMA)
                    color.rgb = pow(color.rgb, 2.2);
                #endif
                color.rgb *= _Color.rgb;
                return color;
            }
            ENDHLSL
        }
    }
}
