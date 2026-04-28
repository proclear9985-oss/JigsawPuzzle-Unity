using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// 런타임에 로컬 파일 또는 URL 에서 이미지를 로드합니다.
/// Resources 폴더 외에 사용자가 커스텀 이미지를 추가할 때 사용합니다.
/// </summary>
public class ImageLoader : MonoBehaviour
{
    public static ImageLoader Instance { get; private set; }

    [Header("Supported Formats")]
    [Tooltip("지원하는 확장자 목록")]
    public string[] supportedExtensions = { ".png", ".jpg", ".jpeg" };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 파일 경로에서 로드 ────────────────────────────────────────────────
    /// <summary>로컬 파일 경로에서 Texture2D 로드 (동기)</summary>
    public Texture2D LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[ImageLoader] 파일 없음: {path}");
            return null;
        }

        byte[] data = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (tex.LoadImage(data))
        {
            tex.name = Path.GetFileNameWithoutExtension(path);
            return tex;
        }

        Debug.LogError($"[ImageLoader] 이미지 로드 실패: {path}");
        return null;
    }

    // ── URL 에서 로드 (코루틴) ────────────────────────────────────────────
    /// <summary>HTTP URL 에서 Texture2D 비동기 로드</summary>
    public IEnumerator LoadFromURL(string url, System.Action<Texture2D> callback)
    {
        using var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            var tex  = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
            tex.name = Path.GetFileNameWithoutExtension(url);
            callback?.Invoke(tex);
        }
        else
        {
            Debug.LogError($"[ImageLoader] URL 로드 실패: {url} → {req.error}");
            callback?.Invoke(null);
        }
    }

    // ── 지원 여부 확인 ────────────────────────────────────────────────────
    public bool IsSupportedFormat(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        foreach (var s in supportedExtensions)
            if (ext == s) return true;
        return false;
    }

    // ── StreamingAssets 에서 로드 ─────────────────────────────────────────
    public IEnumerator LoadFromStreamingAssets(string fileName, System.Action<Texture2D> callback)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

#if UNITY_ANDROID
        // 안드로이드는 UnityWebRequest 필요
        yield return LoadFromURL("file://" + path, callback);
#else
        var tex = LoadFromFile(path);
        callback?.Invoke(tex);
        yield break;
#endif
    }

    // ── 텍스처 → PNG 바이트 저장 ─────────────────────────────────────────
    public static void SaveTextureToPNG(Texture2D tex, string savePath)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        Debug.Log($"[ImageLoader] PNG 저장: {savePath}");
    }
}
