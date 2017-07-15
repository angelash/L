Shader "Mogo/UIETC2 (SoftClip)" 
{
    Properties 
	{
		 _MainTex ("Base (RGB)", 2D) = "white" { }
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
			Blend SrcAlpha OneMinusSrcAlpha, One One 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;

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
				fixed2 uv : TEXCOORD0;
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

			float4 frag (v2f i) : COLOR
			{
				float2 factor = (float2(1.0, 1.0) - abs(i.worldPos)) * _ClipArgs;

				float a = clamp( min(factor.x, factor.y), 0.0, 1.0);
				float4 texcol = tex2D (_MainTex, i.uv);// * i.color;
				texcol *= a; 
				//texcol.rgb /= texcol
				//texcol.a *= 

				return texcol;
			}
			ENDCG
		 }
    }
} 