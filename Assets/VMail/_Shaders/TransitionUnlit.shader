Shader "VMail/TransitionUnlit"
{
    Properties
    {
		_TransitionValue("Transition value", Range(0, 1)) = 0
        _MainTex ("Texture", 2D) = "white" {}
		_MainTex2 ("Texture", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			half _TransitionValue;

            sampler2D _MainTex;
            float4 _MainTex_ST;

			sampler2D _MainTex2;
			float4 _MainTex2_ST;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _MainTex2);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 col = lerp(tex2D(_MainTex, i.uv), tex2D(_MainTex2, i.uv2), _TransitionValue);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
	Fallback "Diffuse"
}
