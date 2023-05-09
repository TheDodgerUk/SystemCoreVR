// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Avatar/Mobile/Diffuse" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

CGPROGRAM
#pragma surface surf Lambert vertex:vert nolightmap noforwardadd

    #pragma target 3.5 // necessary for use of SV_VertexID
    #include "Assets/SystemCoreVR/Oculus/Avatar2/Scripts/ShaderUtils/AvatarCustom.cginc"

sampler2D _MainTex;

struct Input {
    float2 uv_MainTex;
};

void vert(inout OvrDefaultAppdata v) {
  OvrInitializeDefaultAppdataAndPopulateWithVertexData(v);
}

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/Diffuse"
}
