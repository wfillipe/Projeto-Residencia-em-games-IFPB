Shader "Milthrill/Pixel Darkness"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Raio da abertura", Range(0, 1)) = 0.25
        _PixelSize ("Tamanho do pixel", Range(0.005, 0.08)) = 0.025
        _Center ("Centro da abertura", Vector) = (0.5, 0.44, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            float _Radius;
            float _PixelSize;
            float4 _Center;

            struct VertexIn
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct VertexOut
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            VertexOut Vert(VertexIn input)
            {
                VertexOut output;
                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            fixed4 Frag(VertexOut input) : SV_Target
            {
                float2 offset = input.uv - _Center.xy;
                offset.x *= _ScreenParams.x / _ScreenParams.y;
                float2 cell = floor(offset / _PixelSize + 0.5) * _PixelSize;
                fixed shadow = step(_Radius * _Radius, dot(cell, cell));
                return fixed4(input.color.rgb, input.color.a * shadow);
            }
            ENDCG
        }
    }
}
