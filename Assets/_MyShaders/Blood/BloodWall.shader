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
        [Toggle] _WarningMessage ("Mensaje FUERA DE AQUÍ", Float) = 0
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
                float _WarningMessage;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 positionWS:TEXCOORD1; half3 normalWS:TEXCOORD2; half fog:TEXCOORD3; };
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            float StrokeDistance(float2 p, float2 a, float2 b)
            {
                float2 ab = b - a;
                return length(p - a - ab * saturate(dot(p - a, ab) / dot(ab, ab)));
            }

            float WarningMask(float2 uv, float grain)
            {
                // Coordinates match the tall facade quad. Draw the letters as uneven strokes.
                float2 p = float2(uv.x * 7, (1 - uv.y) * 8.87);
                p += float2(Noise(p * 8), Noise(p * 7 + 11)) * .11 - .055;
                float ink = 10;
                ink = min(ink, StrokeDistance(p, float2(1.160,3.770), float2(1.120,2.450)));
                ink = min(ink, StrokeDistance(p, float2(1.120,2.450), float2(1.830,2.400)));
                ink = min(ink, StrokeDistance(p, float2(1.140,3.000), float2(1.690,2.940)));
                ink = min(ink, StrokeDistance(p, float2(2.060,2.490), float2(2.110,3.620)));
                ink = min(ink, StrokeDistance(p, float2(2.110,3.620), float2(2.320,3.870)));
                ink = min(ink, StrokeDistance(p, float2(2.320,3.870), float2(2.590,3.780)));
                ink = min(ink, StrokeDistance(p, float2(2.590,3.780), float2(2.740,3.500)));
                ink = min(ink, StrokeDistance(p, float2(2.740,3.500), float2(2.720,2.500)));
                ink = min(ink, StrokeDistance(p, float2(3.660,2.600), float2(3.020,2.650)));
                ink = min(ink, StrokeDistance(p, float2(3.020,2.650), float2(3.000,3.920)));
                ink = min(ink, StrokeDistance(p, float2(3.000,3.920), float2(3.710,3.850)));
                ink = min(ink, StrokeDistance(p, float2(3.020,3.240), float2(3.570,3.180)));
                ink = min(ink, StrokeDistance(p, float2(3.970,3.780), float2(3.950,2.460)));
                ink = min(ink, StrokeDistance(p, float2(3.950,2.460), float2(4.410,2.390)));
                ink = min(ink, StrokeDistance(p, float2(4.410,2.390), float2(4.620,2.630)));
                ink = min(ink, StrokeDistance(p, float2(4.620,2.630), float2(4.520,2.940)));
                ink = min(ink, StrokeDistance(p, float2(4.520,2.940), float2(3.960,3.070)));
                ink = min(ink, StrokeDistance(p, float2(3.960,3.070), float2(4.600,3.800)));
                ink = min(ink, StrokeDistance(p, float2(4.840,3.850), float2(5.210,2.510)));
                ink = min(ink, StrokeDistance(p, float2(5.210,2.510), float2(5.580,3.840)));
                ink = min(ink, StrokeDistance(p, float2(5.000,3.380), float2(5.420,3.330)));
                ink = min(ink, StrokeDistance(p, float2(0.470,5.960), float2(0.470,4.630)));
                ink = min(ink, StrokeDistance(p, float2(0.470,4.630), float2(0.870,4.690)));
                ink = min(ink, StrokeDistance(p, float2(0.870,4.690), float2(1.120,5.020)));
                ink = min(ink, StrokeDistance(p, float2(1.120,5.020), float2(1.140,5.530)));
                ink = min(ink, StrokeDistance(p, float2(1.140,5.530), float2(0.890,5.860)));
                ink = min(ink, StrokeDistance(p, float2(0.890,5.860), float2(0.470,5.960)));
                ink = min(ink, StrokeDistance(p, float2(2.060,4.720), float2(1.420,4.770)));
                ink = min(ink, StrokeDistance(p, float2(1.420,4.770), float2(1.400,6.040)));
                ink = min(ink, StrokeDistance(p, float2(1.400,6.040), float2(2.110,5.970)));
                ink = min(ink, StrokeDistance(p, float2(1.420,5.360), float2(1.970,5.300)));
                ink = min(ink, StrokeDistance(p, float2(3.240,5.970), float2(3.610,4.630)));
                ink = min(ink, StrokeDistance(p, float2(3.610,4.630), float2(3.980,5.960)));
                ink = min(ink, StrokeDistance(p, float2(3.400,5.500), float2(3.820,5.450)));
                ink = min(ink, StrokeDistance(p, float2(4.460,4.730), float2(4.780,4.690)));
                ink = min(ink, StrokeDistance(p, float2(4.780,4.690), float2(4.920,5.040)));
                ink = min(ink, StrokeDistance(p, float2(4.920,5.040), float2(4.900,5.690)));
                ink = min(ink, StrokeDistance(p, float2(4.900,5.690), float2(4.700,6.020)));
                ink = min(ink, StrokeDistance(p, float2(4.700,6.020), float2(4.370,6.010)));
                ink = min(ink, StrokeDistance(p, float2(4.370,6.010), float2(4.200,5.660)));
                ink = min(ink, StrokeDistance(p, float2(4.200,5.660), float2(4.230,5.070)));
                ink = min(ink, StrokeDistance(p, float2(4.230,5.070), float2(4.460,4.730)));
                ink = min(ink, StrokeDistance(p, float2(4.630,5.690), float2(4.960,6.200)));
                ink = min(ink, StrokeDistance(p, float2(5.160,4.770), float2(5.210,5.900)));
                ink = min(ink, StrokeDistance(p, float2(5.210,5.900), float2(5.420,6.150)));
                ink = min(ink, StrokeDistance(p, float2(5.420,6.150), float2(5.690,6.060)));
                ink = min(ink, StrokeDistance(p, float2(5.690,6.060), float2(5.840,5.780)));
                ink = min(ink, StrokeDistance(p, float2(5.840,5.780), float2(5.820,4.780)));
                ink = min(ink, StrokeDistance(p, float2(6.400,4.670), float2(6.340,5.970)));
                ink = min(ink, StrokeDistance(p, float2(6.410,4.450), float2(6.600,4.190)));
                float mask = 1 - smoothstep(.042, .11, ink + (grain - .5) * .055);
                // Runs begin on the lower edges of selected letters.
                mask = max(mask, (1 - smoothstep(.018, .046, StrokeDistance(p, float2(1.14, 3.73), float2(1.15, 4.34)))) * .85);
                mask = max(mask, (1 - smoothstep(.012, .034, StrokeDistance(p, float2(4.62, 3.82), float2(4.59, 4.19)))) * .8);
                mask = max(mask, (1 - smoothstep(.017, .043, StrokeDistance(p, float2(1.41, 5.96), float2(1.39, 6.95)))) * .8);
                mask = max(mask, (1 - smoothstep(.016, .045, StrokeDistance(p, float2(4.96, 6.14), float2(4.94, 6.83)))) * .85);
                return mask;
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
                if (_WarningMessage > .5)
                    mask=WarningMask(uv,grain);
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
