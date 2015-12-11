﻿Shader "RL/UnLitTransparent_ZWriteOff" {
    
    Properties 
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _AlphaMap ("Alpha Map", 2D) = "white" {}
        _Cutoff ("Base Alpha cutoff", Range (0,.9)) = 0.001
        _Offset("Offset", float ) = 0
        _OffsetMin("OffsetMin", float ) = 0

    }
    SubShader 
    {
      Tags { "Queue" = "Transparent+1" }
      Blend SrcAlpha OneMinusSrcAlpha 
      Offset [_Offset],[_OffsetMin]
      ZWrite off 
      Cull Back  

         Pass 
       {  
    	   CGPROGRAM
    	   #pragma vertex vert
    	   #pragma fragment frag
    	   
    	   #include "UnityCG.cginc"
           struct appdata_t 
           {
    	   	   float4 vertex : POSITION;
    	   	   float4 color : COLOR;
    	   	   float2 texcoord : TEXCOORD0;
    	   };
    	   
  		   struct v2f 
  		   {
    	   	   float4 vertex : POSITION;
    	   	   float4 color : COLOR;
    	   	   float2 texcoord : TEXCOORD0;
    	   };
    	   sampler2D _MainTex;
    	   sampler2D _AlphaMap;
    	   float4 _MainTex_ST;
    	   float _Cutoff;
    	   
  		   v2f vert (appdata_t v)
    	   {
    	       v2f o;
    	       o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
    	       o.color = v.color;
    	       o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
    	       return o;
    	   }
    	    
    	   float4 _Color;
    	   half4 frag (v2f i) : COLOR
    	   {
    	   	   half4 col = _Color * tex2D(_MainTex, i.texcoord);
    	   	   col.a = tex2D(_AlphaMap, i.texcoord).a;
    	   	   if ( _Cutoff == 0 )
    	   	   {
    	   	       col.a = 1;
    	   	   }
    	   	   else if ( _Cutoff == 1 )
    	   	   {
    	   	       clip( -1 );
    	   	   }
    	   	   else
    	   	   {
    	   	       clip( col.a - _Cutoff );
    	   	   }
    	   	   col.a *= _Color.a;
    	   	   if ( col.a == 0 )
    	   	   {
    	   	       clip( -1 );
    	   	   }
    	   	   return col;
    	   }
    	   ENDCG
       }
    }
}

