Shader "Hidden/FractalShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            //#pragma shader_feature MANDELBROT
            //#pragma shader_feature JULIA
            #pragma shader_feature VIZ_MAG
            #pragma shader_feature VIZ_ITER
            #pragma shader_feature MOD_ABS
            #pragma shader_feature ALSO_COLORIZE

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _GradientTex;
            float _GradientFrequency;
            float4x4 _CameraTransform;
            int _MaxIterations;
            float _GradientPull;
            float2 _C;
            float _MandelBrotJuliaBlend;
            float2 _FractalWindowPosition;
            float _FractalWindowSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 fracCoord : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float4 ReadGradient(float u)
            {
                u = 1 - pow(1 - frac(_GradientFrequency * u), _GradientPull);
                return tex2D(_GradientTex, u.xx);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float4 vert = v.vertex;
                vert.xy -= 0.5;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy;// v.uv;
                o.fracCoord.xy = _FractalWindowPosition + v.vertex.xy * _FractalWindowSize; //mul(_CameraTransform, vert);
                o.fracCoord.x *= -1;
                o.fracCoord.zw = 0;
                return o;
            }

            float4 frag (v2f input) : SV_Target
            {
                //return float4(input.uv, 0, 1);
                //return float4(1,1,50,1);
                float2 C = lerp(input.fracCoord.xy, _C, _MandelBrotJuliaBlend);
                float2 N = lerp(_C, input.fracCoord.xy, _MandelBrotJuliaBlend);

                int i = 0;
                while (i < _MaxIterations && dot(N, N) < (1<<16))
                {
#ifdef MOD_ABS
                    N = abs(N);
#endif
                    // N = N^2 + C
                    float2 N2 = N * N;
                    N = float2(N2.x - N2.y, 2 * N.x*N.y) - C;
                    i++;
                }

#ifdef ALSO_COLORIZE
                float u;

#ifdef VIZ_MAG
                u = length(N) / 2;
#else
                u = (float)i;
                if (i < _MaxIterations)
                {
                    float lzn = log(dot(N, N)) / 2;
                    float nu = log(lzn / log(2)) / log(2);
                    u += 1 - (float)nu;
                }
                u /= (float)_MaxIterations;
#endif

                float4 fracColor = ReadGradient(u);
                
                float4 uiColor = tex2D(_MainTex, input.uv);
                return lerp(fracColor, uiColor, uiColor.a);
#else
                return float4(N, i, 1);
#endif
            }
            ENDCG
        }
    }
}
