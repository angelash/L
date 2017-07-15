
Shader "MOGO2/CharacterLow"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "grey" {}
		_LightDirX("_LightDirX", Range(-1, 1)) = 1
		_LightDirY("_LightDirY", Range(-1, 1)) = 0.3
		_LightDirZ("_LightDirZ", Range(-1, 1)) = 1

		_MainColor("_MainColor", Color) = (1, 1, 0.8, 1)
		_MainColorStrength("_MainColorStrength", float) = 1.2
		_BackColor("_BackColor", Color) = (0.3, 0.3, 0.4, 1)

		_FinalPower("FinalPower",Float) = 1
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+50"}
		LOD 200
		Lighting Off
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			half _LightDirX;
			half _LightDirY;
			half _LightDirZ;

			half3 _MainColor;
			half _MainColorStrength;
			half3 _BackColor;

			half _FinalPower;

			struct v2f
			{
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half3 dfs : COLOR0;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;

				half3 n = normalize(mul((half3x3)_Object2World, v.normal.xyz));
				half3 l = normalize(half3(_LightDirX, _LightDirY, _LightDirZ));
				o.dfs = lerp(_BackColor, _MainColor * _MainColorStrength, (dot(n, l) + 1) * 0.5);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				half4 clr = tex2D(_MainTex, i.uv);
				clr.rgb *= i.dfs * _FinalPower;
				return clr;
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}
