
Shader "Teleport/Man" {
    Properties {
        _MainColor ("Main Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _MainExponent ("Main Fresnel", Range(0, 5)) = 1.160751

        _OutlineColor ("Outline Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _OutlineExponent ("Outline Fresnel", Range(0, 5)) = 1.160751

        _OverdrawColor ("Overdraw Color", Color) = (0.07843138,0.3921569,0.7843137,1)
		_Overdraw ("Overdraw Strength", Range(0, 1)) = 0.5
    }

	CGINCLUDE
	#include "UnityCG.cginc"
    #pragma multi_compile_fwdbase
    #pragma only_renderers d3d9 d3d11 glcore gles 
    #pragma target 2.0
    #pragma vertex vert

	struct VertexInput {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};
	struct VertexOutput {
		float4 pos : SV_POSITION;
		float4 posWorld : TEXCOORD1;
		float3 normalDir : TEXCOORD2;
	};
	
    VertexOutput vert (VertexInput v) {
        VertexOutput o = (VertexOutput)0;
        o.normalDir = UnityObjectToWorldNormal(v.normal);
        o.posWorld = mul(unity_ObjectToWorld, v.vertex);
        o.pos = UnityObjectToClipPos( v.vertex );
        return o;
    }

    float4 fragment(VertexOutput i, float3 colour, float alpha, float emissionStrength, float exponent) : COLOR {
        i.normalDir = normalize(i.normalDir);
        float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
        float3 emissive = (colour * pow(1.0 - max(0, dot(i.normalDir, viewDirection)), exponent));
        return fixed4(emissive * emissionStrength, alpha);
    }

	ENDCG

    SubShader {
		Blend One One
		ZWrite Off
		Offset -10, 1

        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            CGPROGRAM
            #pragma fragment frag

            uniform float4 _MainColor;
			uniform float _MainExponent;

            float4 frag(VertexOutput i) : COLOR {
				return fragment(i, _MainColor.rgb, _MainColor.a, 1, _MainExponent);
            }
            ENDCG
        }

        Pass {
            CGPROGRAM
            #pragma fragment frag

            uniform float4 _OutlineColor;
            uniform float _OutlineExponent;

            float4 frag(VertexOutput i) : COLOR {
				return fragment(i, _OutlineColor.rgb, _OutlineColor.a, 1, _OutlineExponent);
            }
            ENDCG
        }

        Pass {
			ZTest Off
            
            CGPROGRAM
            #pragma fragment frag

            uniform float4 _OverdrawColor;
            uniform float _MainExponent;
			uniform float _Overdraw;

            float4 frag(VertexOutput i) : COLOR {
				return fragment(i, _OverdrawColor.rgb, _OverdrawColor.a, _Overdraw, _MainExponent);
            }
            ENDCG
        }
    }
    
}
