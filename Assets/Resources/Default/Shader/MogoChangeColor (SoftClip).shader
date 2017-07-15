Shader "Mogo/ChangeColor (SoftClip)" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_ColorCtrl("Color Ctrl",float) = 0.5
	}
	SubShader {
		Tags { "Queue" = "Transparent+10" }
		LOD 200
		 Pass
		 {
			ZWrite On
			ZTest Off
			Blend SrcAlpha OneMinusSrcAlpha
			Lighting Off
			//Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _Color;
			float _ColorCtrl;

			struct vin {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 color : COLOR0;

			};

			struct v2f 
			{
				fixed4  pos : SV_POSITION;
				fixed2  uv : TEXCOORD0;
				fixed2 worldPos : TEXCOORD1;
				fixed4  color : COLOR0;
			};

			fixed4 _MainTex_ST;
			fixed4 _ClipRange = fixed4(0.0, 0.0, 1.0, 1.0);
			fixed2 _ClipArgs = fixed2(1000.0, 1000.0);

			v2f vert (vin v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				o.color = v.color;

				o.worldPos = v.vertex.xy * _ClipRange.zw + _ClipRange.xy;
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 texcol = tex2D (_MainTex, i.uv);
				fixed4 result = fixed4(1,1,1,1);
				//result.rgb = pow(result.rgb, 0.5);
				
				if(texcol.r <= _ColorCtrl)
				{
					result = 2*texcol * _Color;
				}
				else
				{
					result = 1 - 2 * ( fixed4(1,1,1,1) - texcol) * (fixed4(1,1,1,1) - _Color);
				}
				

				result.a = texcol.a;

				fixed2 factor = (fixed2(1.0, 1.0) - abs(i.worldPos)) * _ClipArgs;
				result.a *= clamp( min(factor.x, factor.y), 0.0, 1.0);

				result.a *= i.color.a;

				return result;
			}
			ENDCG
		 }
	} 
}
