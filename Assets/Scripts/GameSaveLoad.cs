using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 퍼즐 진행 상태를 JSON 파일로 저장/불러오기합니다.
/// </summary>
public static class GameSaveLoad
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "puzzle_save.json");

    // ── 저장 ──────────────────────────────────────────────────────────────
    public static void Save(PuzzleManager pm, Dictionary<int, PuzzlePiece> pieces)
    {
        var saveData = new SaveData
        {
            imageName   = GameManager.Instance.selectedImageName,
            columns     = GameManager.Instance.columns,
            rows        = GameManager.Instance.rows,
            elapsedTime = GameManager.Instance.elapsedTime,
            pieces      = new List<PieceState>()
        };

        foreach (var kv in pieces)
            saveData.pieces.Add(kv.Value.GetState());

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[GameSaveLoad] 저장 완료 → {SavePath}");
    }

    // ── 불러오기 ──────────────────────────────────────────────────────────
    public static void Load(PuzzleManager pm, Dictionary<int, PuzzlePiece> pieces)
    {
        if (!HasSaveData()) return;

        string   json     = File.ReadAllText(SavePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        GameManager.Instance.elapsedTime = saveData.elapsedTime;

        foreach (var state in saveData.pieces)
        {
            if (pieces.TryGetValue(state.id, out var piece))
                piece.ApplyState(state);
        }

        Debug.Log($"[GameSaveLoad] 불러오기 완료 ← {SavePath}");
    }

    // ── 유틸 ──────────────────────────────────────────────────────────────
    public static bool HasSaveData() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[GameSaveLoad] 저장 파일 삭제");
        }
    }

    public static SaveData PeekSaveData()
    {
        if (!HasSaveData()) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
    }
}

// ── 저장 데이터 구조 ──────────────────────────────────────────────────────────
[System.Serializable]
public class SaveData
{
    public string          imageName;
    public int             columns;
    public int             rows;
    public float           elapsedTime;
    public List<PieceState> pieces;
}
