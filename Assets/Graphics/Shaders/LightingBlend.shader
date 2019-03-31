Shader "Engine/LightingBlend"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "black" {}
        _LightingTex("Lighting Texture", 2D) = "black" {}
        _Multiplier("Brightness", float) = 1.0
        _Multiplier("MonochromeFactorR", float) = 0.2989
        _Multiplier("MonochromeFactorG", float) = 0.5870
        _Multiplier("MonochromeFactorB", float) = 0.1140
        _Multiplier("MonochromeDisplayR", float) = 0.0
        _Multiplier("MonochromeDisplayG", float) = 1.0
        _Multiplier("MonochromeDisplayB", float) = 0.0
        _Multiplier("MonochromeAmount", float) = 0.0
    }

        SubShader
        {

            Pass
        {
            CGPROGRAM

    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"

            struct appdata_t
        {
            float4 vertex   : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex   : SV_POSITION;
            half2 uv : TEXCOORD0;
        };

        v2f vert(appdata_t i)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(i.vertex);
            o.uv = i.uv;

            return o;
        }

        sampler2D _MainTex;
        sampler2D _LightingTex;
        float _Brightness;
        float _MonochromeFactorR;
        float _MonochromeFactorG;
        float _MonochromeFactorB;
        float _MonochromeDisplayR;
        float _MonochromeDisplayG;
        float _MonochromeDisplayB;
        float _MonochromeAmount;

        half4 frag(v2f IN) : SV_Target
        {
            fixed4 base = tex2D(_MainTex, IN.uv);
            fixed lightContribution = base.a;
            fixed4 lighting = tex2D(_LightingTex, IN.uv) * 0.75;
            // Lighting.a is available for additional effects

            fixed4 col = base * lighting * _Brightness;

            fixed mono = col.r * _MonochromeFactorR + col.g * _MonochromeFactorG + col.b * _MonochromeFactorB;
            fixed4 monoDisplay = fixed4(mono * _MonochromeDisplayR, mono * _MonochromeDisplayG, mono * _MonochromeDisplayB, 1.0);
            fixed4 albedoAndLight = monoDisplay * _MonochromeAmount + col * (1.0 - _MonochromeAmount);
            return lightContribution * albedoAndLight + (1 - lightContribution) * base;
        }

            ENDCG
        }
        }
}
