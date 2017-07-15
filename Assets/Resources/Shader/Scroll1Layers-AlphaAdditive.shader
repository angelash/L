
Shader "MOGO2/Environment/Additive/Scroll1Layers-AlphaAdditive" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_ScrollX ("Base layer Scroll speed X", Float) = 1.0
	_ScrollY ("Base layer Scroll speed Y", Float) = 0.0
	
	_Color("Color", Color) = (1,1,1,1)
	
	_MMultiplier ("Layer Multiplier", Float) = 2.0
}

	
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
	LOD 300

	Blend SrcAlpha One
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
	
	CGINCLUDE
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	#pragma exclude_renderers molehill    
	#include "UnityCG.cginc"

	sampler2D _MainTex;

	float4 _MainTex_ST;
	
	float _ScrollX;
	float _ScrollY;

	float _MMultiplier;
	
	float4 _Color;

	struct v2f {
		float4 pos		: SV_POSITION;
		float4 uv		: TEXCOORD0;
		fixed4 color	: TEXCOORD1;
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.uv.xy = TRANSFORM_TEX(v.texcoord.xy,_MainTex) + (float2(_ScrollX, _ScrollY) * _Time);
		
		o.color = _MMultiplier * _Color * v.color;

		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		fixed4 frag (v2f i) : COLOR
		{
			fixed4 o;

			fixed4 tex = tex2D (_MainTex, i.uv.xy);
			
			o = tex * i.color;
						
			return o;
		}
		ENDCG 
	}	
}
}
