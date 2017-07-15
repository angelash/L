Shader "Mogo/Black (SoftClip)" 
{
    Properties 
	{
		 _MainTex ("Base (RGB)", 2D) = "white" { }
    }
    SubShader
	{
		 Tags
		 {
			"Queue" = "Transparent+10"
		 }
         Pass
		 {
			Lighting Off
			ZTest Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _Color;
		
			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
				float2 worldPos : TEXCOORD1;
			};

			half4 _MainTex_ST;
			
			float4 _ClipRange = float4(0.0, 0.0, 1.0, 1.0);
			float2 _ClipArgs = float2(1000.0, 1000.0);

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);

				o.worldPos = v.vertex.xy * _ClipRange.zw + _ClipRange.xy;
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 texcol = tex2D (_MainTex, i.uv);
				half4 result = half4(0, 0, 0,texcol.a);

				float2 factor = (float2(1.0, 1.0) - abs(i.worldPos)) * _ClipArgs;
				result.a *= clamp( min(factor.x, factor.y), 0.0, 1.0);
				return result;
			}
			ENDCG
		 }
    }
} 