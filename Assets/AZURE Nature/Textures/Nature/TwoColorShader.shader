Shader "Custom/TwoColorShaderWithTransparencyBothSides"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (1,1,1,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _BlendPoint ("Blend Point", Range(0,1)) = 0.7
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off  // Desativa o culling para renderizar ambos os lados

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            float4 _Color1;
            float4 _Color2;
            float _BlendPoint;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 baseColor = tex2D(_BaseMap, uv);

                // Calculate the blending based on the Y coordinate of the UVs
                float blendFactor = step(_BlendPoint, uv.y);
                float4 color = lerp(_Color1, _Color2, blendFactor);

                // Apply alpha transparency
                color.a *= _Alpha;

                return baseColor * color;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Cutout/VertexLit"
}
