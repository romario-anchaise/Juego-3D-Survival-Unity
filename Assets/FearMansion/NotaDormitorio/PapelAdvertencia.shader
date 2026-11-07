Shader "Survival/Notes/AgedPaper"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura del Canvas", 2D) = "white" {}
        _PaperTex ("Papel Paper001", 2D) = "white" {}
        _Brightness ("Luminosidad", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_PaperTex);
            SAMPLER(sampler_PaperTex);
            CBUFFER_START(UnityPerMaterial)
                float _Brightness;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                o.uv=input.uv;
                o.color=input.color;
                return o;
            }
            float Hash(float2 p)
            {
                return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);
            }
            float Noise(float2 p)
            {
                float2 cell=floor(p), f=frac(p);
                f=f*f*(3-2*f);
                return lerp(lerp(Hash(cell),Hash(cell+float2(1,0)),f.x),
                    lerp(Hash(cell+float2(0,1)),Hash(cell+1),f.x),f.y);
            }
            half4 Frag(Varyings input):SV_Target
            {
                float2 uv=input.uv;
                float edge=min(min(uv.x,1-uv.x),min(uv.y,1-uv.y));
                float grain=SAMPLE_TEXTURE2D(_PaperTex,sampler_PaperTex,uv*2.6).r;
                float stains=Noise(uv*8+13);
                float fine=Noise(uv*89);
                float border=smoothstep(0,.09+stains*.07,edge);
                float rag=.003+Noise(uv*70)*.005;
                rag+=.022*(1-smoothstep(.02,.09,abs(uv.y-.28)));
                rag+=.014*(1-smoothstep(.01,.05,abs(uv.x-.71)));
                float alpha=smoothstep(rag,rag+.0025,edge);
                half3 paper=lerp(half3(.32,.20,.09),half3(.75,.64,.43),border);
                paper*=.77+grain*.19+stains*.10;
                float foxing=step(.78,Noise(uv*95+4))*(1-smoothstep(.05,.28,edge));
                paper=lerp(paper,half3(.29,.16,.06),foxing*.4);
                float crease=exp(-abs(uv.x-.49)*210);
                float creaseLight=exp(-abs(uv.x-.495)*260);
                float crossFold=exp(-abs(uv.y-.53)*240);
                paper*=1-crease*.24-crossFold*.11;
                paper+=creaseLight*.025;
                // Blood soaked into the torn edge, with dragged streaks and dried flecks.
                float2 smearUV=uv-float2(.87,.07);
                smearUV.x+=smearUV.y*.9;
                float smear=1-smoothstep(.45,.98,length(smearUV/float2(.19,.12))+(stains-.5)*.55);
                float streak=0;
                [unroll] for(int j=0;j<3;j++)
                {
                    float x=.87+j*.036;
                    float run=abs(uv.x-x+(uv.y-.08)*.22+sin(uv.y*70+j)*.002);
                    float runMask=(1-smoothstep(.003,.013,run))*smoothstep(.015,.07,uv.y)
                        *(1-smoothstep(.11+j*.018,.19+j*.018,uv.y));
                    streak=max(streak,runMask*(.4+fine*.5));
                }
                float corner=1-smoothstep(.35,1,length((uv-float2(.025,.92))/float2(.07,.115))+(stains-.5)*.8);
                float blood=max(smear*(.62+fine*.38),max(streak,corner*.85));
                [unroll] for(int k=0;k<13;k++)
                {
                    float2 center=float2(.65+Hash(float2(k,3))*.31,.025+Hash(float2(k,11))*.18);
                    float radius=.002+Hash(float2(k,21))*.009;
                    float fleck=1-smoothstep(radius*.55,radius,distance(uv,center));
                    blood=max(blood,fleck);
                }
                paper=lerp(paper,lerp(half3(.11,.025,.014),half3(.28,.042,.024),fine),blood*.92);
                return half4(paper*_Brightness,alpha);
            }
            ENDHLSL
        }
    }
}
