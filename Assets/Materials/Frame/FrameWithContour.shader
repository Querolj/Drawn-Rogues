// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Frame/Contour"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		BrushTex ("Brush Texture", 2D) = "white" {}
		Tex ("Texture", 2D) = "white" {}
		TexWidth("Tex width", Int) = 1
		TexHeight("Tex height", Int) = 1

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
			"IgnoreProjector"="True" 
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
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			sampler2D _MainTex;
			sampler2D Tex;
			sampler2D BrushTex;
			float Width;
			float Height;
			int TexWidth;
			int TexHeight;
			int MousePosX;
			int MousePosY;
			int BrushWidth;
			int BrushHeight;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;

				#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				return OUT;
			}

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

				bool useBrush = brushUv.x >= 0 && (brushUv.x * BrushWidth) < BrushWidth 
				&& brushUv.y >= 0 && (brushUv.y * BrushHeight) < BrushHeight; // check only the brush uv directly

				if(useBrush && pixelOnBrush.r < 0.001)
				{
					float2 texUv = float2(pixelPos.x % TexWidth, pixelPos.y / TexHeight);
					texUv /= float2(TexWidth, TexHeight);
					
					pixel = tex2D (Tex, IN.texcoord);
				}

				if(pixel.a <= 0.001)
				{
					float outlinePixWidth = 1;
					float pixelUv = outlinePixWidth/Width;
					fixed4 pixelNorth = tex2D (_MainTex, clamp(IN.texcoord + float2(0, pixelUv), 0, 0.9999));
					fixed4 pixelEst = tex2D (_MainTex, clamp(IN.texcoord + float2(pixelUv, 0), 0, 0.9999));
					fixed4 pixelSouth = tex2D (_MainTex, saturate(IN.texcoord + float2(0, -pixelUv)));
					fixed4 pixelWeast = tex2D (_MainTex, saturate(IN.texcoord + float2(-pixelUv,0)));
					pixel.a = saturate(pixelNorth.a + pixelEst.a + pixelSouth.a + pixelWeast.a);
				}
				
				pixel.a *= IN.color.a;
				pixel.rgb *= pixel.a * IN.color.rgb;

				return pixel;
			}
			ENDCG
		}
	}
}