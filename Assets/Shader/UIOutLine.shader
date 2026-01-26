Shader "UI/UIOutline_Inner"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [HDR] _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
        [Toggle] _ShowOutline ("Show Outline", Float) = 0 
        _BreathSpeed ("Breath Speed", Range(0, 10)) = 3
        
        //Ç¿ÖÆ¾ØÐÎ±ß¿ò
        [Toggle] _ForceRectBorder ("Force Rect Border", Float) = 1 
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
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _ShowOutline;
            float _BreathSpeed;
            float _ForceRectBorder;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                if (_ShowOutline < 0.5) return color;


                float isRectEdge = 0;
                if (_ForceRectBorder > 0.5)
                {
                    isRectEdge += step(IN.texcoord.x, _OutlineWidth); 
                    isRectEdge += step(IN.texcoord.y, _OutlineWidth);
                    isRectEdge += step(1.0 - IN.texcoord.x, _OutlineWidth);
                    isRectEdge += step(1.0 - IN.texcoord.y, _OutlineWidth);
                }

                float2 up = float2(0, _OutlineWidth);
                float2 right = float2(_OutlineWidth, 0);
                
                float aUp = tex2D(_MainTex, IN.texcoord + up).a;
                float aDown = tex2D(_MainTex, IN.texcoord - up).a;
                float aRight = tex2D(_MainTex, IN.texcoord + right).a;
                float aLeft = tex2D(_MainTex, IN.texcoord - right).a;

                float alphaEdge = color.a * (4.0 - (aUp + aDown + aRight + aLeft));

                float isEdge = max(isRectEdge, alphaEdge);
                isEdge = saturate(isEdge);

                float breath = (sin(_Time.y * _BreathSpeed) * 0.5 + 0.5); 
                breath = 0.5 + breath * 0.5;

                fixed3 finalRGB = lerp(color.rgb, _OutlineColor.rgb, isEdge * _OutlineColor.a * breath);
                
                return fixed4(finalRGB, color.a);
            }
            ENDCG
        }
    }
}