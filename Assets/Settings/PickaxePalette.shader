Shader "Milthrill/Pickaxe Palette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _PickaxeLevel ("Nível da picareta", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _PickaxeLevel;

            struct Attributes { float3 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                SetUpSpriteInstanceProperties();
                output.positionCS = TransformObjectToHClip(UnityFlipSprite(input.positionOS, unity_SpriteProps.xy));
                output.uv = input.uv;
                output.color = input.color * unity_SpriteColor;
                return output;
            }

            float Matches(float3 rgb, float3 palette)
            {
                float3 delta = abs(rgb - palette / 255.0);
                return step(max(delta.r, max(delta.g, delta.b)), 0.55 / 255.0);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                if (_PickaxeLevel > 1.5)
                {
                    float3 rgb = sprite.rgb;
                    #ifndef UNITY_COLORSPACE_GAMMA
                    rgb = LinearToSRGB(rgb);
                    #endif

                    // Apenas os tons de metal da picareta entram na troca de paleta.
                    float dark = Matches(rgb, float3(90, 105, 136));
                    float mid = Matches(rgb, float3(139, 155, 180));
                    float light = Matches(rgb, float3(192, 203, 220));
                    float shine = Matches(rgb, float3(234, 239, 247));
                    float isMetal = dark + mid + light + shine;
                    float3 gold = (dark * float3(108, 77, 19) + mid * float3(164, 123, 28) +
                                   light * float3(204, 163, 50) + shine * float3(231, 191, 78)) / 255.0;
                    float3 cyan = (dark * float3(9, 106, 130) + mid * float3(15, 189, 216) +
                                   light * float3(73, 241, 255) + shine * float3(181, 255, 255)) / 255.0;
                    float3 metal = _PickaxeLevel > 2.5 ? cyan : gold;
                    #ifndef UNITY_COLORSPACE_GAMMA
                    metal = SRGBToLinear(metal);
                    #endif
                    sprite.rgb = lerp(sprite.rgb, metal, isMetal);
                }
                return sprite * input.color;
            }
            ENDHLSL
        }
    }
}
