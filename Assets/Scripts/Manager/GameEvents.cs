using System;

public static class GameEvents
{
    public static event Action<float> OnDistanceChanged;
    public static event Action<float> OnSpeedUpCountdownChanged;
    public static event Action<int> OnScoreChanged;
    public static event Action<int, int> OnHealthChanged;
    public static event Action OnGameOver;
    public static event Action OnRunPrepared;

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
}
