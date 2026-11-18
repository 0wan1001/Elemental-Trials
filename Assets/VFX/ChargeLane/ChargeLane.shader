// 보스 돌진 예고선: 돌진 경로 폭만큼 두 줄 테두리 + 옅은 바탕, 안에서 진행 방향으로 흐르는 꺾쇠,
// 보스 쪽부터 _Progress만큼 차오르는 바탕과 그 앞 머리선, 끝 지점 착지 표시. 픽셀 격자(_SizePixels)에 맞춰 그린다.
// UV: x = 경로 방향(0 보스 ~ 1 끝), y = 폭 방향.
Shader "ElementalTrials/ChargeLane"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Color", Color) = (1, 0.55, 0.2, 1)
        _SizePixels ("Size (px)", Vector) = (300, 48, 0, 0)
        _Progress ("Progress", Range(0, 1)) = 0
        _ChevronSpacing ("Chevron Spacing (px)", Float) = 14
        _ChevronSpeed ("Chevron Speed (px/s)", Float) = 60
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _SizePixels;
                float _Progress;
                float _ChevronSpacing;
                float _ChevronSpeed;
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

            half4 Frag(Varyings input) : SV_Target
            {
                float2 size = _SizePixels.xy;
                float2 pixel = floor(input.uv * size);
                float halfWidth = (size.y - 1.0) * 0.5;
                float across = abs(pixel.y - halfWidth);
                float progressPixel = floor(_Progress * size.x);
                float3 color = _Color.rgb;

                // 두 줄 테두리
                if (pixel.y < 1.0 || pixel.y > size.y - 2.0) return half4(color, 0.9);
                // 끝 지점 착지 표시: 두 줄 세로선
                if (pixel.x > size.x - 3.0 || (pixel.x > size.x - 7.0 && pixel.x < size.x - 5.0)) return half4(color, 0.9);
                // 차오르는 머리선
                if (_Progress < 1.0 && abs(pixel.x - progressPixel) < 1.0) return half4(1, 0.95, 0.8, 0.95);

                // 꺾쇠 ">": 진행 방향으로 흐른다. 가운데가 앞으로 나온 모양.
                float phase = pixel.x - floor(_Time.y * _ChevronSpeed) + across;
                float chevron = fmod(fmod(phase, _ChevronSpacing) + _ChevronSpacing, _ChevronSpacing);
                bool isChevron = chevron < 2.0 && across < halfWidth - 3.0;

                bool isFilled = pixel.x <= progressPixel;
                float alpha = isFilled ? 0.32 : 0.1;
                if (isChevron) alpha = isFilled ? 0.75 : 0.45;
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
