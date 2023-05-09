// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:33040,y:32679,varname:node_2865,prsc:2|diff-9211-RGB,spec-617-R,normal-6380-RGB,emission-9380-OUT;n:type:ShaderForge.SFN_Tex2d,id:9211,x:32600,y:32041,ptovrint:False,ptlb:Albedo,ptin:_Albedo,varname:node_9211,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:6640305344bd4c34c9a3bb30fa6ab3bd,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:6380,x:32600,y:32392,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:node_6380,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ffd2b06de742a384380dc68d7d6d589b,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Tex2d,id:617,x:32600,y:32218,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:node_617,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:98e1aa4f47bb2f048a471f4b403127e4,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:1940,x:31989,y:32393,ptovrint:False,ptlb:Emission,ptin:_Emission,varname:node_1940,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:6151a0a80cb14fe48b345124cad3e439,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Time,id:9670,x:31060,y:31628,varname:node_9670,prsc:2;n:type:ShaderForge.SFN_FragmentPosition,id:3249,x:30721,y:31883,varname:node_3249,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:6928,x:31743,y:31794,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:node_6928,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-9660-OUT;n:type:ShaderForge.SFN_Set,id:9788,x:31910,y:31815,varname:SampledNoise,prsc:2|IN-6928-R;n:type:ShaderForge.SFN_Get,id:2630,x:31399,y:32776,varname:node_2630,prsc:2|IN-9788-OUT;n:type:ShaderForge.SFN_Lerp,id:5556,x:31675,y:32614,varname:node_5556,prsc:2|A-9729-RGB,B-6537-RGB,T-2630-OUT;n:type:ShaderForge.SFN_Color,id:9729,x:31420,y:32459,ptovrint:False,ptlb:ColorA,ptin:_ColorA,varname:node_9729,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:1,c3:0.9586205,c4:1;n:type:ShaderForge.SFN_Color,id:6537,x:31420,y:32633,ptovrint:False,ptlb:ColorB,ptin:_ColorB,varname:_ColorA_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0.04827595,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:1348,x:31460,y:32124,ptovrint:False,ptlb:Mask,ptin:_Mask,varname:node_1348,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:d90a45bab94880146b2707a89c20aac9,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:1539,x:31871,y:32614,varname:node_1539,prsc:2|A-4523-OUT,B-5556-OUT;n:type:ShaderForge.SFN_Add,id:1469,x:30910,y:31851,varname:node_1469,prsc:2|A-3249-X,B-3249-Y;n:type:ShaderForge.SFN_Add,id:2866,x:30910,y:31977,varname:node_2866,prsc:2|A-3249-Y,B-3249-Z;n:type:ShaderForge.SFN_Append,id:745,x:31086,y:31907,varname:node_745,prsc:2|A-1469-OUT,B-2866-OUT;n:type:ShaderForge.SFN_Add,id:9660,x:31529,y:31811,varname:node_9660,prsc:2|A-7655-OUT,B-113-OUT;n:type:ShaderForge.SFN_Slider,id:4719,x:30910,y:32137,ptovrint:False,ptlb:Scale,ptin:_Scale,varname:node_4719,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:2.806303,max:40;n:type:ShaderForge.SFN_Multiply,id:113,x:31249,y:31896,varname:node_113,prsc:2|A-745-OUT,B-4719-OUT;n:type:ShaderForge.SFN_Multiply,id:7655,x:31276,y:31724,varname:node_7655,prsc:2|A-9670-TSL,B-3833-OUT;n:type:ShaderForge.SFN_Slider,id:3833,x:30903,y:31774,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:node_3833,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:20;n:type:ShaderForge.SFN_Slider,id:7954,x:31714,y:32770,ptovrint:False,ptlb:NoiseEmission,ptin:_NoiseEmission,varname:node_7954,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.9316448,max:4;n:type:ShaderForge.SFN_Multiply,id:6159,x:32084,y:32700,varname:node_6159,prsc:2|A-1539-OUT,B-7954-OUT;n:type:ShaderForge.SFN_Set,id:1933,x:31636,y:32124,varname:NoiseMask,prsc:2|IN-1348-R;n:type:ShaderForge.SFN_Get,id:4523,x:31654,y:32553,varname:node_4523,prsc:2|IN-1933-OUT;n:type:ShaderForge.SFN_Set,id:3232,x:31636,y:32177,varname:HandMask,prsc:2|IN-1348-G;n:type:ShaderForge.SFN_Set,id:4579,x:31636,y:32231,varname:DotsMask,prsc:2|IN-1348-B;n:type:ShaderForge.SFN_Multiply,id:1029,x:32188,y:32476,varname:node_1029,prsc:2|A-1940-RGB,B-7573-OUT,C-1597-OUT;n:type:ShaderForge.SFN_Get,id:7573,x:31968,y:32549,varname:node_7573,prsc:2|IN-3232-OUT;n:type:ShaderForge.SFN_Add,id:9380,x:32384,y:32606,varname:node_9380,prsc:2|A-1029-OUT,B-6159-OUT,C-8479-OUT;n:type:ShaderForge.SFN_Get,id:4845,x:32049,y:33032,varname:node_4845,prsc:2|IN-4579-OUT;n:type:ShaderForge.SFN_Color,id:1162,x:32084,y:32893,ptovrint:False,ptlb:DotsColor,ptin:_DotsColor,varname:node_1162,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:8479,x:32311,y:32933,varname:node_8479,prsc:2|A-1162-RGB,B-4845-OUT,C-9446-OUT;n:type:ShaderForge.SFN_Time,id:8609,x:31721,y:32003,varname:node_8609,prsc:2;n:type:ShaderForge.SFN_Sin,id:4730,x:31918,y:32087,varname:node_4730,prsc:2|IN-8609-T;n:type:ShaderForge.SFN_RemapRange,id:3221,x:32081,y:32087,varname:node_3221,prsc:2,frmn:-1,frmx:1,tomn:0.4,tomx:1|IN-4730-OUT;n:type:ShaderForge.SFN_Set,id:5863,x:32280,y:32087,varname:PulseTimer,prsc:2|IN-3221-OUT;n:type:ShaderForge.SFN_Get,id:1597,x:32008,y:32597,varname:node_1597,prsc:2|IN-5863-OUT;n:type:ShaderForge.SFN_Get,id:9446,x:32049,y:33088,varname:node_9446,prsc:2|IN-5863-OUT;proporder:9729-6537-1162-9211-617-6380-1348-1940-6928-7954-4719-3833;pass:END;sub:END;*/

Shader "Shader Forge/HandShader" {
    Properties {
        _ColorA ("ColorA", Color) = (0,1,0.9586205,1)
        _ColorB ("ColorB", Color) = (0,0.04827595,1,1)
        _DotsColor ("DotsColor", Color) = (0.5,0.5,0.5,1)
        _Albedo ("Albedo", 2D) = "white" {}
        _Metallic ("Metallic", 2D) = "black" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Mask ("Mask", 2D) = "white" {}
        _Emission ("Emission", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
        _NoiseEmission ("NoiseEmission", Range(0, 4)) = 0.9316448
        _Scale ("Scale", Range(1, 40)) = 2.806303
        _Speed ("Speed", Range(1, 20)) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _Albedo; uniform float4 _Albedo_ST;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Metallic; uniform float4 _Metallic_ST;
            uniform sampler2D _Emission; uniform float4 _Emission_ST;
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float4 _ColorA;
            uniform float4 _ColorB;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform float _Scale;
            uniform float _Speed;
            uniform float _NoiseEmission;
            uniform float4 _DotsColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 normalLocal = _Normal_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = 0.5;
                float perceptualRoughness = 1.0 - 0.5;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float4 _Metallic_var = tex2D(_Metallic,TRANSFORM_TEX(i.uv0, _Metallic));
                float3 specularColor = _Metallic_var.r;
                float specularMonochrome;
                float4 _Albedo_var = tex2D(_Albedo,TRANSFORM_TEX(i.uv0, _Albedo));
                float3 diffuseColor = _Albedo_var.rgb; // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float4 _Emission_var = tex2D(_Emission,TRANSFORM_TEX(i.uv0, _Emission));
                float4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(i.uv0, _Mask));
                float HandMask = _Mask_var.g;
                float4 node_8609 = _Time;
                float PulseTimer = (sin(node_8609.g)*0.3+0.7);
                float NoiseMask = _Mask_var.r;
                float4 node_9670 = _Time;
                float2 node_9660 = ((node_9670.r*_Speed)+(float2((i.posWorld.r+i.posWorld.g),(i.posWorld.g+i.posWorld.b))*_Scale));
                float4 _Noise_var = tex2D(_Noise,TRANSFORM_TEX(node_9660, _Noise));
                float SampledNoise = _Noise_var.r;
                float DotsMask = _Mask_var.b;
                float3 emissive = ((_Emission_var.rgb*HandMask*PulseTimer)+((NoiseMask*lerp(_ColorA.rgb,_ColorB.rgb,SampledNoise))*_NoiseEmission)+(_DotsColor.rgb*DotsMask*PulseTimer));
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _Albedo; uniform float4 _Albedo_ST;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Metallic; uniform float4 _Metallic_ST;
            uniform sampler2D _Emission; uniform float4 _Emission_ST;
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float4 _ColorA;
            uniform float4 _ColorB;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform float _Scale;
            uniform float _Speed;
            uniform float _NoiseEmission;
            uniform float4 _DotsColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 normalLocal = _Normal_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
				UNITY_LIGHT_ATTENUATION(attenuation, i, i.posWorld.xyz);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = 0.5;
                float perceptualRoughness = 1.0 - 0.5;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float4 _Metallic_var = tex2D(_Metallic,TRANSFORM_TEX(i.uv0, _Metallic));
                float3 specularColor = _Metallic_var.r;
                float specularMonochrome;
                float4 _Albedo_var = tex2D(_Albedo,TRANSFORM_TEX(i.uv0, _Albedo));
                float3 diffuseColor = _Albedo_var.rgb; // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _Albedo; uniform float4 _Albedo_ST;
            uniform sampler2D _Metallic; uniform float4 _Metallic_ST;
            uniform sampler2D _Emission; uniform float4 _Emission_ST;
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float4 _ColorA;
            uniform float4 _ColorB;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform float _Scale;
            uniform float _Speed;
            uniform float _NoiseEmission;
            uniform float4 _DotsColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float4 _Emission_var = tex2D(_Emission,TRANSFORM_TEX(i.uv0, _Emission));
                float4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(i.uv0, _Mask));
                float HandMask = _Mask_var.g;
                float4 node_8609 = _Time;
                float PulseTimer = (sin(node_8609.g)*0.3+0.7);
                float NoiseMask = _Mask_var.r;
                float4 node_9670 = _Time;
                float2 node_9660 = ((node_9670.r*_Speed)+(float2((i.posWorld.r+i.posWorld.g),(i.posWorld.g+i.posWorld.b))*_Scale));
                float4 _Noise_var = tex2D(_Noise,TRANSFORM_TEX(node_9660, _Noise));
                float SampledNoise = _Noise_var.r;
                float DotsMask = _Mask_var.b;
                o.Emission = ((_Emission_var.rgb*HandMask*PulseTimer)+((NoiseMask*lerp(_ColorA.rgb,_ColorB.rgb,SampledNoise))*_NoiseEmission)+(_DotsColor.rgb*DotsMask*PulseTimer));
                
                float4 _Albedo_var = tex2D(_Albedo,TRANSFORM_TEX(i.uv0, _Albedo));
                float3 diffColor = _Albedo_var.rgb;
                float specularMonochrome;
                float3 specColor;
                float4 _Metallic_var = tex2D(_Metallic,TRANSFORM_TEX(i.uv0, _Metallic));
                diffColor = DiffuseAndSpecularFromMetallic( diffColor, _Metallic_var.r, specColor, specularMonochrome );
                o.Albedo = diffColor + specColor * 0.125; // No gloss connected. Assume it's 0.5
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    
}
