using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 여러 퍼즐 조각을 하나의 그룹으로 묶어 함께 이동/회전시킵니다.
/// </summary>
public class PieceGroup : MonoBehaviour
{
    private List<PuzzlePiece> _members = new();

    // ── 정적 팩토리 ───────────────────────────────────────────────────────
    /// <summary>두 조각을 같은 그룹으로 합칩니다.</summary>
    public static void MergeOrCreate(PuzzlePiece a, PuzzlePiece b)
    {
        if (a.Group != null && b.Group != null)
        {
            if (a.Group == b.Group) return;          // 이미 같은 그룹
            a.Group.AbsorbGroup(b.Group);            // 두 그룹 병합
        }
        else if (a.Group != null)
        {
            a.Group.AddPiece(b);
        }
        else if (b.Group != null)
        {
            b.Group.AddPiece(a);
        }
        else
        {
            // 새 그룹 생성
            GameObject go    = new GameObject("PieceGroup");
            var        group = go.AddComponent<PieceGroup>();
            group.AddPiece(a);
            group.AddPiece(b);
        }
    }

    // ── 멤버 관리 ─────────────────────────────────────────────────────────
    public void AddPiece(PuzzlePiece piece)
    {
        if (_members.Contains(piece)) return;
        if (piece.Group != null && piece.Group != this)
            piece.Group.RemovePiece(piece);

        piece.Group = this;
        _members.Add(piece);
    }

    public void RemovePiece(PuzzlePiece piece)
    {
        _members.Remove(piece);
        if (piece.Group == this) piece.Group = null;
        if (_members.Count == 0) Destroy(gameObject);
    }

    private void AbsorbGroup(PieceGroup other)
    {
        var toMove = new List<PuzzlePiece>(other._members);
        foreach (var p in toMove) AddPiece(p);
        Destroy(other.gameObject);
    }

    // ── 이동 ──────────────────────────────────────────────────────────────
    public void Translate(Vector3 delta)
    {
        foreach (var p in _members)
            p.transform.position += delta;
    }

    // ── 회전 ──────────────────────────────────────────────────────────────
    public void Rotate(float angle)
    {
        // 그룹 중심 기준 회전
        Vector3 center = GetGroupCenter();
        foreach (var p in _members)
        {
            p.transform.RotateAround(center, Vector3.forward, angle);
        }
    }

    // ── Z-Order ───────────────────────────────────────────────────────────
    public void BringToFront(int order)
    {
        foreach (var p in _members)
            p.GetComponent<SpriteRenderer>().sortingOrder = order;
    }

    public void ResetOrder(int order)
    {
        foreach (var p in _members)
            p.GetComponent<SpriteRenderer>().sortingOrder = order;
    }

    // ── 스냅 ──────────────────────────────────────────────────────────────
    public void TrySnapAll()
    {
        foreach (var p in new List<PuzzlePiece>(_members))
            p.TrySnap();
    }

    // ── 유틸 ──────────────────────────────────────────────────────────────
    private Vector3 GetGroupCenter()
    {
        if (_members.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        foreach (var p in _members) sum += p.transform.position;
        return sum / _members.Count;
    }

    public List<PuzzlePiece> GetMembers() => new List<PuzzlePiece>(_members);
}
