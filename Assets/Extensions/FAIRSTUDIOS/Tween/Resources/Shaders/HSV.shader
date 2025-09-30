// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "KTween/HSV" {
  Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Color to Shift (RGB) Trans (A)", 2D) = "white" {}

    _Hue ("Hue (0-360)", Float) = 0
    _Saturation ("Saturation", Float) = 1
    _Brightness ("Brightness", Float) = 1
	_AlphaShift ("Alpha", Float) = 1
  }

  SubShader {
    Tags 
    {
      "Queue"="Transparent" 
      "RenderType"="Transparent"
      "IgnoreProjector"="True"
    }

    LOD 200
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    Cull Off
 
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0
      #include "UnityCG.cginc"

      fixed4 _Color;
      sampler2D _MainTex;
 
      fixed _Hue;
      fixed _Saturation;
      fixed _Brightness;
	  fixed _AlphaShift;
      fixed4 _MainTex_ST;

      fixed3 shift_col(fixed3 c, fixed3 shift)
      {
        fixed3 RESULT = float3(c);
        fixed VSU = shift.z*shift.y*cos(shift.x*0.0174532925);  //3.14159265/180
        fixed VSW = shift.z*shift.y*sin(shift.x*0.0174532925);
     
        RESULT.x = (.299*shift.z+.701*VSU+.168*VSW)*c.x
                 + (.587*shift.z-.587*VSU+.330*VSW)*c.y
                 + (.114*shift.z-.114*VSU-.497*VSW)*c.z;
     
        RESULT.y = (.299*shift.z-.299*VSU-.328*VSW)*c.x
                 + (.587*shift.z+.413*VSU+.035*VSW)*c.y
                 + (.114*shift.z-.114*VSU+.292*VSW)*c.z;
     
        RESULT.z = (.299*shift.z-.300*VSU+1.25*VSW)*c.x
                 + (.587*shift.z-.588*VSU-1.05*VSW)*c.y
                 + (.114*shift.z+.886*VSU-.203*VSW)*c.z;
     
        return (RESULT);
      }
    
      struct v2f 
      {
        float4  pos : SV_POSITION;
        float2  uv : TEXCOORD0;
      };

      v2f vert (appdata_base v)
      {
        v2f o;
        o.pos = UnityObjectToClipPos (v.vertex);
        o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
        return o;
      }

      half4 frag(v2f i) : COLOR
      {
        half4 col = tex2D(_MainTex, i.uv);
        fixed3 shift = float3(_Hue, _Saturation, _Brightness);

        return half4( half3(shift_col(col, shift)), col.a * _AlphaShift);
      }

      ENDCG
    }
  }
  Fallback "Transparent/Cutout/VertexLit"
}
