// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33066,y:32674,varname:node_3138,prsc:2|emission-2926-OUT,alpha-3474-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32022,y:33122,ptovrint:False,ptlb:StartColor,ptin:_StartColor,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_Tex2d,id:2250,x:32061,y:32791,ptovrint:False,ptlb:Background,ptin:_Background,varname:node_2250,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:f3914337be13f6249b5fb7e57bee59a2,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:6970,x:32270,y:32747,varname:node_6970,prsc:2|A-4922-OUT,B-2250-RGB;n:type:ShaderForge.SFN_Multiply,id:2926,x:32850,y:32686,varname:node_2926,prsc:2|A-4789-OUT,B-6970-OUT,C-8206-OUT,D-3474-OUT;n:type:ShaderForge.SFN_TexCoord,id:5439,x:31741,y:33436,varname:node_5439,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Lerp,id:1772,x:32307,y:33272,varname:node_1772,prsc:2|A-7241-RGB,B-3475-RGB,T-7701-OUT;n:type:ShaderForge.SFN_Color,id:3475,x:32022,y:33289,ptovrint:False,ptlb:EndColor,ptin:_EndColor,varname:_StartColor_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_Set,id:8366,x:32469,y:33272,varname:LineColor,prsc:2|IN-1772-OUT;n:type:ShaderForge.SFN_Get,id:4922,x:32040,y:32719,varname:node_4922,prsc:2|IN-8366-OUT;n:type:ShaderForge.SFN_RemapRange,id:1335,x:31911,y:33436,varname:node_1335,prsc:2,frmn:0,frmx:1,tomn:0,tomx:3|IN-5439-U;n:type:ShaderForge.SFN_Clamp01,id:7701,x:32080,y:33436,varname:node_7701,prsc:2|IN-1335-OUT;n:type:ShaderForge.SFN_Set,id:8999,x:32270,y:32878,varname:LineMask,prsc:2|IN-2250-R;n:type:ShaderForge.SFN_Get,id:4789,x:32674,y:32686,varname:node_4789,prsc:2|IN-8999-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:1904,x:31166,y:31945,varname:node_1904,prsc:2;n:type:ShaderForge.SFN_Vector4Property,id:2969,x:31166,y:32094,ptovrint:False,ptlb:StartFadeInPosition,ptin:_StartFadeInPosition,varname:node_2969,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Distance,id:1103,x:31388,y:32019,varname:node_1103,prsc:2|A-1904-XYZ,B-2969-XYZ;n:type:ShaderForge.SFN_Slider,id:8623,x:31241,y:31873,ptovrint:False,ptlb:LineFadeIn,ptin:_LineFadeIn,varname:node_8623,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.09014788,max:5;n:type:ShaderForge.SFN_Clamp01,id:3932,x:31904,y:32049,varname:node_3932,prsc:2|IN-3818-OUT;n:type:ShaderForge.SFN_Set,id:8906,x:32090,y:32049,varname:LineMaskIn,prsc:2|IN-3932-OUT;n:type:ShaderForge.SFN_Get,id:8206,x:32627,y:32754,varname:node_8206,prsc:2|IN-8906-OUT;n:type:ShaderForge.SFN_Vector1,id:3077,x:31398,y:31788,varname:node_3077,prsc:2,v1:0;n:type:ShaderForge.SFN_Smoothstep,id:3818,x:31639,y:31992,varname:node_3818,prsc:2|A-3077-OUT,B-8623-OUT,V-1103-OUT;n:type:ShaderForge.SFN_Slider,id:3474,x:32406,y:33028,ptovrint:False,ptlb:Alpha,ptin:_Alpha,varname:node_3474,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;proporder:7241-2250-3475-2969-8623-3474;pass:END;sub:END;*/

Shader "Shader Forge/UILaserFX" {
    Properties {
        _StartColor ("StartColor", Color) = (0.07843138,0.3921569,0.7843137,1)
        _Background ("Background", 2D) = "white" {}
        _EndColor ("EndColor", Color) = (0.07843138,0.3921569,0.7843137,1)
        _StartFadeInPosition ("StartFadeInPosition", Vector) = (0,0,0,0)
        _LineFadeIn ("LineFadeIn", Range(0, 5)) = 0.09014788
        _Alpha ("Alpha", Range(0, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 2.0
            uniform float4 _StartColor;
            uniform sampler2D _Background; uniform float4 _Background_ST;
            uniform float4 _EndColor;
            uniform float4 _StartFadeInPosition;
            uniform float _LineFadeIn;
            uniform float _Alpha;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _Background_var = tex2D(_Background,TRANSFORM_TEX(i.uv0, _Background));
                float LineMask = _Background_var.r;
                float3 LineColor = lerp(_StartColor.rgb,_EndColor.rgb,saturate((i.uv0.r*3.0+0.0)));
                float LineMaskIn = saturate(smoothstep( 0.0, _LineFadeIn, distance(i.posWorld.rgb,_StartFadeInPosition.rgb) ));
                float3 emissive = (LineMask*(LineColor*_Background_var.rgb)*LineMaskIn*_Alpha);
                float3 finalColor = emissive;
                return fixed4(finalColor,_Alpha);
            }
            ENDCG
        }
    }
    
}
