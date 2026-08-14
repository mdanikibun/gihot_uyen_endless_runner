using System;

public static class GameEvents
{
    public static event Action<float> OnDistanceChanged;
    public static event Action<float> OnSpeedUpCountdownChanged;
    public static event Action<int> OnScoreChanged;
    public static event Action<int, int> OnHealthChanged;
    public static event Action OnGameOver;
    public static event Action OnRunPrepared;
    public static event Action OnMusicGameplay;
    public static event Action OnMusicRelaxed;
    public static event Action OnMusicReset;
    public static event Action OnPlayerHitSFX;
    public static event Action OnPlayerJumpSFX;
    public static event Action OnCoinCollectedSFX;
    public static event Action OnPowerUpStartedSFX;
    public static event Action OnPowerUpEndedSFX;

    public static void RaiseDistanceChanged(float distance) {
        OnDistanceChanged?.Invoke(distance);
    }

    public static void RaiseSpeedUpCountdownChanged(float countdown) {
        OnSpeedUpCountdownChanged?.Invoke(countdown);
    }

    public static void RaiseScoreChanged(int score) {
        OnScoreChanged?.Invoke(score);
    }

    public static void RaiseHealthChanged(int currentHealth, int maxHealth) {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public static void RaiseGameOver() {
        OnGameOver?.Invoke();
    }

    public static void RaiseRunPrepared() {
        OnRunPrepared?.Invoke();
    }

    public static void RaiseMusicGameplay() {
        OnMusicGameplay?.Invoke();
    }

    public static void RaiseMusicRelaxed() {
        OnMusicRelaxed?.Invoke();
    }

    public static void RaiseMusicReset() {
        OnMusicReset?.Invoke();
    }

    public static void RaisePlayerHitSFX() {
        OnPlayerHitSFX?.Invoke();
    }

    public static void RaisePlayerJumpSFX() {
        OnPlayerJumpSFX?.Invoke();
    }

    public static void RaiseCoinCollectedSFX() {
        OnCoinCollectedSFX?.Invoke();
    }

    public static void RaisePowerUpStartedSFX() {
        OnPowerUpStartedSFX?.Invoke();
    }

    public static void RaisePowerUpEndedSFX() {
        OnPowerUpEndedSFX?.Invoke();
    }
}
