using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 메인 메뉴 갤러리의 개별 썸네일 아이템입니다.
/// Prefab으로 만들어 GridLayoutGroup 안에 배치합니다.
/// </summary>
public class ThumbnailItem : MonoBehaviour
{
    [Header("References")]
    public RawImage            thumbnail;
    public TextMeshProUGUI     nameLabel;
    public Button              selectButton;
    public GameObject          selectedBorder;   // 선택 시 강조 테두리

    private int       _index;
    private System.Action<int> _onSelect;

    // ── 초기화 ────────────────────────────────────────────────────────────
    public void Setup(int index, Texture2D tex, string imageName, System.Action<int> onSelect)
    {
        _index    = index;
        _onSelect = onSelect;

        if (thumbnail  != null) thumbnail.texture = tex;
        if (nameLabel  != null) nameLabel.text     = imageName;

        selectButton?.onClick.RemoveAllListeners();
        selectButton?.onClick.AddListener(OnClicked);

        SetSelected(false);
    }

    // ── 선택 상태 표시 ────────────────────────────────────────────────────
    public void SetSelected(bool selected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(selected);

        if (thumbnail != null)
        {
            var color       = thumbnail.color;
            color.a         = selected ? 1f : 0.85f;
            thumbnail.color = color;
        }
    }

    // ── 클릭 ──────────────────────────────────────────────────────────────
    private void OnClicked() => _onSelect?.Invoke(_index);
}
