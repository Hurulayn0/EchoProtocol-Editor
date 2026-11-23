Shader "EchoProtocol/Effects"
{
    Properties { _Color ("Tint", Color) = (1,1,1,1) _Softness ("Soft particles", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            fixed4 _Color; float _Softness;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.color=v.color*_Color; o.uv=v.uv; return o; }
            fixed4 frag(v2f i) : SV_Target { i.color.a *= lerp(1,saturate(1-dot(i.uv-.5,i.uv-.5)*4),_Softness); return i.color; }
            ENDCG
        }
    }
}
