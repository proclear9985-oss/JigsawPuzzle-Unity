using UnityEngine;

/// <summary>
/// 오디오 재생을 담당하는 싱글톤 매니저
/// BGM, SFX(스냅 효과음, 버튼 클릭 등)를 관리합니다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip gameSceneBGM;

    [Header("SFX Clips")]
    public AudioClip snapSound;       // 조각 스냅 효과음
    public AudioClip pickupSound;     // 조각 집어들기 효과음
    public AudioClip clearSound;      // 퍼즐 완성 효과음
    public AudioClip buttonClickSound;// 버튼 클릭 효과음

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    // ── 생명주기 ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ApplyVolume();
    }

    // ── BGM ───────────────────────────────────────────────────────────────
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource?.Stop();

    public void PlayMainMenuBGM() => PlayBGM(mainMenuBGM);
    public void PlayGameBGM()     => PlayBGM(gameSceneBGM);

    // ── SFX ───────────────────────────────────────────────────────────────
    public void PlaySnap()        => PlaySFX(snapSound);
    public void PlayPickup()      => PlaySFX(pickupSound);
    public void PlayClear()       => PlaySFX(clearSound);
    public void PlayButtonClick() => PlaySFX(buttonClickSound);

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ── 볼륨 설정 ─────────────────────────────────────────────────────────
    public void SetBGMVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    private void ApplyVolume()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
}
