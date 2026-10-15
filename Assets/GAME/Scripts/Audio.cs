using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource rainSource; // Отдельный источник для дождя
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private List<AudioClip> BgMusics; // [0] - Музыка, [1] - Дождь, [2...] - Эмбиент
    public AudioClip rainClip; // Можно назначить напрямую в инспекторе, если хотите

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ValidateAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ValidateAudioSources()
    {
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (ambientSource == null) ambientSource = gameObject.AddComponent<AudioSource>();
        if (rainSource == null) rainSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        ambientSource.loop = true;
        rainSource.loop = true; // Дождь тоже должен быть зациклен
    }

    private void Start()
    {
        // Запуск музыки по умолчанию (если список не пустой)
        if (BgMusics != null && BgMusics.Count > 0)
        {
            PlayMusic(BgMusics[0], 0.3f);
        }

        // Запуск остальных фоновых звуков (если они есть в списке, начиная с 2-го индекса)
        if (BgMusics != null && BgMusics.Count > 1)
        {
            for (int i = 1; i < BgMusics.Count; i++)
            {
                PlayAmbient(BgMusics[i], 0.4f);
            }
        }
    }

    // --- УПРАВЛЕНИЕ МУЗЫКОЙ ---
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null || (musicSource.clip == clip && musicSource.isPlaying)) return;
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();
    public void SetMusicVolume(float volume) => musicSource.volume = volume;


    // --- УПРАВЛЕНИЕ ДОЖДЕМ ---
    public void PlayRain(AudioClip clip, float volume = 1f)
    {
        if (clip == null || (rainSource.clip == clip && rainSource.isPlaying)) return;
        rainSource.clip = clip;
        rainSource.volume = volume;
        rainSource.Play();
    }

    public void StopRain() => rainSource.Stop();
    public void SetRainVolume(float volume) => rainSource.volume = volume;


    // --- УПРАВЛЕНИЕ ЭМБИЕНТОМ ---
    public void PlayAmbient(AudioClip clip, float volume = 1f)
    {
        if (clip == null || (ambientSource.clip == clip && ambientSource.isPlaying)) return;
        ambientSource.clip = clip;
        ambientSource.volume = volume;
        ambientSource.Play();
    }

    public void StopAmbient() => ambientSource.Stop();
    public void SetAmbientVolume(float volume) => ambientSource.volume = volume;


    // --- УПРАВЛЕНИЕ SFX ---
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
}