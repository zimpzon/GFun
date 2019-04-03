Shader "GFun/ClaritySolidShader"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Clarity("Clarity", float) = 1.0
        _Color("Color", Color) = (1,1,1,1)
    }
        SubShader
    {
        Tags
    {
        "RenderType" = "Opaque"
        "Queue" = "Transparent+1"
        "PreviewType" = "Plane"
        "CanUseSpriteAtlas" = "True"
    }

        Pass
    {
        Cull Off
        Blend One Zero

        CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		float4 _MainTex_ST;
        sampler2D _MainTex;
        float _Clarity;
        fixed4 _Color;

    struct Vertex
    {
        float4 vertex : POSITION;
        float2 uv_MainTex : TEXCOORD0;
    };

    struct Fragment
    {
        float4 vertex : POSITION;
        float2 uv_MainTex : TEXCOORD0;
    };

    Fragment vert(Vertex v)
    {
        Fragment o;

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv_MainTex = TRANSFORM_TEX(v.uv_MainTex, _MainTex);
        o.vertex = UnityPixelSnap(o.vertex);
        return o;
    }

    float4 frag(Fragment IN) : COLOR
    {
        half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        c.a = _Clarity;
		return c;
    }
        ENDCG
    }
    }
}
