Shader "EchoProtocol/Atmosphere"
{
    Properties { _MainTex ("Scene", 2D) = "white" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize, _Accent;
            float _Rewind, _Clock;
            fixed4 frag(v2f_img i) : SV_Target
            {
                float3 color = tex2D(_MainTex, i.uv).rgb;
                float2 d1 = _MainTex_TexelSize.xy * 2.0;
                float2 d2 = _MainTex_TexelSize.xy * 4.5;
                float3 glow1 = tex2D(_MainTex, i.uv + d1).rgb + tex2D(_MainTex, i.uv - d1).rgb
                             + tex2D(_MainTex, i.uv + float2(d1.x,-d1.y)).rgb + tex2D(_MainTex, i.uv + float2(-d1.x,d1.y)).rgb;
                float3 glow2 = tex2D(_MainTex, i.uv + d2).rgb + tex2D(_MainTex, i.uv - d2).rgb
                             + tex2D(_MainTex, i.uv + float2(d2.x,-d2.y)).rgb + tex2D(_MainTex, i.uv + float2(-d2.x,d2.y)).rgb;
                color += max(glow1 * 0.25 - 0.58, 0.0) * 0.16;
                color += max(glow2 * 0.25 - 0.68, 0.0) * 0.08;
                float luminance = dot(color, float3(0.2126, 0.7152, 0.0722));
                color = lerp(luminance.xxx, color, 1.08);
                color *= lerp(float3(0.93, 1.01, 1.07), float3(1.05, 1.02, 0.99), saturate(luminance));
                float2 edge = (i.uv - 0.5) * 1.35;
                color *= 1.0 - dot(edge, edge) * 0.18;
                float grain = frac(sin(dot(i.uv * _MainTex_TexelSize.zw, float2(12.9898, 78.233)) + floor(_Clock * 24)) * 43758.5453) - 0.5;
                color += grain * 0.004;
                float rewindEdge = smoothstep(0.35, 0.72, length(i.uv - 0.5));
                color += _Accent.rgb * saturate(_Rewind) * 0.14 * rewindEdge;
                return fixed4(max(color, 0.0), 1.0);
            }
            ENDCG
        }
    }
}
