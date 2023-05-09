// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33066,y:32674,varname:node_3138,prsc:2|emission-2926-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32022,y:33122,ptovrint:False,ptlb:StartColor,ptin:_StartColor,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_Tex2d,id:9723,x:31497,y:32898,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_9723,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:770c7f12856b353448112fb9299e1e7b,ntxv:0,isnm:False|UVIN-8874-OUT;n:type:ShaderForge.SFN_Tex2d,id:2250,x:32061,y:32791,ptovrint:False,ptlb:Background,ptin:_Background,varname:node_2250,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:f3914337be13f6249b5fb7e57bee59a2,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:6970,x:32270,y:32747,varname:node_6970,prsc:2|A-4922-OUT,B-2250-RGB;n:type:ShaderForge.SFN_TexCoord,id:7282,x:30766,y:32853,varname:node_7282,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:2926,x:32850,y:32686,varname:node_2926,prsc:2|A-4789-OUT,B-1482-OUT,C-8206-OUT,D-5833-OUT;n:type:ShaderForge.SFN_Multiply,id:4536,x:32376,y:32333,varname:node_4536,prsc:2|A-4997-OUT,B-3755-OUT,C-5574-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5424,x:30647,y:32643,ptovrint:False,ptlb:Invert,ptin:_Invert,varname:node_5424,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Abs,id:4997,x:32135,y:32214,varname:node_4997,prsc:2|IN-1743-OUT;n:type:ShaderForge.SFN_Panner,id:8145,x:31330,y:33000,varname:node_8145,prsc:2,spu:0.1,spv:0|UVIN-8874-OUT;n:type:ShaderForge.SFN_Multiply,id:7978,x:30984,y:32791,varname:node_7978,prsc:2|A-5092-OUT,B-7282-U;n:type:ShaderForge.SFN_Append,id:8874,x:31138,y:32898,varname:node_8874,prsc:2|A-7978-OUT,B-7282-V;n:type:ShaderForge.SFN_RemapRange,id:5092,x:30829,y:32688,varname:node_5092,prsc:2,frmn:-1,frmx:1,tomn:1,tomx:-1|IN-5424-OUT;n:type:ShaderForge.SFN_Distance,id:1874,x:31447,y:32406,varname:node_1874,prsc:2|A-2969-XYZ,B-461-XYZ;n:type:ShaderForge.SFN_FragmentPosition,id:461,x:31212,y:32424,varname:node_461,prsc:2;n:type:ShaderForge.SFN_Slider,id:4149,x:31290,y:32607,ptovrint:False,ptlb:Distance,ptin:_Distance,varname:node_4149,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:10;n:type:ShaderForge.SFN_Divide,id:542,x:31650,y:32521,varname:node_542,prsc:2|A-1874-OUT,B-4149-OUT;n:type:ShaderForge.SFN_Clamp01,id:5520,x:31808,y:32521,varname:node_5520,prsc:2|IN-542-OUT;n:type:ShaderForge.SFN_OneMinus,id:8126,x:31967,y:32521,varname:node_8126,prsc:2|IN-5520-OUT;n:type:ShaderForge.SFN_TexCoord,id:5439,x:31741,y:33436,varname:node_5439,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Lerp,id:1772,x:32307,y:33272,varname:node_1772,prsc:2|A-7241-RGB,B-3475-RGB,T-7701-OUT;n:type:ShaderForge.SFN_Color,id:3475,x:32022,y:33289,ptovrint:False,ptlb:EndColor,ptin:_EndColor,varname:_StartColor_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_Set,id:8366,x:32469,y:33272,varname:LineColor,prsc:2|IN-1772-OUT;n:type:ShaderForge.SFN_Get,id:4922,x:32040,y:32719,varname:node_4922,prsc:2|IN-8366-OUT;n:type:ShaderForge.SFN_RemapRange,id:1335,x:31911,y:33436,varname:node_1335,prsc:2,frmn:0,frmx:1,tomn:0,tomx:3|IN-5439-U;n:type:ShaderForge.SFN_Clamp01,id:7701,x:32080,y:33436,varname:node_7701,prsc:2|IN-1335-OUT;n:type:ShaderForge.SFN_Set,id:8999,x:32270,y:32878,varname:LineMask,prsc:2|IN-2250-R;n:type:ShaderForge.SFN_Get,id:4789,x:32674,y:32686,varname:node_4789,prsc:2|IN-8999-OUT;n:type:ShaderForge.SFN_Set,id:3707,x:31679,y:32898,varname:ArrowTexMask,prsc:2|IN-9723-RGB;n:type:ShaderForge.SFN_Get,id:3755,x:32114,y:32369,varname:node_3755,prsc:2|IN-3707-OUT;n:type:ShaderForge.SFN_Set,id:430,x:30853,y:32552,varname:ArrowDirection,prsc:2|IN-5424-OUT;n:type:ShaderForge.SFN_Get,id:1743,x:31966,y:32214,varname:node_1743,prsc:2|IN-430-OUT;n:type:ShaderForge.SFN_Set,id:4867,x:32146,y:32567,varname:DistanceMask,prsc:2|IN-8126-OUT;n:type:ShaderForge.SFN_Get,id:5574,x:32114,y:32423,varname:node_5574,prsc:2|IN-4867-OUT;n:type:ShaderForge.SFN_Blend,id:1482,x:32540,y:32750,varname:node_1482,prsc:2,blmd:6,clmp:True|SRC-4536-OUT,DST-6970-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:1904,x:31166,y:31945,varname:node_1904,prsc:2;n:type:ShaderForge.SFN_Vector4Property,id:2969,x:31166,y:32094,ptovrint:False,ptlb:StartFadeInPosition,ptin:_StartFadeInPosition,varname:node_2969,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Distance,id:1103,x:31388,y:32019,varname:node_1103,prsc:2|A-1904-XYZ,B-2969-XYZ;n:type:ShaderForge.SFN_Slider,id:8623,x:31241,y:31873,ptovrint:False,ptlb:LineFadeIn,ptin:_LineFadeIn,varname:node_8623,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.09014788,max:5;n:type:ShaderForge.SFN_Clamp01,id:3932,x:31904,y:32049,varname:node_3932,prsc:2|IN-3818-OUT;n:type:ShaderForge.SFN_Set,id:8906,x:32090,y:32049,varname:LineMaskIn,prsc:2|IN-3932-OUT;n:type:ShaderForge.SFN_Get,id:8206,x:32674,y:32803,varname:node_8206,prsc:2|IN-8906-OUT;n:type:ShaderForge.SFN_Vector1,id:3077,x:31398,y:31788,varname:node_3077,prsc:2,v1:0;n:type:ShaderForge.SFN_Smoothstep,id:3818,x:31639,y:31992,varname:node_3818,prsc:2|A-3077-OUT,B-8623-OUT,V-1103-OUT;n:type:ShaderForge.SFN_Vector4Property,id:6729,x:31256,y:31637,ptovrint:False,ptlb:EndFadeInPosition,ptin:_EndFadeInPosition,varname:_StartFadeInPosition_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Distance,id:8334,x:31480,y:31618,varname:node_8334,prsc:2|A-6729-XYZ,B-1904-XYZ;n:type:ShaderForge.SFN_Smoothstep,id:4689,x:31703,y:31646,varname:node_4689,prsc:2|A-3077-OUT,B-8623-OUT,V-8334-OUT;n:type:ShaderForge.SFN_Clamp01,id:9507,x:31908,y:31632,varname:node_9507,prsc:2|IN-4689-OUT;n:type:ShaderForge.SFN_Set,id:3639,x:32088,y:31632,varname:LineMaskOut,prsc:2|IN-9507-OUT;n:type:ShaderForge.SFN_Get,id:5833,x:32674,y:32857,varname:node_5833,prsc:2|IN-3639-OUT;proporder:7241-9723-2250-5424-4149-3475-2969-8623-6729;pass:END;sub:END;*/

Shader "Shader Forge/LaserHand" {
    Properties {
        _StartColor ("StartColor", Color) = (0.07843138,0.3921569,0.7843137,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _Background ("Background", 2D) = "white" {}
        _Invert ("Invert", Float ) = 0
        _Distance ("Distance", Range(0, 10)) = 1
        _EndColor ("EndColor", Color) = (0.07843138,0.3921569,0.7843137,1)
        _StartFadeInPosition ("StartFadeInPosition", Vector) = (0,0,0,0)
        _LineFadeIn ("LineFadeIn", Range(0, 5)) = 0.09014788
        _EndFadeInPosition ("EndFadeInPosition", Vector) = (0,0,0,0)
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
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _Background; uniform float4 _Background_ST;
            uniform float _Invert;
            uniform float _Distance;
            uniform float4 _EndColor;
            uniform float4 _StartFadeInPosition;
            uniform float _LineFadeIn;
            uniform float4 _EndFadeInPosition;
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
                float ArrowDirection = _Invert;
                float2 node_8874 = float2(((_Invert*-1.0+0.0)*i.uv0.r),i.uv0.g);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_8874, _MainTex));
                float3 ArrowTexMask = _MainTex_var.rgb;
                float DistanceMask = (1.0 - saturate((distance(_StartFadeInPosition.rgb,i.posWorld.rgb)/_Distance)));
                float3 LineColor = lerp(_StartColor.rgb,_EndColor.rgb,saturate((i.uv0.r*3.0+0.0)));
                float node_3077 = 0.0;
                float LineMaskIn = saturate(smoothstep( node_3077, _LineFadeIn, distance(i.posWorld.rgb,_StartFadeInPosition.rgb) ));
                float LineMaskOut = saturate(smoothstep( node_3077, _LineFadeIn, distance(_EndFadeInPosition.rgb,i.posWorld.rgb) ));
                float3 emissive = (LineMask*saturate((1.0-(1.0-(abs(ArrowDirection)*ArrowTexMask*DistanceMask))*(1.0-(LineColor*_Background_var.rgb))))*LineMaskIn*LineMaskOut);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}
