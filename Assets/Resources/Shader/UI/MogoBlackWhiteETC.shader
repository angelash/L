Shader "Mogo/BlackWhiteETC" 
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
		
			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half gray = dot(tex2D (_MainTex, i.uv).rgb, half3(0.299, 0.587, 0.114));
				half alpha = tex2D(_AlphaTex,i.uv);
				return half4(gray, gray, gray, alpha);
			}
			ENDCG
		 }
    }
} 