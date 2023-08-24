Shader "UI/OutlineEx"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth("Outline Width", Float) = 1

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask[_ColorMask]

            Pass
            {
                Name "OUTLINE"

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                //Add for RectMask2D  
                #include "UnityUI.cginc"
                //End for RectMask2D 

                sampler2D _MainTex;
                float4 _Color;
                float4 _TextureSampleAdd;
                float4 _MainTex_TexelSize;

                float4 _OutlineColor;
                float _OutlineWidth;
                //Add for RectMask2D  
                float4 _ClipRect;
                //End for RectMask2D

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float2 texcoord1 : TEXCOORD1;
                    float2 texcoord2 : TEXCOORD2;
                    float4 color : COLOR;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    float2 uvOriginXY : TEXCOORD1;
                    float2 uvOriginZW : TEXCOORD2;
                    //Add for RectMask2D  
                    float4 worldPosition : TEXCOORD4;
                    //End for RectMask2D

                    float4 color : COLOR;
                };

                v2f vert(appdata IN)
                {
                    v2f o;
                    //Add for RectMask2D  
                    o.worldPosition = IN.vertex;
                    //End for RectMask2D

                    o.vertex = UnityObjectToClipPos(IN.vertex);
                    o.texcoord = IN.texcoord;
                    o.uvOriginXY = IN.texcoord1;
                    o.uvOriginZW = IN.texcoord2;
                    o.color = IN.color * _Color;

                    return o;
                }

                float IsInRect(float2 pPos, float2 pClipRectXY, float2 pClipRectZW)
                {
                    pPos = step(pClipRectXY, pPos) * step(pPos, pClipRectZW);
                    return pPos.x * pPos.y;
                }

                float SampleAlpha(int pIndex, v2f IN)
                {
                    const float sinArray[12] = { 0, 0.5, 0.866, 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5 };
                    const float cosArray[12] = { 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5, 0, 0.5, 0.866 };
                    float2 pos = IN.texcoord + _MainTex_TexelSize.xy * float2(cosArray[pIndex], sinArray[pIndex]) * _OutlineWidth;
                    return IsInRect(pos, IN.uvOriginXY, IN.uvOriginZW) * (tex2D(_MainTex, pos) + _TextureSampleAdd).w * _OutlineColor.w;
                }

                float4 frag(v2f IN) : SV_Target
                {
                    float4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                    if (_OutlineWidth > 0)
                    {
                        color.w *= IsInRect(IN.texcoord, IN.uvOriginXY, IN.uvOriginZW);
                        half4 val = half4(_OutlineColor.x, _OutlineColor.y, _OutlineColor.z, 0);

                        val.w += SampleAlpha(0, IN);
                        val.w += SampleAlpha(1, IN);
                        val.w += SampleAlpha(2, IN);
                        val.w += SampleAlpha(3, IN);
                        val.w += SampleAlpha(4, IN);
                        val.w += SampleAlpha(5, IN);
                        val.w += SampleAlpha(6, IN);
                        val.w += SampleAlpha(7, IN);
                        val.w += SampleAlpha(8, IN);
                        val.w += SampleAlpha(9, IN);
                        val.w += SampleAlpha(10, IN);
                        val.w += SampleAlpha(11, IN);

                        val.w = clamp(val.w, 0, 1);
                        color = (val * (1.0 - color.a)) + (color * color.a);
                    }


                    //Add for RectMask2D 
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
#ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
#endif
                    //End for RectMask2D
                    return color;
                }
                ENDCG
            }
        }
}