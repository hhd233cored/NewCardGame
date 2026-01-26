Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        [HDR] _OutlineColor ("Outline Color", Color) = (1,1,0,1) //支持HDR，可以配合Bloom发光
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01   //描边宽度
        [Toggle] _ShowOutline ("Show Outline", Float) = 0       //开关
        _BreathSpeed ("Breath Speed", Range(0, 10)) = 2         //呼吸速度
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _ShowOutline;
            float _BreathSpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float _EnableExternalAlpha;

            fixed4 SampleSpriteTexture (float2 uv)
            {
                fixed4 color = tex2D (_MainTex, uv);
                #if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D (_AlphaTex, uv);
                color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
                #endif
                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                
                if (_ShowOutline < 0.5) return c;


                float2 up = float2(0, _OutlineWidth);
                float2 down = float2(0, -_OutlineWidth);
                float2 left = float2(-_OutlineWidth, 0);
                float2 right = float2(_OutlineWidth, 0);

                fixed4 pixelUp = SampleSpriteTexture(IN.texcoord + up);
                fixed4 pixelDown = SampleSpriteTexture(IN.texcoord + down);
                fixed4 pixelLeft = SampleSpriteTexture(IN.texcoord + left);
                fixed4 pixelRight = SampleSpriteTexture(IN.texcoord + right);


                float alphaSum = pixelUp.a + pixelDown.a + pixelLeft.a + pixelRight.a;
                
                float outlineAlpha = clamp(alphaSum, 0, 1);
                
                float breath = (sin(_Time.y * _BreathSpeed) * 0.5 + 0.5); 
                breath = 0.5 + breath * 0.5;


                fixed4 outlineCol = _OutlineColor;
                outlineCol.a *= outlineAlpha * breath;

                c.rgb *= c.a; 
                

                fixed4 finalColor = c + outlineCol * (1 - c.a);

                return finalColor;
            }
            ENDCG
        }
    }
}