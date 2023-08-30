using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 顶点外扩
/// </summary>
public class OutlineEx : BaseMeshEffect
{
    private const string FONT_TEX_SIZE_NAME = "_FontTexSize";
    private const string OUTLINE_COLOR_NAME = "_OutlineColor";
    private const string OUTLINE_WIDTH_NAME = "_OutlineWidth";
    private const string SHADER_PATH = "UI/OutlineEx";

    [Range(0, 3)] public float outlineWidth = 1.5f;
    public float width = 512;
    public float height = 512;

    [Header("默认使用Base/Scripts/Outline.mat, 如果需要修改描边颜色这个为null")] 
    public Material material;
    private Text text;

    public Color outlineColor = Color.black;
    private static List<UIVertex> vetexList = new List<UIVertex>();

    protected override void Awake()
    {
        base.Awake();
        text = GetComponent<Text>();
        UpdateMaterial();
    }

    protected override void Start()
    {
        base.Start();
        UpdateFontMainTexTexelSize(text.font);
        UpdateAdditionalShaderChannels();
        Font.textureRebuilt += TextTextureRebuild;
    }

    private void UpdateAdditionalShaderChannels()
    {
        if (graphic == null || graphic.canvas == null)
            return;

        var v1 = graphic.canvas.additionalShaderChannels;
        var v2 = AdditionalCanvasShaderChannels.TexCoord1;

        if ((v1 & v2) != v2)
        {
            graphic.canvas.additionalShaderChannels |= v2;
        }

        v2 = AdditionalCanvasShaderChannels.TexCoord2;
        if ((v1 & v2) != v2)
        {
            graphic.canvas.additionalShaderChannels |= v2;
        }
    }

    private void TextTextureRebuild(Font font)
    {
        if (this == null)
            return;

        if (text == null)
            text = GetComponent<Text>();

        if (text == null)
        {
#if CLIENT
            Logger.Error("no find text com, name: ", gameObject.name);
#endif
            return;
        }

        if (text.font == font)
        {
            UpdateFontMainTexTexelSize(font);
        }
    }

    private void UpdateFontMainTexTexelSize(Font font)
    {
        if (material == null)
            return;

        if (font == null || font.material == null || font.material.mainTexture == null)
            return;

        width = font.material.mainTexture.width;
        height = font.material.mainTexture.height;
        Vector4 vector = new Vector4(1.0f / width, 1.0f / height, width, height);
        material.SetVector(FONT_TEX_SIZE_NAME, vector);
    }

    private void SetParam()
    {
        material.SetColor(OUTLINE_COLOR_NAME, outlineColor);
        material.SetFloat(OUTLINE_WIDTH_NAME, outlineWidth);
    }

    private void UpdateMaterial()
    {
        if (graphic == null)
        {
#if CLIENT
            Logger.Error("base.graphic == null");
#endif
            return;
        }

        if (material == null)
            material = new Material(Shader.Find(SHADER_PATH));

        graphic.material = material;
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        SetParam();
        vh.GetUIVertexStream(vetexList);

        ProcessVertices();

        vh.Clear();
        vh.AddUIVertexTriangleStream(vetexList);
    }

    private void ProcessVertices()
    {
        for (int i = 0, count = vetexList.Count - 3; i <= count; i += 3)
        {
            var v1 = vetexList[i];
            var v2 = vetexList[i + 1];
            var v3 = vetexList[i + 2];
            // 计算原顶点坐标中心点
            //
            var minX = Min(v1.position.x, v2.position.x, v3.position.x);
            var minY = Min(v1.position.y, v2.position.y, v3.position.y);
            var maxX = Max(v1.position.x, v2.position.x, v3.position.x);
            var maxY = Max(v1.position.y, v2.position.y, v3.position.y);
            var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
            // 计算原始顶点坐标和UV的方向
            //
            Vector2 triX, triY, uvX, uvY;
            Vector2 pos1 = v1.position;
            Vector2 pos2 = v2.position;
            Vector2 pos3 = v3.position;
            if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right))
                > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right)))
            {
                triX = pos2 - pos1;
                triY = pos3 - pos2;
                uvX = v2.uv0 - v1.uv0;
                uvY = v3.uv0 - v2.uv0;
            }
            else
            {
                triX = pos3 - pos2;
                triY = pos2 - pos1;
                uvX = v3.uv0 - v2.uv0;
                uvY = v2.uv0 - v1.uv0;
            }
            // 计算原始UV框
            //
            var uvMin = Min(v1.uv0, v2.uv0, v3.uv0);
            var uvMax = Max(v1.uv0, v2.uv0, v3.uv0);
            var uvOrigin = new Vector4(uvMin.x, uvMin.y, uvMax.x, uvMax.y);
            // 为每个顶点设置新的Position和UV，并传入原始UV框
            //
            v1 = SetNewPosAndUV(v1, outlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);
            v2 = SetNewPosAndUV(v2, outlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);
            v3 = SetNewPosAndUV(v3, outlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);
            // 应用设置后的UIVertex
            //
            vetexList[i] = v1;
            vetexList[i + 1] = v2;
            vetexList[i + 2] = v3;
        }
    }

    private static UIVertex SetNewPosAndUV(UIVertex pVertex, float pOutLineWidth,
        Vector2 pPosCenter,
        Vector2 pTriangleX, Vector2 pTriangleY,
        Vector2 pUVX, Vector2 pUVY,
        Vector4 pUVOrigin)
    {
        // Position
        var pos = pVertex.position;
        var posXOffset = pos.x > pPosCenter.x ? pOutLineWidth : -pOutLineWidth;
        var posYOffset = pos.y > pPosCenter.y ? pOutLineWidth : -pOutLineWidth;
        pos.x += posXOffset;
        pos.y += posYOffset;
        pVertex.position = pos;

        // UV
        var uv = pVertex.uv0;
        uv += new Vector4(
            (pUVX / pTriangleX.magnitude * posXOffset * (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1)).x,
            (pUVX / pTriangleX.magnitude * posXOffset * (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1)).y,
            0, 0);

        uv += new Vector4(
            (pUVY / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1)).x,
            (pUVY / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1)).y,
            0, 0);

        pVertex.uv0 = uv;

        // 原始UV框
        pVertex.uv1 = new Vector2(pUVOrigin.x, pUVOrigin.y);
        pVertex.uv2 = new Vector2(pUVOrigin.z, pUVOrigin.w);

        return pVertex;
    }

    private static float Min(float pA, float pB, float pC)
    {
        return Mathf.Min(Mathf.Min(pA, pB), pC);
    }

    private static float Max(float pA, float pB, float pC)
    {
        return Mathf.Max(Mathf.Max(pA, pB), pC);
    }

    private static Vector2 Min(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(Min(pA.x, pB.x, pC.x), Min(pA.y, pB.y, pC.y));
    }

    private static Vector2 Max(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(Max(pA.x, pB.x, pC.x), Max(pA.y, pB.y, pC.y));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        vetexList?.Clear();
        Font.textureRebuilt -= TextTextureRebuild;
    }
}
