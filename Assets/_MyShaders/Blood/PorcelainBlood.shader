Shader "Survival/Horror/PorcelainBlood"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+12" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            float4 _BoundsMin, _BoundsSize;
            float _Fixture;
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; float3 positionWS:TEXCOORD1; half3 normalWS:TEXCOORD2; half fog:TEXCOORD3; };
            V Vert(A a)
            {
                V o;
                o.p = (a.positionOS.xyz - _BoundsMin.xyz) / max(_BoundsSize.xyz, .001);
                o.positionWS = TransformObjectToWorld(a.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(a.normalOS);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            half4 Frag(V i):SV_Target
            {
                float3 p=i.p;
                float2 q=p.xz;
                float grain=Noise(q*53+p.y*7);
                float rough=(Noise(q*16)-.5)*.22+(grain-.5)*.065;
                // Toilet bowl is the low, forward half; avoid coating the cistern.
                float2 center = _Fixture > .5 && _Fixture < 1.5 ? float2(.68,.50) : float2(.57,.48);
                float bowl = 1-smoothstep(.74,1.03,length((q-center)/float2(.33,.41))+rough);
                float upper = _Fixture > 1.5 ? 1 : smoothstep(.19,.45,p.y);
                if (_Fixture > .5 && _Fixture < 1.5) upper *= 1-smoothstep(.68,.76,p.y);
                float streak=1-smoothstep(.065,.13,abs(p.z-(.62+sin(p.y*9)*.055))+rough*.5);
                streak*=smoothstep(.08,.23,p.y)*(1-smoothstep(.49,.61,p.y))*smoothstep(.61,.88,p.x);
                float2 cells=floor(q*23);
                float2 drop=frac(q*23)-float2(Hash(cells),Hash(cells+8));
                float drops=(1-smoothstep(.07,.29,length(drop)))*step(.82,Hash(cells+3))*upper;
                float coverage=max(max(bowl*upper,streak*.78),drops*.7);
                coverage*=lerp(.62,1,smoothstep(.15,.69,grain));
                clip(coverage-.05);
                half3 n=normalize(i.normalWS);
                half3 view=GetWorldSpaceNormalizeViewDir(i.positionWS);
                Light sun=GetMainLight();
                half3 tint=lerp(half3(.13,.004,.008),half3(.49,.015,.021),grain);
                half3 light=max(SampleSH(n),half3(.25,.22,.22)) + sun.color*saturate(dot(n,sun.direction))*.23;
                half wet=pow(saturate(dot(n,normalize(view+half3(-.35,.86,-.32)))),32)*.085;
                return half4(MixFog(tint*light+wet,i.fog),coverage*.96);
            }
            ENDHLSL
        }
    }
}
