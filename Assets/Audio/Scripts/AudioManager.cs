using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Audio References")] [SerializeField]
    private AudioMixer mixer;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")] [SerializeField]
    private AudioClip[] musicClips;

    [SerializeField] private AudioClip[] sfxClips;
    [SerializeField] private AudioClip buttonClickSfx;

    [Header("Slider References")] 
    [SerializeField] [CanBeNull] private UnityEngine.UI.Slider masterVolumeSlider;
    [SerializeField] [CanBeNull] private UnityEngine.UI.Slider musicVolumeSlider;
    [SerializeField] [CanBeNull] private UnityEngine.UI.Slider sfxVolumeSlider;

    public void SetMasterVolume()
    {
        if (masterVolumeSlider.value <= 0)
            mixer.SetFloat("MasterVolume", -80f); // Assign a very low value for silence
        else
            mixer.SetFloat("MasterVolume", Mathf.Log10(masterVolumeSlider.value) * 20);
    }


    public void SetMusicVolume()
    {
        if (musicVolumeSlider.value <= 0)
            mixer.SetFloat("MusicVolume", -80f); // Assign a very low value for silence
        else
            mixer.SetFloat("MusicVolume", Mathf.Log10(musicVolumeSlider.value) * 20);
    }

    public void SetSfxVolume()
    {
        if (sfxVolumeSlider.value <= 0)
            mixer.SetFloat("SFXVolume", -80f); // Assign a very low value for silence
        else
            mixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolumeSlider.value) * 20);
    }

    /// <param name="index">0 = Main Menu, 1 = Level 1, 2 = Level 2, etc.</param>
    public void PlayMusic(int index)
    {
        if (index < 0 || index >= musicClips.Length) return;
        musicSource.clip = musicClips[index];
        musicSource.Play();
    }

    public void PlaySfx(int index)
    {
        if (index < 0 || index >= sfxClips.Length) return;
        sfxSource.PlayOneShot(sfxClips[index]);
    }

    // For unity events, we can have specific methods for each music and sfx to avoid needing to pass indices
    public void PlayButtonClickSfx() => sfxSource.PlayOneShot(buttonClickSfx);

    public void PlayMainMenuMusic() => PlayMusic(0);
    public void PlayLevel1Music() => PlayMusic(1);
    public void PlayLevel2Music() => PlayMusic(2);
    public void PlayLevel3Music() => PlayMusic(3);

    public void PlayCastSfx() => PlaySfx(0);
    public void PlaySpellHitSfx() => PlaySfx(1);
    public void PlayCastFailedSfx() => PlaySfx(2);
    public void PlayPlayerHitSfx() => PlaySfx(3);
}