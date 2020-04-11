Shader "Hidden/ColorizerShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UITex("Texture", 2D) = "black" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature VIZ_MAG
            #pragma shader_feature VIZ_VEL
            #pragma shader_feature CONVERT_SRGB
            #pragma shader_feature BGR_SWAP
            #pragma shader_feature FLIP_VERTICAL

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
            sampler2D _GradientTex;
            sampler2D _UITex;

            int _MaxIterations;
            float _GradientFrequency;
            float _GradientPull;

            float4 ReadGradient(float u)
            {
                u = 1 - pow(1 - frac(u), _GradientPull);
                return tex2D(_GradientTex, u.xx);
            }

            fixed4 frag (v2f input) : SV_Target
            {
                float4 fractal = tex2D(_MainTex, input.uv);
                float2 N = fractal.xy;
                int i = length(fractal.zw);
                float2 DD = normalize(fractal.zw);
                //return float4(DD/2+0.5, 0, 1);

                float u;

                if (i < _MaxIterations)
                {
#ifdef VIZ_VEL
                    float lzn = log(dot(N, N)) / 2;
                    float nu = log(lzn / log(2) + .4) / log(2);
                    u = (float)i + 1 - (float)nu;
                    u /= (float)_MaxIterations;
#else
                    u = 1;
#endif
                }
                else
                {
#ifdef VIZ_MAG
                    u = length(N) / 2;
#else
                    u = 1;
#endif
                }

                float4 c = ReadGradient(u);
                c.a = 1;

#ifdef CONVERT_SRGB
                //c.rgb = pow(c.rgb, 2.2);
#endif

#ifdef BGR_SWAP
                c.bgr = c.rgb;
#endif



                return c;
                //float4 fracColor = ReadGradient(u);
                //float4 uiColor = tex2D(_UITex, input.uv);
                //return lerp(fracColor, uiColor, uiColor.a);
            }
            ENDCG
        }
    }
}
