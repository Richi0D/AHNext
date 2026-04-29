Shader "Custom/PointParticle"
{
  Properties
  {
    POINT_COLOR ("Point color", Color) = (1.0,1.0,1.0,1.0)
    POINT_SIZE ("Point size in pixels", Float) = 16.0
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Blend SrcAlpha One
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma geometry geom
      #pragma fragment frag
      #pragma target 4.0   // geometry shaders need SM4+

      fixed4 POINT_COLOR;
      float POINT_SIZE;

      // --- vertex stage: just pass clip-space position through ---
      struct v2g
      {
        float4 pos : SV_POSITION;
      };

      v2g vert(float4 in_pos : POSITION)
      {
        v2g o;
        o.pos = UnityObjectToClipPos(in_pos);
        return o;
      }

      // --- geometry stage: expand each point to a screen-space quad ---
      struct g2f
      {
        float4 pos   : SV_POSITION;
        float2 uv    : TEXCOORD0;   // -1..1 within the quad
      };

      [maxvertexcount(4)]
      void geom(point v2g input[1], inout TriangleStream<g2f> stream)
      {
        float4 center = input[0].pos;

        // pixel size → NDC size  (divide by w first to get NDC, then offset)
        float2 halfSize = (POINT_SIZE * 0.5) / _ScreenParams.xy;

        // emit two triangles (triangle strip = 4 verts)
        const float2 corners[4] = {
          float2(-1, 1), float2( 1, 1),
          float2(-1,-1), float2( 1,-1)
        };

        for (int i = 0; i < 4; i++)
        {
          g2f o;
          // offset in NDC; center is already in clip space so divide by w
          float2 offset = corners[i] * halfSize * center.w;
          o.pos = center + float4(offset, 0, 0);
          o.uv  = corners[i];        // -1..1
          stream.Append(o);
        }
      }

      // --- fragment stage: soft circular disc, same as before ---
      half4 frag(g2f i) : SV_Target
      {
        float dist = length(i.uv);          // 0 at centre, ~1.41 at corner
        float k = 1.0 - saturate(dist);     // linear falloff, clip outside circle
        if (k <= 0) discard;

        half4 output = POINT_COLOR;
        output.w *= k;
        return output;
      }
      ENDCG
    }
  }
}