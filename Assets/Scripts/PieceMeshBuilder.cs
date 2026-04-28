using UnityEngine;

/// <summary>
/// 퍼즐 조각의 외곽선을 베지어 곡선 기반의 탭/소켓 형태로 생성하는 Mesh 빌더.
/// MeshFilter + MeshRenderer 가 있는 조각 GameObject에서 호출합니다.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PieceMeshBuilder : MonoBehaviour
{
    [Header("Bezier Settings")]
    public int   bezierSteps = 12;    // 베지어 곡선 분할 수 (높을수록 부드러움)
    public float tabSize     = 0.25f; // 탭 크기 (조각 한 변 대비 비율)
    public float tabDepth    = 0.15f; // 탭 깊이 비율

    /// <summary>
    /// 주어진 PieceData 와 월드 크기를 바탕으로 조각 Mesh를 생성합니다.
    /// </summary>
    public void Build(PieceData data, float worldSize, Texture2D sourceImage,
                      int totalCols, int totalRows)
    {
        float hw = worldSize * 0.5f;  // half width
        float hh = worldSize * 0.5f;  // half height

        // 탭 방향 (true = 볼록, false = 오목)
        bool tRight  = data.tabRight;
        bool tTop    = data.tabTop;
        bool tLeft   = data.tabLeft;
        bool tBottom = data.tabBottom;

        var verts  = new System.Collections.Generic.List<Vector3>();
        var uvs    = new System.Collections.Generic.List<Vector2>();
        var tris   = new System.Collections.Generic.List<int>();

        // 외곽 경계선 포인트 생성 (시계 반대 방향: Bottom → Right → Top → Left)
        var outline = new System.Collections.Generic.List<Vector2>();

        // Bottom edge (Left→Right, tabBottom)
        AddEdge(outline, new Vector2(-hw, -hh), new Vector2(hw, -hh),
                new Vector2(0, -1), tBottom);

        // Right edge (Bottom→Top, tabRight)
        AddEdge(outline, new Vector2(hw, -hh), new Vector2(hw, hh),
                new Vector2(1, 0), tRight);

        // Top edge (Right→Left, tabTop)
        AddEdge(outline, new Vector2(hw, hh), new Vector2(-hw, hh),
                new Vector2(0, 1), tTop);

        // Left edge (Top→Bottom, tabLeft)
        AddEdge(outline, new Vector2(-hw, hh), new Vector2(-hw, -hh),
                new Vector2(-1, 0), tLeft);

        // UV 계산 (0~1 범위)
        float uvOffX = (float)data.col / totalCols;
        float uvOffY = (float)data.row / totalRows;
        float uvW    = 1f / totalCols;
        float uvH    = 1f / totalRows;

        // 중심 삼각형 팬(Fan) 방식으로 Mesh 생성
        int centerIdx = outline.Count;
        foreach (var p in outline)
        {
            verts.Add(new Vector3(p.x, p.y, 0f));
            // UV: 로컬 좌표 → 텍스처 좌표
            float u = uvOffX + (p.x / worldSize + 0.5f) * uvW;
            float v = uvOffY + (p.y / worldSize + 0.5f) * uvH;
            uvs.Add(new Vector2(u, v));
        }

        // 중심 정점
        verts.Add(Vector3.zero);
        uvs.Add(new Vector2(uvOffX + uvW * 0.5f, uvOffY + uvH * 0.5f));

        // 삼각형 팬
        for (int i = 0; i < outline.Count; i++)
        {
            int next = (i + 1) % outline.Count;
            tris.Add(centerIdx);
            tris.Add(i);
            tris.Add(next);
        }

        // Mesh 적용
        var mesh        = new Mesh();
        mesh.vertices   = verts.ToArray();
        mesh.uv         = uvs.ToArray();
        mesh.triangles  = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;

        // 텍스처 적용
        var mat      = GetComponent<MeshRenderer>().material;
        mat.mainTexture = sourceImage;
    }

    // ── 엣지 생성 (베지어 탭/소켓) ───────────────────────────────────────
    private void AddEdge(System.Collections.Generic.List<Vector2> pts,
                         Vector2 from, Vector2 to, Vector2 normal, bool isTab)
    {
        Vector2 dir    = (to - from);
        float   len    = dir.magnitude;
        Vector2 dirN   = dir.normalized;
        float   depth  = len * tabDepth * (isTab ? 1f : -1f);

        // 탭 위치: 엣지 중앙 1/3 ~ 2/3 구간
        Vector2 t0 = from + dirN * len * 0.35f;
        Vector2 t1 = from + dirN * len * 0.65f;
        Vector2 mid = (from + to) * 0.5f + normal * depth;

        // 직선 → 탭 시작 (베지어)
        AddBezierCurve(pts, from, t0, t0, t0);
        AddBezierCurve(pts, t0, t0, mid, t1);
        AddBezierCurve(pts, t1, mid, t1, to);
    }

    private void AddBezierCurve(System.Collections.Generic.List<Vector2> pts,
                                Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        for (int i = 1; i <= bezierSteps; i++)
        {
            float t = i / (float)bezierSteps;
            pts.Add(ImageSlicer.CubicBezier(p0, p1, p2, p3, t));
        }
    }
}
