Shader "Unity Shaders Book/Chapter 5/Simple Shader" {
	
	Properties {
		_Color ("Color Tint", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader {
		Pass {
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;

			/*
			float4 vert(float4 v : POSITION) : SV_POSITION {
				return mul(UNITY_MATRIX_MVP, v);
			}
			
			float4 frag() : SV_Target {
				return fixed4(0.0, 1.0, 1.0, 1.0);
			}
			*/
			
			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord: TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed3 color : COLOR0;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert(a2v v) {
				v2f o;
				
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.normal * 0.5 + fixed3(0.5, 0.5, 0.5);
				o.texcoord = v.texcoord;

				return o;
			};

			fixed4 frag(v2f i) : SV_Target {
				fixed3 c = i.color * _Color;
				return fixed4(c, 1.0);
			};
			
			ENDCG
		}
	}
}