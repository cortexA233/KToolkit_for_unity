Shader "SDF/FirstSDF"
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
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;


            fixed4 SDFBox(fixed3 coord, fixed3 size){
                fixed2 sdf = length(max(abs(coord) - size, 0));
                // sdf = -sdf;
                // int finalSDF = sdf.x + sdf.y;
                int finalSDF = (max(sdf, 0.0) - min(max(sdf.x, sdf.y), 0)) * 111;
                // fixed2 sdf = sign(size/2 - abs(coord));
                return saturate(fixed4(finalSDF, finalSDF, finalSDF, 0));
            }
            
            fixed4 SDFCircle(fixed2 coord, fixed radius, fixed2 center = fixed2(0.5, 0.5)){
                coord = coord - center;
                float sdf = -(length(coord) - radius);
                int finalSDF = int(sdf * 10000007);
                // float finalSDF = sdf * 1;
                fixed4 color = fixed4(1, 1, 1, 1) * finalSDF;
                return saturate(color);
            }
            
            fixed4 SDFRect(fixed2 coord, fixed2 size, fixed2 center = fixed2(0.5, 0.5), bool need_clamp=true){
                coord = coord - center;
                fixed2 sdf = abs(coord) - size;
                fixed finalSDF = -(length(max(sdf, 0)) + min(max(sdf.x, sdf.y), 0));
                if (need_clamp)
                {
                    finalSDF *= 10000007;
                    fixed4 color = fixed4(1, 1, 1, 1) * finalSDF;
                    return saturate(color);
                }
                return fixed4(1, 1, 1, 1) * finalSDF;
            }

            fixed4 SDFLineSegment(fixed2 coord, fixed2 p0, fixed2 p1, fixed width, bool need_clamp=true)
            {
                fixed2 dir = p1 - p0;
                fixed2 diff = coord - p0;
                float len = length(dir);
                dir /= len;
                float projection = dot(diff, dir);
                float sdf;
                if (projection < 0.0)
                {
                    // 如果投影小于0，说明点在线段的起点之前，返回该点到起点的距离
                    sdf = length(coord - p0);
                }
                else if (projection > len)
                {
                    // 如果投影大于线段长度，说明点在线段的终点之后，返回该点到终点的距离
                    sdf = length(coord - p1);
                }
                else
                {
                    // 否则返回点到线段的最短距离
                    sdf = abs(cross(fixed3(dir, 0), fixed3(diff, 0)).z);
                }
                sdf = -sdf;
                sdf += width;
                
                if (need_clamp)
                {
                    int finalSDF = int(sdf * 10000007);
                    fixed4 color = fixed4(1, 1, 1, 1) * finalSDF;
                    return saturate(color);
                }
                return fixed4(1, 1, 1, 1) * sdf;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                fixed2 eyesLeft = fixed2(0.33, 0.66);
                fixed2 eyesRight = fixed2(0.66, 0.66);

                col = SDFCircle(i.uv, 0.45);
                // col -= SDFCircle(i.uv, 0.07, eyesLeft);
                // col -= SDFCircle(i.uv, 0.07, eyesRight);

                // lumen & shadow
                col += SDFRect(i.uv, fixed2(0.08, 0.1), fixed2(0.5, 0.45), false);
                col -= SDFLineSegment(i.uv, fixed2(0.8, 1.3), fixed2(1.3, 0.8), 1, false);
                
                col -= SDFRect(i.uv, fixed2(0.08, 0.1), fixed2(0.5, 0.45));
                col += SDFRect(i.uv, fixed2(0.04, 0.1), fixed2(0.5, 0.45));

                // K eyes
                col -= SDFLineSegment(i.uv, fixed2(0.26, 0.66), fixed2(0.4, 0.66), 0.01);
                col -= SDFLineSegment(i.uv, fixed2(0.26, 0.7), eyesLeft, 0.01);
                col -= SDFLineSegment(i.uv, eyesLeft, fixed2(0.4, 0.7), 0.01);
                
                col -= SDFLineSegment(i.uv, fixed2(0.59, 0.66), fixed2(0.73, 0.66), 0.01);
                col -= SDFLineSegment(i.uv, fixed2(0.59, 0.7), eyesRight, 0.01);
                col -= SDFLineSegment(i.uv, eyesRight, fixed2(0.73, 0.7), 0.01);
                
                // K mouth
                col -= SDFLineSegment(i.uv, fixed2(0.3, 0.25), fixed2(0.7, 0.25), 0.02);
                col -= SDFLineSegment(i.uv, fixed2(0.3, 0.15), fixed2(0.5, 0.25), 0.02);
                col -= SDFLineSegment(i.uv, fixed2(0.5, 0.25), fixed2(0.7, 0.15), 0.02);

                return col;
            }
            ENDCG
        }
    }
}
