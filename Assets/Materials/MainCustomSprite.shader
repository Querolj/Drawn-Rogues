Shader "Custom/MainCustomSprite"
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
        _Cutoff ("Cutoff", Range(0, 1)) = 0.5
        Width("tex Width", float) = 0
        Height("tex Width", float) = 0
        ContourColor ("Contour Color", Color) = (1,1,1,1)
        DisplayContour("Display Red Contour", int) = 0
    }

    SubShader
    {
        Tags
        {
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf RemoveLight vertex:vert nofog alphatest:_Cutoff
        #pragma multi_compile_local _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "UnitySprites.cginc"
        
        // temporary, because with lambert light, the sprite gets dark when rotating on y axis
        half4 LightingRemoveLight (SurfaceOutput s, half3 lightDir, half atten) {
            half4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }
        
        struct Input
        {
            float2 uv_MainTex;
            fixed4 color;
        };

        float Width;
        float Height;
        float4 ContourColor;
        int DisplayContour;

        void vert (inout appdata_full v, out Input o)
        {
            v.vertex = UnityFlipSprite(v.vertex, _Flip);

            #if defined(PIXELSNAP_ON)
                v.vertex = UnityPixelSnap (v.vertex);
            #endif

            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 pixel = SampleSpriteTexture (IN.uv_MainTex) * IN.color;

            if(DisplayContour == 1)
            {
                float pixelUvWidth = 1.0/Width;
                float pixelUvHeight = 1.0/Height;

                if(pixel.a <= 0.1) // draw outline of drawed texture
                {
                    fixed4 pixelNorth = SampleSpriteTexture ( clamp(IN.uv_MainTex + float2(0, pixelUvHeight), 0, 0.9999));
                    fixed4 pixelEst = SampleSpriteTexture ( clamp(IN.uv_MainTex + float2(pixelUvWidth, 0), 0, 0.9999));
                    fixed4 pixelSouth = SampleSpriteTexture ( saturate(IN.uv_MainTex + float2(0, -pixelUvHeight)));
                    fixed4 pixelWest = SampleSpriteTexture ( saturate(IN.uv_MainTex + float2(-pixelUvWidth,0)));
                    pixel.a = saturate(pixelNorth.a + pixelEst.a + pixelSouth.a + pixelWest.a);
                    pixel.rgb = pixel.a > 0.01 ? ContourColor : pixel.rgb;
                }
            }

            o.Albedo = pixel.rgb * pixel.a;
            o.Alpha = pixel.a;
        }
        ENDCG
    }

    Fallback "Transparent/VertexLit"
}