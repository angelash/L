Shader "Mogo/Font (SoftClip)" {
	Properties {
		_MainTex ("Font Texture", 2D) = "white" {}
	}

	SubShader {

		Tags { "Queue"="Transparent+30" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Lighting Off  ZTest Off ZWrite Off Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				half4 color :COLOR0;
			};

			struct v2f {
				float4 vertex : POSITION;
				half4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
				float2 worldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _ClipRange = float4(0.0, 0.0, 1.0, 1.0);
			float2 _ClipArgs = float2(1000.0, 1000.0);
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				o.worldPos = v.vertex.xy * _ClipRange.zw + _ClipRange.xy;
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				float2 factor = (float2(1.0, 1.0) - abs(i.worldPos)) * _ClipArgs;

				fixed4 col = i.color;
				col.a *= tex2D(_MainTex, i.texcoord).a;

				col.a *= clamp( min(factor.x, factor.y), 0.0, 1.0);
				return col;
			}
			ENDCG 
		}
	} 	
}
