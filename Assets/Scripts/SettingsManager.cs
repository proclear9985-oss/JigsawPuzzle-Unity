using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 설정 패널 – BGM/SFX 볼륨, 그래픽 품질 등을 조절합니다.
/// 메인 메뉴 및 인게임 일시정지 메뉴에서 공용으로 사용됩니다.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject settingsPanel;

    [Header("Audio")]
    public Slider           bgmSlider;
    public Slider           sfxSlider;
    public TextMeshProUGUI  bgmValueText;
    public TextMeshProUGUI  sfxValueText;

    [Header("Graphics")]
    public Toggle           vSyncToggle;
    public TMP_Dropdown     qualityDropdown;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadSettings();
        BindUI();
        settingsPanel?.SetActive(false);
    }

    // ── UI 바인딩 ─────────────────────────────────────────────────────────
    private void BindUI()
    {
        bgmSlider?.onValueChanged.AddListener(v => {
            AudioManager.Instance?.SetBGMVolume(v);
            if (bgmValueText != null) bgmValueText.text = Mathf.RoundToInt(v * 100) + "%";
        });

        sfxSlider?.onValueChanged.AddListener(v => {
            AudioManager.Instance?.SetSFXVolume(v);
            if (sfxValueText != null) sfxValueText.text = Mathf.RoundToInt(v * 100) + "%";
        });

        vSyncToggle?.onValueChanged.AddListener(v => {
            QualitySettings.vSyncCount = v ? 1 : 0;
            PlayerPrefs.SetInt("VSync", v ? 1 : 0);
        });

        qualityDropdown?.onValueChanged.AddListener(v => {
            QualitySettings.SetQualityLevel(v);
            PlayerPrefs.SetInt("QualityLevel", v);
        });
    }

    // ── 저장/불러오기 ─────────────────────────────────────────────────────
    private void LoadSettings()
    {
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        int   vsync   = PlayerPrefs.GetInt("VSync", 1);
        int   quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());

        if (bgmSlider != null) { bgmSlider.value = bgm; }
        if (sfxSlider != null) { sfxSlider.value = sfx; }
        if (vSyncToggle    != null) vSyncToggle.isOn = vsync == 1;
        if (qualityDropdown != null) qualityDropdown.value = quality;

        QualitySettings.vSyncCount = vsync;
        QualitySettings.SetQualityLevel(quality);
    }

    // ── 패널 토글 ─────────────────────────────────────────────────────────
    public void OpenSettings()  => settingsPanel?.SetActive(true);
    public void CloseSettings()
    {
        settingsPanel?.SetActive(false);
        PlayerPrefs.Save();
    }
}
