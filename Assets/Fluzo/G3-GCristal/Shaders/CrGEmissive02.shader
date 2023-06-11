// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True;n:type:ShaderForge.SFN_Final,id:4795,x:34504,y:32735,varname:node_4795,prsc:2|normal-1986-OUT,emission-7380-OUT,custl-3570-OUT,alpha-2053-OUT;n:type:ShaderForge.SFN_Tex2d,id:1321,x:32865,y:32642,ptovrint:False,ptlb:Map,ptin:_Map,varname:node_1321,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:7380,x:33097,y:32768,varname:node_7380,prsc:2|A-1321-RGB,B-9282-RGB,C-305-OUT;n:type:ShaderForge.SFN_Color,id:9282,x:32847,y:32905,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_9282,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:305,x:33014,y:33077,ptovrint:False,ptlb:PowerEmit,ptin:_PowerEmit,varname:node_305,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Fresnel,id:4025,x:33343,y:32850,varname:node_4025,prsc:2;n:type:ShaderForge.SFN_OneMinus,id:5981,x:33514,y:32850,varname:node_5981,prsc:2|IN-4025-OUT;n:type:ShaderForge.SFN_Multiply,id:3570,x:33832,y:32897,varname:node_3570,prsc:2|A-5981-OUT,B-5729-RGB,C-2554-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2554,x:33441,y:33107,ptovrint:False,ptlb:Fresnel,ptin:_Fresnel,varname:node_2554,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1;n:type:ShaderForge.SFN_Fresnel,id:2053,x:33641,y:33124,varname:node_2053,prsc:2|EXP-8641-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8641,x:33480,y:33218,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_8641,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Color,id:5729,x:33589,y:32645,ptovrint:False,ptlb:ColorFresnel,ptin:_ColorFresnel,varname:node_5729,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Tex2d,id:1919,x:34038,y:32621,ptovrint:False,ptlb:Normal Map,ptin:_NormalMap,varname:node_1919,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Vector3,id:7927,x:34170,y:32766,varname:node_7927,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Lerp,id:1986,x:34364,y:32615,varname:node_1986,prsc:2|A-1919-RGB,B-7927-OUT,T-4640-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4640,x:34279,y:32883,ptovrint:False,ptlb:smoothNormals,ptin:_smoothNormals,varname:node_4640,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:1321-9282-305-2554-8641-5729-1919-4640;pass:END;sub:END;*/

Shader "manyworlds/CrGEmissive01" {
    Properties {
        _Map ("Map", 2D) = "white" {}
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _PowerEmit ("PowerEmit", Float ) = 0
        _Fresnel ("Fresnel", Float ) = 0.1
        _Opacity ("Opacity", Float ) = 1
        _ColorFresnel ("ColorFresnel", Color) = (0.5,0.5,0.5,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _smoothNormals ("smoothNormals", Float ) = 0
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
            Blend SrcAlpha OneMinusSrcAlpha
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Map; uniform float4 _Map_ST;
            uniform float4 _Color;
            uniform float _PowerEmit;
            uniform float _Fresnel;
            uniform float _Opacity;
            uniform float4 _ColorFresnel;
            uniform sampler2D _NormalMap; uniform float4 _NormalMap_ST;
            uniform float _smoothNormals;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _NormalMap_var = UnpackNormal(tex2D(_NormalMap,TRANSFORM_TEX(i.uv0, _NormalMap)));
                float3 normalLocal = lerp(_NormalMap_var.rgb,float3(0,0,1),_smoothNormals);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
////// Lighting:
////// Emissive:
                float4 _Map_var = tex2D(_Map,TRANSFORM_TEX(i.uv0, _Map));
                float3 emissive = (_Map_var.rgb*_Color.rgb*_PowerEmit);
                float3 finalColor = emissive + ((1.0 - (1.0-max(0,dot(normalDirection, viewDirection))))*_ColorFresnel.rgb*_Fresnel);
                fixed4 finalRGBA = fixed4(finalColor,pow(1.0-max(0,dot(normalDirection, viewDirection)),_Opacity));
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
}
