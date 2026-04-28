using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 퍼즐 완성 시 축하 이펙트 (파티클 + UI 애니메이션) 담당
/// ClearPanel 하위 오브젝트에 부착합니다.
/// </summary>
public class ClearEffect : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup     canvasGroup;
    public RectTransform   resultPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI pieceText;
    public ParticleSystem  confetti;

    [Header("Animation Settings")]
    public float fadeDuration  = 0.5f;
    public float scaleDuration = 0.4f;
    public float startDelay    = 0.3f;

    // ── 공개 메서드 ───────────────────────────────────────────────────────
    public void PlayClearAnimation(float elapsedTime, int totalPieces)
    {
        gameObject.SetActive(true);

        if (timeText  != null) timeText.text  = $"완성 시간  {GameManager.FormatTime(elapsedTime)}";
        if (pieceText != null) pieceText.text = $"조각 수  {totalPieces} 개";

        StartCoroutine(AnimateIn());
        confetti?.Play();
    }

    // ── 애니메이션 코루틴 ─────────────────────────────────────────────────
    private IEnumerator AnimateIn()
    {
        // 초기 상태
        if (canvasGroup  != null) canvasGroup.alpha    = 0f;
        if (resultPanel  != null) resultPanel.localScale = Vector3.one * 0.5f;

        yield return new WaitForSecondsRealtime(startDelay);

        // 페이드 인
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        // 스케일 팝
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / scaleDuration;
            float s = Mathf.LerpUnclamped(0.5f, 1f, EaseOutBack(t));
            if (resultPanel != null) resultPanel.localScale = Vector3.one * s;
            yield return null;
        }

        if (resultPanel != null) resultPanel.localScale = Vector3.one;
    }

    // ── Ease ──────────────────────────────────────────────────────────────
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
