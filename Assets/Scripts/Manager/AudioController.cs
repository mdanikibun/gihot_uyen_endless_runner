using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] GameSettings settings;

    [Header("Clips")]
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip dieClip;
    [SerializeField] AudioClip coinClip;
    [SerializeField] AudioClip powerUpClip;

    bool powerUpMutedByPause;
    float volumeBeforeMute = 1f;

    void Awake() {
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.ignoreListenerPause = true;
        audioSource.volume = 1f;
        volumeBeforeMute = 1f;
    }

    void OnEnable() {
        GameEvents.OnPlayerHitSFX += PlayHit;
        GameEvents.OnPlayerJumpSFX += PlayJump;
        GameEvents.OnGameOver += HandleGameOver;
        GameEvents.OnCoinCollectedSFX += PlayCoin;
        GameEvents.OnPowerUpStartedSFX += StartPowerUpLoop;
        GameEvents.OnPowerUpEndedSFX += StopPowerUpLoop;
        GameEvents.OnRunPrepared += StopPowerUpLoop;
        GameEvents.OnMusicReset += RestoreVolumeFromMute;
        GameEvents.OnMusicRelaxed += MutePowerUpForPause;
        GameEvents.OnMusicGameplay += RestorePowerUpAfterPause;
    }

    void OnDisable() {
        GameEvents.OnPlayerHitSFX -= PlayHit;
        GameEvents.OnPlayerJumpSFX -= PlayJump;
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnCoinCollectedSFX -= PlayCoin;
        GameEvents.OnPowerUpStartedSFX -= StartPowerUpLoop;
        GameEvents.OnPowerUpEndedSFX -= StopPowerUpLoop;
        GameEvents.OnRunPrepared -= StopPowerUpLoop;
        GameEvents.OnMusicReset -= RestoreVolumeFromMute;
        GameEvents.OnMusicRelaxed -= MutePowerUpForPause;
        GameEvents.OnMusicGameplay -= RestorePowerUpAfterPause;
    }

    void PlayHit() {
        PlayClip(hitClip, settings.sfx.hitVolume);
    }

    void PlayJump() {
        PlayClip(jumpClip, settings.sfx.jumpVolume);
    }

    void PlayDie() {
        PlayClip(dieClip, settings.sfx.dieVolume);
    }

    void PlayCoin() {
        PlayClip(coinClip, settings.sfx.coinVolume);
    }

    void HandleGameOver() {
        PlayDie();
        StopPowerUpLoop();
    }

    void StartPowerUpLoop() {
        powerUpMutedByPause = false;
        audioSource.volume = Mathf.Clamp01(settings.sfx.powerUpVolume);
        volumeBeforeMute = audioSource.volume;

        if (audioSource.isPlaying && audioSource.clip == powerUpClip) {
            return;
        }

        audioSource.clip = powerUpClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    void StopPowerUpLoop() {
        if (audioSource.isPlaying && audioSource.clip == powerUpClip) {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }

        RestoreVolumeFromMute();
    }

    void MutePowerUpForPause() {
        if (!IsPowerUpLoopPlaying()) return;

        volumeBeforeMute = audioSource.volume;
        powerUpMutedByPause = true;
        audioSource.volume = 0f;
    }

    void RestorePowerUpAfterPause() {
        if (!powerUpMutedByPause) return;

        powerUpMutedByPause = false;
        audioSource.volume = volumeBeforeMute;

        if (!IsPowerUpLoopPlaying()) return;
        // vẫn đang loop power-up sau resume
    }

    void RestoreVolumeFromMute() {
        if (!powerUpMutedByPause) return;

        powerUpMutedByPause = false;
        audioSource.volume = volumeBeforeMute > 0f ? volumeBeforeMute : 1f;
    }

    bool IsPowerUpLoopPlaying() {
        return audioSource != null
            && audioSource.isPlaying
            && audioSource.clip == powerUpClip;
    }

    void PlayClip(AudioClip clip, float volume) {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
