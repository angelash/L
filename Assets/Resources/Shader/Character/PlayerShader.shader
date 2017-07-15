Shader "Mogo/PlayerShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Layer1Tex("Layer1",2D) = "white"{}
		_Color ("Main Color", Color) = (1,1,1,1)
		_CtrlColor("CtrlColor",Color) = (1,1,1,1)
		//_HitColor("Hit Color",Color) = (0,0,0,0)
		_BRDFTex ("NdotL NdotH (RGBA)", 2D) = "white" {}
		_HighLight("High Light",Float) = 1
	}
	
	SubShader {
		LOD 200
		Tags
		{
			"Queue" = "Geometry+110"
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
		}
		

		 Pass
		 {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _Color;
			//fixed4 _HitColor;

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
				half4 result = tex2D(_MainTex,i.uv);
				return result;
			}
			ENDCG
		 }
	} 

	
}
