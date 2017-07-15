Shader "Mogo/ChangeColorETC (SoftClip)" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaTex("AlphaTex",2D) = "white"{}
		_Color ("Main Color", Color) = (1,1,1,1)
		_ColorCtrl("Color Ctrl",float) = 0.5
	}
	SubShader {
		Tags { "Queue" = "Transparent+8" }
		LOD 200
		 Pass
		 {
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest Off
			Fog { Mode Off }		
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			fixed4 _Color;
			float _ColorCtrl;

			float4 _ClipRange = float4(0.0, 0.0, 1.0, 1.0);
			float2 _ClipArgs = float2(1000.0, 1000.0);

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				half4 color :COLOR0;
			};
		
			struct v2f {
				float4 pos : POSITION;
				half4 color : COLOR0;
				float2 uv : TEXCOORD0;
				float2 worldPos : TEXCOORD1;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv =  v.texcoord;
				o.color = v.color;
				o.worldPos = v.vertex.xy * _ClipRange.zw + _ClipRange.xy;
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				float2 factor = (float2(1.0, 1.0) - abs(i.worldPos)) * _ClipArgs;
				fixed4 texcol = tex2D (_MainTex, i.uv) * i.color;
				fixed4 result = fixed4(1,1,1,1);

				if(texcol.r <= _ColorCtrl)
				{
					result = 2 * texcol * _Color;
				}
				else
				{
					result = 1 - 2 * ( fixed4(1,1,1,1) - texcol) * (fixed4(1,1,1,1) - _Color);
				}

				result.a = tex2D(_AlphaTex,i.uv).r * i.color.a;
				result.a *= clamp( min(factor.x, factor.y), 0.0, 1.0);

				return result;
			}
			ENDCG
		 }
	} 
}
