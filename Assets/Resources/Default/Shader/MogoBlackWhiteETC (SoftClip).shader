Shader "Mogo/BlackWhiteETC (SoftClip)" 
{
    Properties 
	{
		 _MainTex ("Base (RGB)", 2D) = "white" { }
		 _AlphaTex("AlphaTex",2D) = "white"{}
    }
    SubShader
	{

		 Tags
		 {
			"Queue" = "Transparent+8"
		 }
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
		
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
		
			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float2 worldPos : TEXCOORD1;
			};

			float4 _ClipRange = float4(0.0, 0.0, 1.0, 1.0);
			float2 _ClipArgs = float2(1000.0, 1000.0);

			v2f vert (appdata_t v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				o.worldPos = v.vertex.xy * _ClipRange.zw + _ClipRange.xy;
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				float2 factor = (float2(1.0, 1.0) - abs(i.worldPos)) * _ClipArgs;
				half gray = dot(tex2D (_MainTex, i.uv).rgb, half3(0.299, 0.587, 0.114));
				half alpha = tex2D(_AlphaTex,i.uv);
				return half4(gray, gray, gray, alpha * clamp( min(factor.x, factor.y), 0.0, 1.0));
			}
			ENDCG
		 }
    }
} 