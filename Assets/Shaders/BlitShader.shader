Shader "Hidden/BlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        //Blend OneMinusDstAlpha DstAlpha
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy;// v.uv;

#ifdef FLIP_VERTICAL
                o.uv.y = 1 - o.uv.y;
#endif
                return o;
            }

            sampler2D _MainTex;
            float4 _Filter;

            fixed4 frag (v2f input) : SV_Target
            {
                //return float4(0,1,0,1);
                //return float4(input.uv, 0, 1) /* _Filter*/;
                //return _Filter;
                return tex2D(_MainTex, input.uv) *_Filter;
            }
            ENDCG
        }
    }
}
