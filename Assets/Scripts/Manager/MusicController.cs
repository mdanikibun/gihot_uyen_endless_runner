using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] GameSettings settings;

    Coroutine fadeCoroutine;

    void Awake() {
        audioSource.ignoreListenerPause = true;
        ApplyVolumeImmediate(settings.music.menuVolume);
        EnsurePlaying();
    }

    void OnEnable() {
        GameEvents.OnMusicGameplay += HandleGameplay;
        GameEvents.OnMusicRelaxed += HandleRelaxed;
        GameEvents.OnMusicReset += HandleReset;
        GameEvents.OnGameOver += HandleRelaxed;
    }

    void OnDisable() {
        GameEvents.OnMusicGameplay -= HandleGameplay;
        GameEvents.OnMusicRelaxed -= HandleRelaxed;
        GameEvents.OnMusicReset -= HandleReset;
        GameEvents.OnGameOver -= HandleRelaxed;
    }

    void HandleGameplay() {
        EnsurePlaying();
        FadeTo(settings.music.gameplayVolume);
    }

    void HandleRelaxed() {
        EnsurePlaying();
        FadeTo(settings.music.menuVolume);
    }

    void HandleReset() {
        StopFade();
        audioSource.Stop();
        audioSource.time = 0f;
        ApplyVolumeImmediate(settings.music.menuVolume);
        audioSource.Play();
    }

    void FadeTo(float targetVolume) {
        StopFade();
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(targetVolume));
    }

    void StopFade() {
        if (fadeCoroutine == null) return;
        StopCoroutine(fadeCoroutine);
        fadeCoroutine = null;
    }

    IEnumerator FadeVolumeRoutine(float targetVolume) {
        float startVolume = audioSource.volume;
        float duration = Mathf.Max(0f, settings.music.fadeDuration);

        if (duration <= 0f) {
            ApplyVolumeImmediate(targetVolume);
            fadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        ApplyVolumeImmediate(targetVolume);
        fadeCoroutine = null;
    }

    void ApplyVolumeImmediate(float volume) {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    void EnsurePlaying() {
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }
}
