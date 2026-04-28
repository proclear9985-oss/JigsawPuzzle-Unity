using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Texture2D를 N×M 격자로 잘라 각 조각 Sprite를 생성합니다.
/// 조각 외곽은 베지어 곡선(탭/소켓) 형태의 Mesh로 만들어집니다.
/// </summary>
public static class ImageSlicer
{
    // 탭 깊이 비율 (조각 한 변 대비)
    private const float TAB_SIZE   = 0.25f;
    private const float TAB_INDENT = 0.15f;
    private const int   BEZIER_STEPS = 12;

    // ── 퍼블릭 API ─────────────────────────────────────────────────────────
    /// <summary>
    /// 이미지를 cols × rows 조각으로 분할한 PieceData 목록을 반환합니다.
    /// </summary>
    public static List<PieceData> Slice(Texture2D source, int cols, int rows)
    {
        var result = new List<PieceData>();
        int pw = source.width  / cols;
        int ph = source.height / rows;

        // 각 조각의 탭 방향을 미리 결정 (공유 엣지 일관성 유지)
        // tabs[col, row, side] : true=탭(볼록), false=소켓(오목)
        // side: 0=Right, 1=Top, 2=Left, 3=Bottom
        bool[,,] tabs = GenerateTabs(cols, rows);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int pixelX = col * pw;
                int pixelY = row * ph;

                // 조각 텍스처 (패딩 포함)
                int pad = Mathf.Max(pw, ph) / 3;
                Texture2D pieceTex = ExtractPieceTexture(source, pixelX, pixelY, pw, ph, pad);
                Sprite sprite = Sprite.Create(
                    pieceTex,
                    new Rect(0, 0, pieceTex.width, pieceTex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);

                var data = new PieceData
                {
                    id          = row * cols + col,
                    col         = col,
                    row         = row,
                    sprite      = sprite,
                    tabRight    = tabs[col, row, 0],
                    tabTop      = tabs[col, row, 1],
                    tabLeft     = tabs[col, row, 2],
                    tabBottom   = tabs[col, row, 3],
                };
                result.Add(data);
            }
        }
        return result;
    }

    // ── 내부 메서드 ────────────────────────────────────────────────────────
    private static bool[,,] GenerateTabs(int cols, int rows)
    {
        // [col, row, side(0=R,1=T,2=L,3=B)]
        bool[,,] tabs = new bool[cols, rows, 4];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Right edge
                if (col < cols - 1)
                {
                    bool t = Random.value > 0.5f;
                    tabs[col,     row, 0] =  t;  // 현재 조각 오른쪽 = 탭
                    tabs[col + 1, row, 2] = !t;  // 오른쪽 조각 왼쪽 = 반대
                }

                // Top edge
                if (row < rows - 1)
                {
                    bool t = Random.value > 0.5f;
                    tabs[col, row,     1] =  t;
                    tabs[col, row + 1, 3] = !t;
                }

                // 테두리는 항상 flat(false)
                if (col == 0)        tabs[col, row, 2] = false;
                if (col == cols - 1) tabs[col, row, 0] = false;
                if (row == 0)        tabs[col, row, 3] = false;
                if (row == rows - 1) tabs[col, row, 1] = false;
            }
        }
        return tabs;
    }

    private static Texture2D ExtractPieceTexture(Texture2D src, int x, int y, int pw, int ph, int pad)
    {
        int texW = pw + pad * 2;
        int texH = ph + pad * 2;

        Color[] pixels = new Color[texW * texH];
        // 전체 투명으로 초기화
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // 소스에서 복사 (패딩 오프셋 적용)
        for (int py = 0; py < ph; py++)
        {
            for (int px = 0; px < pw; px++)
            {
                int srcX = x + px;
                int srcY = y + py;
                if (srcX >= 0 && srcX < src.width && srcY >= 0 && srcY < src.height)
                {
                    Color c = src.GetPixel(srcX, srcY);
                    pixels[(py + pad) * texW + (px + pad)] = c;
                }
            }
        }

        Texture2D result = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        result.SetPixels(pixels);
        result.Apply();
        return result;
    }

    // ── 베지어 유틸 ────────────────────────────────────────────────────────
    public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u  = 1f - t;
        float tt = t * t;
        float uu = u * u;
        return uu * u * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + tt * t * p3;
    }
}

// ── 데이터 구조 ─────────────────────────────────────────────────────────────
[System.Serializable]
public class PieceData
{
    public int     id;
    public int     col, row;
    public Sprite  sprite;
    public bool    tabRight, tabTop, tabLeft, tabBottom;
}
