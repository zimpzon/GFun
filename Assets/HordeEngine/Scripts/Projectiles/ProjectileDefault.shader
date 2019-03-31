Shader "Engine/ActorProjectileDefault"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Clarity("Clarity", float) = 1.0
    }
        SubShader
    {
        Tags
    {
        "RenderType" = "Transparent"
        "Queue" = "Transparent"
        "PreviewType" = "Plane"
        "CanUseSpriteAtlas" = "True"
    }

        Pass
    {
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

        sampler2D _MainTex;
        float _Clarity;

    struct Vertex
    {
        float4 vertex : POSITION;
        float2 uv_MainTex : TEXCOORD0;
        float4 color : COLOR;
    };

    struct Fragment
    {
        float4 vertex : POSITION;
        float2 uv_MainTex : TEXCOORD0;
        float4 color : COLOR;
    };

    Fragment vert(Vertex v)
    {
        Fragment o;

        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv_MainTex = v.uv_MainTex;
        o.color = v.color;
        o.vertex = UnityPixelSnap(o.vertex);
        return o;
    }

    float4 frag(Fragment IN) : COLOR
    {
        half4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
//        c.a = _Clarity;
		return c;
    }
        ENDCG
    }
    }
}