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
        [Enum(Ninguno,0,Fuera de aqui,1,Siempre te observa,2)] _WarningMessage ("Mensaje de sangre", Float) = 0
        [Enum(Mancha,0,Pentagrama,1,Emblema,2)] _CultSymbol ("Marca del culto", Float) = 0
        [Enum(Original,0,Salpicadura,1,Huella,2,Arrastre,3,Charco,4)] _CrimeShape ("Forma de la mancha", Float) = 0
        _FabColor ("Textura de sangre", 2D) = "white" {}
        [Normal] _FabNormal ("Relieve", 2D) = "bump" {}
        _FabRoughness ("Rugosidad", 2D) = "white" {}
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
                float _CultSymbol;
                float _CrimeShape;
                float4 _FabColor_TexelSize;
            CBUFFER_END
            TEXTURE2D(_FabColor); SAMPLER(sampler_FabColor);
            TEXTURE2D(_FabNormal); SAMPLER(sampler_FabNormal);
            TEXTURE2D(_FabRoughness); SAMPLER(sampler_FabRoughness);
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; float4 tangentOS:TANGENT; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 positionWS:TEXCOORD1; half3 normalWS:TEXCOORD2; half fog:TEXCOORD3; half4 tangentWS:TEXCOORD4; };
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
            float WatcherWarningMask(float2 uv, float grain)
            {
                float2 p = float2(uv.x * 11.4, (1 - uv.y) * 3.8);
                p += float2(Noise(p * 9), Noise(p * 7 + 11)) * .14 - .07;
                float ink = 10;
                ink = min(ink, StrokeDistance(p, float2(1.400,0.530), float2(0.780,0.550)));
                ink = min(ink, StrokeDistance(p, float2(0.780,0.550), float2(0.600,0.730)));
                ink = min(ink, StrokeDistance(p, float2(0.600,0.730), float2(0.740,0.950)));
                ink = min(ink, StrokeDistance(p, float2(0.740,0.950), float2(1.300,1.080)));
                ink = min(ink, StrokeDistance(p, float2(1.300,1.080), float2(1.430,1.330)));
                ink = min(ink, StrokeDistance(p, float2(1.430,1.330), float2(1.250,1.500)));
                ink = min(ink, StrokeDistance(p, float2(1.250,1.500), float2(0.630,1.510)));
                ink = min(ink, StrokeDistance(p, float2(2.090,0.629), float2(2.060,1.629)));
                ink = min(ink, StrokeDistance(p, float2(3.490,0.444), float2(2.750,0.474)));
                ink = min(ink, StrokeDistance(p, float2(2.750,0.474), float2(2.710,1.444)));
                ink = min(ink, StrokeDistance(p, float2(2.710,1.444), float2(3.510,1.414)));
                ink = min(ink, StrokeDistance(p, float2(2.750,0.924), float2(3.380,0.904)));
                ink = min(ink, StrokeDistance(p, float2(3.720,1.505), float2(3.750,0.505)));
                ink = min(ink, StrokeDistance(p, float2(3.750,0.505), float2(4.170,1.105)));
                ink = min(ink, StrokeDistance(p, float2(4.170,1.105), float2(4.560,0.505)));
                ink = min(ink, StrokeDistance(p, float2(4.560,0.505), float2(4.610,1.505)));
                ink = min(ink, StrokeDistance(p, float2(4.810,1.638), float2(4.810,0.658)));
                ink = min(ink, StrokeDistance(p, float2(4.810,0.658), float2(5.400,0.638)));
                ink = min(ink, StrokeDistance(p, float2(5.400,0.638), float2(5.610,0.818)));
                ink = min(ink, StrokeDistance(p, float2(5.610,0.818), float2(5.550,1.028)));
                ink = min(ink, StrokeDistance(p, float2(5.550,1.028), float2(5.350,1.138)));
                ink = min(ink, StrokeDistance(p, float2(5.350,1.138), float2(4.810,1.138)));
                ink = min(ink, StrokeDistance(p, float2(5.850,1.463), float2(5.850,0.483)));
                ink = min(ink, StrokeDistance(p, float2(5.850,0.483), float2(6.440,0.463)));
                ink = min(ink, StrokeDistance(p, float2(6.440,0.463), float2(6.650,0.643)));
                ink = min(ink, StrokeDistance(p, float2(6.650,0.643), float2(6.590,0.853)));
                ink = min(ink, StrokeDistance(p, float2(6.590,0.853), float2(6.390,0.963)));
                ink = min(ink, StrokeDistance(p, float2(6.390,0.963), float2(5.850,0.963)));
                ink = min(ink, StrokeDistance(p, float2(6.210,0.963), float2(6.670,1.463)));
                ink = min(ink, StrokeDistance(p, float2(7.650,0.481), float2(6.910,0.511)));
                ink = min(ink, StrokeDistance(p, float2(6.910,0.511), float2(6.870,1.481)));
                ink = min(ink, StrokeDistance(p, float2(6.870,1.481), float2(7.670,1.451)));
                ink = min(ink, StrokeDistance(p, float2(6.910,0.961), float2(7.540,0.941)));
                ink = min(ink, StrokeDistance(p, float2(8.870,0.514), float2(9.870,0.484)));
                ink = min(ink, StrokeDistance(p, float2(9.380,0.524), float2(9.360,1.484)));
                ink = min(ink, StrokeDistance(p, float2(10.770,0.459), float2(10.030,0.489)));
                ink = min(ink, StrokeDistance(p, float2(10.030,0.489), float2(9.990,1.459)));
                ink = min(ink, StrokeDistance(p, float2(9.990,1.459), float2(10.790,1.429)));
                ink = min(ink, StrokeDistance(p, float2(10.030,0.939), float2(10.660,0.919)));
                ink = min(ink, StrokeDistance(p, float2(2.450,2.020), float2(2.820,2.060)));
                ink = min(ink, StrokeDistance(p, float2(2.820,2.060), float2(2.990,2.330)));
                ink = min(ink, StrokeDistance(p, float2(2.990,2.330), float2(2.940,2.780)));
                ink = min(ink, StrokeDistance(p, float2(2.940,2.780), float2(2.710,2.990)));
                ink = min(ink, StrokeDistance(p, float2(2.710,2.990), float2(2.340,2.980)));
                ink = min(ink, StrokeDistance(p, float2(2.340,2.980), float2(2.110,2.750)));
                ink = min(ink, StrokeDistance(p, float2(2.110,2.750), float2(2.110,2.280)));
                ink = min(ink, StrokeDistance(p, float2(2.110,2.280), float2(2.260,2.080)));
                ink = min(ink, StrokeDistance(p, float2(2.260,2.080), float2(2.450,2.020)));
                ink = min(ink, StrokeDistance(p, float2(3.190,3.099), float2(3.190,2.119)));
                ink = min(ink, StrokeDistance(p, float2(3.190,2.119), float2(3.700,2.099)));
                ink = min(ink, StrokeDistance(p, float2(3.700,2.099), float2(3.930,2.269)));
                ink = min(ink, StrokeDistance(p, float2(3.930,2.269), float2(3.830,2.479)));
                ink = min(ink, StrokeDistance(p, float2(3.830,2.479), float2(3.610,2.559)));
                ink = min(ink, StrokeDistance(p, float2(3.610,2.559), float2(3.190,2.559)));
                ink = min(ink, StrokeDistance(p, float2(3.610,2.559), float2(3.980,2.679)));
                ink = min(ink, StrokeDistance(p, float2(3.980,2.679), float2(3.990,2.909)));
                ink = min(ink, StrokeDistance(p, float2(3.990,2.909), float2(3.780,3.089)));
                ink = min(ink, StrokeDistance(p, float2(3.780,3.089), float2(3.190,3.099)));
                ink = min(ink, StrokeDistance(p, float2(4.980,1.914), float2(4.360,1.934)));
                ink = min(ink, StrokeDistance(p, float2(4.360,1.934), float2(4.180,2.114)));
                ink = min(ink, StrokeDistance(p, float2(4.180,2.114), float2(4.320,2.334)));
                ink = min(ink, StrokeDistance(p, float2(4.320,2.334), float2(4.880,2.464)));
                ink = min(ink, StrokeDistance(p, float2(4.880,2.464), float2(5.010,2.714)));
                ink = min(ink, StrokeDistance(p, float2(5.010,2.714), float2(4.830,2.884)));
                ink = min(ink, StrokeDistance(p, float2(4.830,2.884), float2(4.210,2.894)));
                ink = min(ink, StrokeDistance(p, float2(6.030,1.975), float2(5.290,2.005)));
                ink = min(ink, StrokeDistance(p, float2(5.290,2.005), float2(5.250,2.975)));
                ink = min(ink, StrokeDistance(p, float2(5.250,2.975), float2(6.050,2.945)));
                ink = min(ink, StrokeDistance(p, float2(5.290,2.455), float2(5.920,2.435)));
                ink = min(ink, StrokeDistance(p, float2(6.310,3.108), float2(6.310,2.128)));
                ink = min(ink, StrokeDistance(p, float2(6.310,2.128), float2(6.900,2.108)));
                ink = min(ink, StrokeDistance(p, float2(6.900,2.108), float2(7.110,2.288)));
                ink = min(ink, StrokeDistance(p, float2(7.110,2.288), float2(7.050,2.498)));
                ink = min(ink, StrokeDistance(p, float2(7.050,2.498), float2(6.850,2.608)));
                ink = min(ink, StrokeDistance(p, float2(6.850,2.608), float2(6.310,2.608)));
                ink = min(ink, StrokeDistance(p, float2(6.670,2.608), float2(7.130,3.108)));
                ink = min(ink, StrokeDistance(p, float2(7.300,1.933), float2(7.710,2.933)));
                ink = min(ink, StrokeDistance(p, float2(7.710,2.933), float2(8.160,1.933)));
                ink = min(ink, StrokeDistance(p, float2(8.320,2.951), float2(8.770,1.951)));
                ink = min(ink, StrokeDistance(p, float2(8.770,1.951), float2(9.250,2.951)));
                ink = min(ink, StrokeDistance(p, float2(8.530,2.551), float2(9.020,2.541)));
                float mask = 1 - smoothstep(.035, .10, ink + (grain - .5) * .06);
                float drip = min(StrokeDistance(p, float2(.65,1.51),float2(.69,1.87)),
                    StrokeDistance(p,float2(5.30,2.98),float2(5.27,3.56)));
                return max(mask, (1 - smoothstep(.012,.043,drip)) * .85);
            }

            float CultMask(float2 uv, float grain)
            {
                float2 p = (uv - .5) * 2;
                // A slight wandering stroke avoids a perfect printed stencil.
                p += float2(Noise(uv * 21 + _Seed), Noise(uv * 19 - _Seed)) * .018 - .009;
                float ink = 10;
                float angle = atan2(p.y, p.x);
                float ring = abs(length(p) - .73);
                if (_CultSymbol < 1.5)
                {
                    [unroll] for (int k = 0; k < 5; k++)
                    {
                        float a = -1.5707963 + k * 1.2566371;
                        float b = -1.5707963 + (k + 2) * 1.2566371;
                        float2 from = float2(cos(a), sin(a)) * .63;
                        float2 to = float2(cos(b), sin(b)) * .63;
                        // The last line stops short, as if the drawing was interrupted.
                        if (k == 4) to = lerp(from, to, .64);
                        ink = min(ink, StrokeDistance(p, from, to));
                    }
                    if (angle < 2.45 && angle > -2.9) ink = min(ink, ring);
                }
                else
                {
                    // Open ring and three descending cuts: the recurring cult signature.
                    if (angle < 1.12 || angle > 1.82) ink = ring;
                    [unroll] for (int j = 0; j < 3; j++)
                    {
                        float x = (j - 1) * .24;
                        ink = min(ink, StrokeDistance(p, float2(x, .23 + j * .025),
                            float2(x + .025, -.85 + abs(j - 1) * .13)));
                    }
                }
                float width = .018 + grain * .012;
                float mark = 1 - smoothstep(width, width + .013, ink);
                // Dry pigment breaks up in the grain of the wall.
                return mark * lerp(.35, 1, smoothstep(.16, .48, Noise(uv * 180 + _Seed)));
            }
            float CrimeMask(float2 uv, float grain)
            {
                float2 seed = float2(_Seed * 2.41, _Seed * 7.21);
                float rough = (Noise(uv * 17 + seed) - .5) * .10;
                if (_CrimeShape < 1.5)
                {
                    float2 cell = floor(uv * 19);
                    float2 center = (cell + .16 + .68 * float2(Hash(cell + seed), Hash(cell + seed + 4))) / 19;
                    float2 delta = center - float2(.30, .35);
                    float spread = saturate(1 - length(float2(delta.x, (delta.y - delta.x * .40) * 1.8)));
                    float radius = lerp(.002, .026, Hash(cell + seed + 11)) * spread;
                    float2 drop = uv - center;
                    drop.y -= drop.x * .4;
                    return (1 - smoothstep(radius * .65, radius + .001, length(drop * float2(.66, 1.4))))
                        * step(.68 - spread * .26, Hash(cell + seed + 16));
                }
                if (_CrimeShape < 2.5)
                {
                    float2 p = uv;
                    p.x += (uv.y - .5) * .24;
                    float palm = 1 - smoothstep(.86, 1.02, length((p - float2(.51,.42)) / float2(.18,.15)) + rough);
                    float fingers = 0;
                    [unroll] for (int k = 0; k < 4; k++)
                    {
                        float x = .38 + k * .088;
                        float end = .83 - abs(k - 1) * .045;
                        fingers = max(fingers, 1 - smoothstep(.021,.034,
                            StrokeDistance(p,float2(x,.47),float2(x+(k-1.4)*.028,end)) + rough*.12));
                    }
                    float thumb = 1 - smoothstep(.03,.047,StrokeDistance(p,float2(.41,.38),float2(.26,.56)));
                    float slip = (1 - smoothstep(.025,.058,abs(p.x - (.52 + (p.y-.42)*.42)) + rough*.3))
                        * smoothstep(.10,.23,p.y) * (1-smoothstep(.35,.49,p.y)) * .67;
                    return max(max(palm,fingers),max(thumb,slip)) * lerp(.36,1,smoothstep(.20,.57,grain));
                }
                if (_CrimeShape < 3.5)
                {
                    float center = .5 + sin(uv.y * 6 + _Seed) * .10;
                    float width = .08 + Noise(float2(uv.y*12,_Seed))*.15;
                    float edge = abs(uv.x-center) + rough;
                    float breakup = smoothstep(.26,.63,Noise(uv * float2(54,8) + seed));
                    return (1-smoothstep(width*.44,width,edge)) * breakup
                        * smoothstep(.09,.31,uv.y) * (1-smoothstep(.68,.96,uv.y));
                }
                float2 p = uv - float2(.51,.51);
                float pool = length(p / float2(.43,.34)) + rough * 2.6;
                pool += .12 * sin(atan2(p.y,p.x)*5 + _Seed);
                return 1 - smoothstep(.77,.99,pool);
            }
            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);
                o.tangentWS=half4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w * GetOddNegativeScale());
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
                if (_CrimeShape > .5)
                    mask=CrimeMask(uv,grain);
                if (_WarningMessage > 1.5)
                    mask=max(WatcherWarningMask(uv,grain),speck);
                else if (_WarningMessage > .5)
                    mask=max(WarningMask(uv,grain),speck);
                if (_CultSymbol > .5)
                    mask=CultMask(uv,grain);
                mask*=smoothstep(0,.04,uv.x)*smoothstep(0,.04,uv.y)*smoothstep(0,.04,1-uv.x)*smoothstep(0,.04,1-uv.y);
                // Collaborators without the local Fab textures retain the procedural decal.
                bool textured = _FabColor_TexelSize.z > 4 && _WarningMessage < .5 && _CultSymbol < .5;
                half4 decal=half4(0,0,0,0);
                half gloss=_Wetness;
                if (textured)
                {
                    decal=SAMPLE_TEXTURE2D(_FabColor,sampler_FabColor,uv);
                    mask=decal.a;
                    gloss=1-SAMPLE_TEXTURE2D(_FabRoughness,sampler_FabRoughness,uv).r;
                }
                half alpha=mask*_Opacity; clip(alpha-.025);
                half3 n=normalize(i.normalWS)*IS_FRONT_VFACE(face,1,-1);
                if (textured && abs(i.tangentWS.w) > .5)
                {
                    half3 t=normalize(i.tangentWS.xyz);
                    half3 b=cross(n,t)*i.tangentWS.w;
                    half3 detail=UnpackNormal(SAMPLE_TEXTURE2D(_FabNormal,sampler_FabNormal,uv));
                    n=normalize(t*detail.x*.55+b*detail.y*.55+n*detail.z);
                }
                half exponent=lerp(15,90,gloss);
                half3 view=GetWorldSpaceNormalizeViewDir(i.positionWS);
                Light main=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 illumination=SampleSH(n)+main.color*saturate(dot(n,main.direction))*main.shadowAttenuation;
                half spec=pow(saturate(dot(n,normalize(view+main.direction))),exponent)*main.shadowAttenuation;
                #ifdef _ADDITIONAL_LIGHTS
                uint count=GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(count)
                    Light light=GetAdditionalLight(lightIndex,i.positionWS);
                    illumination+=light.color*saturate(dot(n,light.direction))*light.distanceAttenuation;
                    spec+=pow(saturate(dot(n,normalize(view+light.direction))),exponent)*light.distanceAttenuation;
                LIGHT_LOOP_END
                #endif
                half aged=saturate((1-grain)*.65+(1-mask)*.4);
                half3 pigment=textured ? decal.rgb : lerp(_BloodColor.rgb,_DryColor.rgb,aged);
                half3 color=pigment*max(illumination,half3(.24,.24,.24));
                color+=spec*gloss*.12*mask;
                return half4(MixFog(color,i.fog),alpha);
            }
            ENDHLSL
        }
    }
}
