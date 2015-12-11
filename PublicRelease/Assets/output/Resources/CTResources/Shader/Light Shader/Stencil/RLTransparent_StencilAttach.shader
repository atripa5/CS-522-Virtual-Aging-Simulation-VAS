﻿// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_LightmapInd', a built-in variable

// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_LightmapInd', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D
// Upgrade NOTE: replaced tex2D unity_LightmapInd with UNITY_SAMPLE_TEX2D_SAMPLER

Shader "RL/Stencil/Transparent_StencilAttach" 
{
Properties 
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _AlphaMap ("Alpha Map", 2D) = "white" {}
        _Cutoff ("Base Alpha cutoff", Range (0,1.0)) = 0.001
        _Offset("Offset", float ) = 0
        _OffsetMin("OffsetMin", float ) = 0
        _MaskID( "Mask ID", float ) = 0
	}
	
	SubShader 
	{
	    //Tags { "Queue" = "Transparent+1" }
	    Tags { "Queue" = "Transparent+1" }
		Offset [_Offset],[_OffsetMin]
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Back   
		Stencil 
        {
            Ref [_MaskID]
            Comp Equal
        }
       Pass 
      {
          Tags { "LIGHTMODE"="Vertex"  }
          ZWrite off
          Lighting On
          SeparateSpecular On
          Material {
          Ambient [_Color]
          Diffuse [_Color]
          Emission [_Emission]
          Specular [_SpecColor]
          Shininess [_Shininess]
          }
          Blend SrcAlpha OneMinusSrcAlpha
          ColorMask RGBA
          SetTexture [_AlphaMap] {combine texture}
          SetTexture [_MainTex] {combine texture, previous}
          SetTexture [_MainTex] { combine Previous  * primary double }
          //  SetTexture [_MainTex] { combine texture * primary double}
      }
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	Alphatest Greater 0 ZWrite off ColorMask RGB
	

// ---- forward rendering base pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }
		Blend SrcAlpha OneMinusSrcAlpha

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_fwdbasealpha
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 98 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
      //#pragma surface surf Lambert alpha
      uniform float _Cutoff;
       float4 _Color;
       sampler2D _MainTex;
       sampler2D _AlphaMap;
      struct Input {
          float4 color : COLOR;
          float2 uv_MainTex;
      };
      float4 LightingCustomDiffuse (SurfaceOutput s, fixed3 lightDir, fixed atten) {
          float difLight = max(0, dot (s.Normal, lightDir));
          float4 col;
          col.rgb = s.Albedo * _LightColor0.rgb * (difLight * atten * 2);
          col.a = s.Alpha;
          return col;
      }
      void surf (Input IN, inout SurfaceOutput o) {
          half4 c = tex2D (_MainTex, IN.uv_MainTex);
          o.Albedo = c.rgb * _Color.rgb;
          o.Alpha = tex2D (_AlphaMap, IN.uv_MainTex).a;  
          if ( _Cutoff == 0 )
    	  {
    	   	  o.Alpha = 1;
    	  }
    	  else if ( _Cutoff == 1 )
    	  {
    	   	  clip( -1 );
    	  }
    	  else
    	  {
    	   	  clip( o.Alpha - _Cutoff ); 
    	  }
          o.Alpha  *= _Color.a;
          if ( o.Alpha == 0 )
          {
              clip( -1 );
          }
      }
      

// vertex-to-fragment interpolation data
#ifdef LIGHTMAP_OFF
struct v2f_surf {
  float4 pos : SV_POSITION;
  float2 pack0 : TEXCOORD0;
  fixed3 normal : TEXCOORD1;
  fixed3 vlight : TEXCOORD2;
  LIGHTING_COORDS(3,4)
};
#endif
#ifndef LIGHTMAP_OFF
struct v2f_surf {
  float4 pos : SV_POSITION;
  float2 pack0 : TEXCOORD0;
  float2 lmap : TEXCOORD1;
  LIGHTING_COORDS(2,3)
};
#endif
#ifndef LIGHTMAP_OFF
// float4 unity_LightmapST;
#endif
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  #ifndef LIGHTMAP_OFF
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif
  float3 worldN = mul((float3x3)_Object2World, SCALED_NORMAL);
  #ifdef LIGHTMAP_OFF
  o.normal = worldN;
  #endif

  // SH/ambient and vertex lights
  #ifdef LIGHTMAP_OFF
  float3 shlight = ShadeSH9 (float4(worldN,1.0));
  o.vlight = shlight;
  #ifdef VERTEXLIGHT_ON
  float3 worldPos = mul(_Object2World, v.vertex).xyz;
  o.vlight += Shade4PointLights (
    unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
    unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
    unity_4LightAtten0, worldPos, worldN );
  #endif // VERTEXLIGHT_ON
  #endif // LIGHTMAP_OFF

  // pass lighting information to pixel shader
  TRANSFER_VERTEX_TO_FRAGMENT(o);
  return o;
}
#ifndef LIGHTMAP_OFF
// sampler2D unity_Lightmap;
#ifndef DIRLIGHTMAP_OFF
// sampler2D unity_LightmapInd;
#endif
#endif

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  #ifdef UNITY_COMPILER_HLSL
  Input surfIN = (Input)0;
  #else
  Input surfIN;
  #endif
  surfIN.uv_MainTex = IN.pack0.xy;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  #ifdef LIGHTMAP_OFF
  o.Normal = IN.normal;
  #endif

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  fixed atten = LIGHT_ATTENUATION(IN);
  fixed4 c = 0;

  // realtime lighting: call lighting function
  #ifdef LIGHTMAP_OFF
  c = LightingCustomDiffuse (o, _WorldSpaceLightPos0.xyz, atten);
  #endif // LIGHTMAP_OFF || DIRLIGHTMAP_OFF
  #ifdef LIGHTMAP_OFF
  c.rgb += o.Albedo * IN.vlight;
  #endif // LIGHTMAP_OFF

  // lightmaps:
  #ifndef LIGHTMAP_OFF
    #ifndef DIRLIGHTMAP_OFF
      // directional lightmaps
      fixed4 lmtex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy);
      fixed4 lmIndTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd,unity_Lightmap, IN.lmap.xy);
      half3 lm = LightingLambert_DirLightmap(o, lmtex, lmIndTex, 0).rgb;
    #else // !DIRLIGHTMAP_OFF
      // single lightmap
      fixed4 lmtex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy);
      fixed3 lm = DecodeLightmap (lmtex);
    #endif // !DIRLIGHTMAP_OFF

    // combine lightmaps with realtime shadows
    #ifdef SHADOWS_SCREEN
      #if (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)) && defined(SHADER_API_MOBILE)
      c.rgb += o.Albedo * min(lm, atten*2);
      #else
      c.rgb += o.Albedo * max(min(lm,(atten*2)*lmtex.rgb), lm*atten);
      #endif
    #else // SHADOWS_SCREEN
      c.rgb += o.Albedo * lm;
    #endif // SHADOWS_SCREEN
  c.a = o.Alpha;
  #endif // LIGHTMAP_OFF

  c.a = o.Alpha;
  return c;
}

ENDCG

}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite off Blend One One Fog { Color (0,0,0,0) }
		Blend SrcAlpha One

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_fwdadd
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#define UNITY_PASS_FORWARDADD
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 98 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
      //#pragma surface surf Lambert alpha
      uniform float _Cutoff;
       float4 _Color;
       sampler2D _MainTex;
       sampler2D _AlphaMap;
      struct Input {
          float4 color : COLOR;
          float2 uv_MainTex;
      };
      float4 LightingCustomDiffuse (SurfaceOutput s, fixed3 lightDir, fixed atten) {
          float difLight = max(0, dot (s.Normal, lightDir));
          float4 col;
          col.rgb = s.Albedo * _LightColor0.rgb * (difLight * atten * 2);
          col.a = s.Alpha;
          return col;
      }
      void surf (Input IN, inout SurfaceOutput o) {
          half4 c = tex2D (_MainTex, IN.uv_MainTex);
          o.Albedo = c.rgb * _Color.rgb;
          o.Alpha = tex2D (_AlphaMap, IN.uv_MainTex).a;  
          if ( _Cutoff == 0 )
    	  {
    	   	  o.Alpha = 1;
    	  }
    	  else if ( _Cutoff == 1 )
    	  {
    	   	  clip( -1 );
    	  }
    	  else
    	  {
    	   	  clip( o.Alpha - _Cutoff ); 
    	  }
          o.Alpha  *= _Color.a;
          if ( o.Alpha == 0 )
          {
              clip( -1 );
          }
      }
      

// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float2 pack0 : TEXCOORD0;
  fixed3 normal : TEXCOORD1;
  half3 lightDir : TEXCOORD2;
  LIGHTING_COORDS(3,4)
};
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.normal = mul((float3x3)_Object2World, SCALED_NORMAL);
  float3 lightDir = WorldSpaceLightDir( v.vertex );
  o.lightDir = lightDir;

  // pass lighting information to pixel shader
  TRANSFER_VERTEX_TO_FRAGMENT(o);
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  #ifdef UNITY_COMPILER_HLSL
  Input surfIN = (Input)0;
  #else
  Input surfIN;
  #endif
  surfIN.uv_MainTex = IN.pack0.xy;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  o.Normal = IN.normal;

  // call surface function
  surf (surfIN, o);
  #ifndef USING_DIRECTIONAL_LIGHT
  fixed3 lightDir = normalize(IN.lightDir);
  #else
  fixed3 lightDir = IN.lightDir;
  #endif
  fixed4 c = LightingCustomDiffuse (o, lightDir, LIGHT_ATTENUATION(IN));
  c.a = o.Alpha;
  return c;
}

ENDCG

}

	// ---- end of surface shader generated code

#LINE 117

   }
   Fallback "Vertex"
}