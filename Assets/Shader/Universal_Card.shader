Shader "UI/Universal_Card"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.05)) = 0.005 
        [Toggle] _ShowOutline ("Show Outline", Float) = 0 
        _BreathSpeed ("Breath Speed", Range(0, 10)) = 2

        _DissolveTex ("Dissolve Noise", 2D) = "white" {} 
        _DissolveAmount ("Dissolve Amount", Range(0, 1.1)) = 0 
        [HDR] _BurnColor ("Burn Color", Color) = (1, 0.2, 0, 1) 
        _BurnWidth ("Burn Width", Range(0, 0.2)) = 0.05

        [Toggle] _ShowEthereal ("Show Ethereal", Float) = 0
        [HDR] _EtherealColor ("Ethereal Color", Color) = (0.3, 0, 0.6, 0.5) 
        _EtherealSpeed ("Ethereal Speed", Range(0, 10)) = 2

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                
                float2 noiseUV  : TEXCOORD1; 
                float4 worldPosition : TEXCOORD2; 
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _ShowOutline;
            float _BreathSpeed;

            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;
            float _DissolveAmount;
            fixed4 _BurnColor;
            float _BurnWidth;

            //ÐéÎÞ±äÁ¿
            float _ShowEthereal;
            fixed4 _EtherealColor;
            float _EtherealSpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                
                OUT.texcoord = IN.texcoord;
                OUT.noiseUV = TRANSFORM_TEX(IN.texcoord, _DissolveTex);
                
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {

                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                float originalAlpha = color.a;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                if (_DissolveAmount > 0.0)
                {
                    float noise = tex2D(_DissolveTex, IN.noiseUV).r;
                    
                    if (noise < _DissolveAmount)
                    {
                        color.a = 0; 
                    }
                    else if (noise < _DissolveAmount + _BurnWidth)
                    {
                        color.rgb = lerp(color.rgb, _BurnColor.rgb, 0.8);

                        color.a = originalAlpha; 
                    }
                }

                if (color.a <= 0.01) return fixed4(0,0,0,0);

                if (_ShowEthereal > 0.5)
                {
                    float breath = sin(_Time.y * _EtherealSpeed) * 0.5 + 0.5;
                    fixed4 ether = _EtherealColor;
                    ether.a *= breath;
                    color.rgb = lerp(color.rgb, ether.rgb, ether.a * 0.6);
                }

                if (_ShowOutline > 0.5)
                {
                    float2 up = float2(0, _OutlineWidth);
                    float2 down = float2(0, -_OutlineWidth);
                    float2 left = float2(-_OutlineWidth, 0);
                    float2 right = float2(_OutlineWidth, 0);

                    fixed4 pUp = tex2D(_MainTex, IN.texcoord + up);
                    fixed4 pDown = tex2D(_MainTex, IN.texcoord + down);
                    fixed4 pLeft = tex2D(_MainTex, IN.texcoord + left);
                    fixed4 pRight = tex2D(_MainTex, IN.texcoord + right);

                    float alphaSum = pUp.a + pDown.a + pLeft.a + pRight.a;
                    float outlineAlpha = clamp(alphaSum, 0, 1);

                    float breath = (sin(_Time.y * _BreathSpeed) * 0.5 + 0.5); 
                    breath = 0.5 + breath * 0.5;

                    fixed4 outlineCol = _OutlineColor;
                    outlineCol.a *= outlineAlpha * breath;

                    color = color + outlineCol * (1 - color.a);
                }

                return color;
            }
            ENDCG
        }
    }
}