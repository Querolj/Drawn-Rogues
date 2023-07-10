// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Frame/NoContour"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [MainColor] _Color ("Main Color", Color) = (1,1,1,1)
        BrushColor("Brush color", Color) = (0,0,0,0)
        BrushTex ("Brush Texture", 2D) = "white" {}
        Width("Frame width", Int) = 1
        Height("Frame height", Int) = 1
        MousePosX("Mouse pos x in frame space", Int) = 1
        MousePosY("Mouse pos y in frame space", Int) = 1
        BrushWidth("Brush width", Int) = 1
        BrushHeight("Brush height", Int) = 1

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
            #pragma multi_compile _ PIXELSNAP_ON
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
                float2 texcoord  : TEXCOORD0;
            };
            

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                #ifdef PIXELSNAP_ON
                    OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            sampler2D _MainTex;
            sampler2D BrushTex;
            float4 BrushColor;
            fixed4 _Color;
            int Width;
            int Height;
            int MousePosX;
            int MousePosY;
            int BrushWidth;
            int BrushHeight;

            bool Equals(float f1, float f2)
            {
                float epsilon = 0.001;
                return abs(f1 - f2) < epsilon;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 pixel = tex2D (_MainTex, IN.texcoord);
                
                uint2 brushOrigin = uint2(MousePosX - ((BrushWidth) / 2), MousePosY - ((BrushHeight ) / 2) );
                uint2 pixelPos = uint2(IN.texcoord.x * Width, IN.texcoord.y * Height);

                float2 brushUv = float2((pixelPos.x - brushOrigin.x) / (float)BrushWidth, (pixelPos.y - brushOrigin.y) / (float)BrushHeight);
                fixed4 pixelOnBrush = tex2D (BrushTex, brushUv);

                bool useBrush = (pixelPos.x - brushOrigin.x) >= 0 && (pixelPos.x - brushOrigin.x) < BrushWidth 
                && (pixelPos.y - brushOrigin.y) >= 0 && (pixelPos.y - brushOrigin.y) < BrushHeight;

                if(useBrush && Equals(pixelOnBrush.r, 0) )
                {
                    pixel = BrushColor;
                }

                pixel.a *= _Color.a;
                pixel.rgb *= pixel.a * _Color;

                return pixel;
            }
            ENDCG
        }
    }
}