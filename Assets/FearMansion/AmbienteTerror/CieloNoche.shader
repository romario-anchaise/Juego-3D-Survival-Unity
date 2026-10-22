Shader "FearMansion/CieloNoche"
{
    Properties
    {
        _Zenith ("Cielo alto", Color) = (0.007,0.012,0.028,1)
        _Horizon ("Horizonte", Color) = (0.035,0.05,0.075,1)
        _Clouds ("Nubes", Color) = (0.045,0.055,0.08,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Zenith, _Horizon, _Clouds;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 direction : TEXCOORD0; };
            v2f vert(appdata v) {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.direction=v.vertex.xyz; return o;
            }
            float hash(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
            float noise(float3 p) {
                float3 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),
                                 lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),
                            lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),
                                 lerp(hash(i+float3(0,1,1)),hash(i+float3(1,1,1)),f.x),f.y),f.z);
            }
            half4 frag(v2f i) : SV_Target {
                float3 d=normalize(i.direction);
                float altitude=saturate(d.y);
                float3 color=lerp(_Horizon.rgb,_Zenith.rgb,sqrt(altitude));
                float3 p=d*float3(5,13,5)+float3(5,2,8);
                float cloud=noise(p)*.6+noise(p*2.1)*.28+noise(p*4.2)*.12;
                float cover=smoothstep(.4,.7,cloud)*smoothstep(-.02,.2,d.y);
                color=lerp(color,_Clouds.rgb,cover*.65);
                float alignment=dot(d,normalize(float3(.35,.28,.88)));
                float disc=smoothstep(.99965,.99985,alignment);
                float halo=pow(saturate(alignment),1800)*.1;
                color+=(float3(.64,.75,1)*disc+float3(.22,.3,.5)*halo)*(1-cover*.75);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
