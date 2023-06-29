Shader "VMail/InwardShader" {
  Properties {
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
  }
  SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200

    Cull Front

    CGPROGRAM
    #pragma surface surf Standard vertex:vert
    void vert(inout appdata_full v) {
      v.normal.xyz = v.normal * -1;
    }

    sampler2D _MainTex;

    struct Input {
      float2 uv_MainTex;
    };

    void surf (Input IN, inout SurfaceOutputStandard o) {
		float2 uv = IN.uv_MainTex;
		uv.x = (1 - uv.x) + 0.5;
		if (uv.x > 1) {
			uv.x = uv.x - 1;
		}
		fixed4 c = tex2D (_MainTex, uv);
		o.Albedo = c.rgb;
    }
    ENDCG
  } 
  FallBack "Diffuse"
}