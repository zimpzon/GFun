Shader "GFun/LightingBlend2"
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
		_AspectRatio("Aspect ratio", float) = 1
		_Darkness("Darkness amount", float) = 1
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
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
			float4 screenPos : float4;
            float4 vertex   : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        v2f vert(appdata_t i)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(i.vertex);
			o.screenPos = ComputeScreenPos(o.vertex);
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
        float _AspectRatio;
        float _Darkness;

        float4 frag(v2f IN) : SV_Target
        {
            float4 base = tex2D(_MainTex, IN.uv);
			float2 screenPos = IN.screenPos;
			float distX = (0.5f - screenPos.x) * _AspectRatio;
			float distY = 0.5f - screenPos.y;
			float dist2 = sqrt(distX * distX + distY * distY) * 2;
			dist2 = dist2 * dist2;
			dist2 = 1 - dist2 * _Darkness;
			dist2 *= 2;
			float visibility = saturate(dist2);
            float lightContribution = base.a;
            float4 lighting = tex2D(_LightingTex, IN.uv) * 0.75;
            // Lighting.a is available for additional effects

            float4 col = base * lighting * _Brightness;

			float monoAmount = _MonochromeAmount;
            float mono = col.r * _MonochromeFactorR + col.g * _MonochromeFactorG + col.b * _MonochromeFactorB;
            float4 monoDisplay = float4(mono * _MonochromeDisplayR, mono * _MonochromeDisplayG, mono * _MonochromeDisplayB, 1.0);
            float4 albedoAndLight = monoDisplay * monoAmount + col * (1.0 - monoAmount);
            return (lightContribution * albedoAndLight + (1 - lightContribution) * base) * visibility;
        }

            ENDCG
        }
        }
}
