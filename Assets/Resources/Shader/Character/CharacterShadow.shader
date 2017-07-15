
Shader "MOGO2/CharacterShadow"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "grey" {}
		_Gloss("Gloss(R)", 2D) = "while" {}
		
		_LightDirX("_LightDirX", Range(-1, 1)) = 1
		_LightDirY("_LightDirY", Range(-1, 1)) = 0.3
		_LightDirZ("_LightDirZ", Range(-1, 1)) = 1

		_MainColor("_MainColor", Color) = (1, 1, 0.8, 1)
		_MainColorStrength("_MainColorStrength", float) = 1.2
		_BackColor("_BackColor", Color) = (0.3, 0.3, 0.4, 1)

		_SpecularColor("_SpecularColor", Color) = (0, 1, 1, 1)
		_SpecularStrength("_SpecularStrength", float) = 2
		_Shininess("_Shininess", Float) = 2

		_RimPower ("_RimPower", Color) = (0.2, 0.4, 1.0, 0.0)
		_RimColor ("_RimColor", Color) = (1.0, 1.0, 0.0, 1.0)
		_RimStrength("_RimStrength", Range(0.0, 4.0)) = 1

		_FinalPower("FinalPower",Float) = 1
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+50"}
		LOD 200
		Lighting Off
		
		Pass 
		{
			Name "Overlay"
			zwrite off
			ztest greater
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv  : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform fixed4 _OverlayColor;

			v2f vert(appdata_base v) 
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o;
			}
			fixed4 frag(v2f i) : Color {
				return tex2D(_MainTex, i.uv) * _OverlayColor;
			}

			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Gloss;
			
			half _LightDirX;
			half _LightDirY;
			half _LightDirZ;

			half3 _MainColor;
			half _MainColorStrength;
			half3 _BackColor;

			half3 _SpecularColor;
			half _SpecularStrength;
			half _Shininess;

			fixed3 _RimColor;
			fixed _RimStrength;
			fixed3 _RimPower;

			half _FinalPower;

			struct v2f
			{
				float4 pos : POSITION;
				half3 dfs : COLOR0;
				half spc : COLOR1;
				half2 uv : TEXCOORD0;
				half3 view : TEXCOORD1;
				half3 normal : TEXCOORD2;
			};

			v2f vert(appdata_full v)
			{
				v2f o;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;

				half3 n = normalize(mul((half3x3)_Object2World, v.normal.xyz));
				half3 e = normalize(_WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz);
				half3 l = normalize(half3(_LightDirX, _LightDirY, _LightDirZ));
				o.dfs = lerp(_BackColor, _MainColor * _MainColorStrength, (dot(n, l) + 1) * 0.5);
				o.spc = pow(max(dot(normalize(l + e), n), 0), _Shininess * 64) * _SpecularStrength;
				o.view = e;
				o.normal = n;

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				half4 clr = tex2D(_MainTex, i.uv);
				half4 gls = tex2D(_Gloss, i.uv);

				fixed vdn = saturate(dot(i.view, i.normal));
				fixed3 facing = (1.0 - vdn);
				facing.gb *= facing.gb;
				facing.b *= facing.b;
				fixed rim = dot(facing, _RimPower);

				clr.rgb *= i.dfs + gls.r * i.spc * _SpecularColor;
				clr.rgb	= lerp(clr.rgb, _RimColor, saturate(rim * _RimStrength));
				clr.rgb *= _FinalPower;

				return clr;
			}

			ENDCG
		}

	}

	FallBack "Diffuse"
}
