Shader "Survival/Horror/BloodWall"
{
    Properties
    {
        _BloodColor ("Sangre", Color) = (0.32,0.008,0.013,1)
        _DryColor ("Bordes secos", Color) = (0.085,0.009,0.006,1)
        _Seed ("Variacion", Float) = 1
        _Opacity ("Opacidad", Range(0,1)) = 0.94
        _Wetness ("Brillo humedo", Range(0,1)) = 0.45
        _Drips ("Chorretones", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent" }
        Pass
        {
            Name "Blood"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BloodColor, _DryColor;
                float _Seed, _Opacity, _Wetness, _Drips;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 positionWS:TEXCOORD1; half3 normalWS:TEXCOORD2; half fog:TEXCOORD3; };
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);
                o.uv=input.uv; o.fog=ComputeFogFactor(o.positionCS.z); return o;
            }
            half4 Frag(Varyings i, FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target
            {
                float2 uv=i.uv;
                float2 offset=float2(_Seed*7.13,_Seed*3.71);
                float grain=Noise(uv*49+offset);
                float edge=Noise(uv*12+offset)*.19+Noise(uv*26+offset)*.055;
                float blob=1-length((uv-float2(.51,.65))/float2(.31,.205));
                float mask=smoothstep(.05,.15,blob+edge-.18);
                // Uneven downward runs remain connected to the main stain.
                [unroll] for(int d=0;d<7;d++)
                {
                    float r=Hash(float2(d+_Seed,19));
                    float x=.28+d*.069+(r-.5)*.035;
                    float bottom=.10+r*.36;
                    float width=.006+Hash(float2(d, _Seed+31))*.012;
                    float bend=sin(uv.y*21+d)*.004;
                    float drip=(1-smoothstep(width,width+.005,abs(uv.x-x+bend)))
                        *smoothstep(bottom,bottom+.025,uv.y)*(1-smoothstep(.63,.69,uv.y));
                    float drop=1-smoothstep(.65,1,length((uv-float2(x,bottom+.01))/float2(width*1.5,.024)));
                    mask=max(mask,max(drip,drop)*_Drips);
                }
                float2 cell=floor(uv*14);
                float2 center=(cell+float2(Hash(cell+offset),Hash(cell+offset+5)))/14;
                float radius=.003+Hash(cell+offset+7)*.013;
                float speck=(1-smoothstep(radius*.5,radius,distance(uv,center)))*step(.84,Hash(cell+offset+9));
                mask=max(mask,speck);
                mask*=smoothstep(0,.04,uv.x)*smoothstep(0,.04,uv.y)*smoothstep(0,.04,1-uv.x)*smoothstep(0,.04,1-uv.y);
                half alpha=mask*_Opacity; clip(alpha-.025);
                half3 n=normalize(i.normalWS)*IS_FRONT_VFACE(face,1,-1);
                half3 view=GetWorldSpaceNormalizeViewDir(i.positionWS);
                Light main=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 illumination=SampleSH(n)+main.color*saturate(dot(n,main.direction))*main.shadowAttenuation;
                half spec=pow(saturate(dot(n,normalize(view+main.direction))),52)*main.shadowAttenuation;
                #ifdef _ADDITIONAL_LIGHTS
                uint count=GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(count)
                    Light light=GetAdditionalLight(lightIndex,i.positionWS);
                    illumination+=light.color*saturate(dot(n,light.direction))*light.distanceAttenuation;
                    spec+=pow(saturate(dot(n,normalize(view+light.direction))),52)*light.distanceAttenuation;
                LIGHT_LOOP_END
                #endif
                half aged=saturate((1-grain)*.65+(1-mask)*.4);
                half3 color=lerp(_BloodColor.rgb,_DryColor.rgb,aged)*max(illumination,half3(.2,.2,.2));
                color+=spec*_Wetness*.12*mask;
                return half4(MixFog(color,i.fog),alpha);
            }
            ENDHLSL
        }
    }
}
