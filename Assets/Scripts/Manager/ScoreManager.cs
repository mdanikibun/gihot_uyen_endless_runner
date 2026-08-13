using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

[Serializable]
public class LeaderboardEntry
{
    public string name;
    public float distance;
    public int score;
}

[Serializable]
public class LeaderboardSaveData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameSettings settings;

    int score;
    LeaderboardSaveData saveData = new LeaderboardSaveData();

    public int Score => score;

    string FilePath => Path.Combine(Application.persistentDataPath, settings.leaderboard.fileName);

    void Awake() {
        Load();
    }

    void OnEnable() {
        GameEvents.OnScoreChanged += HandleScoreChanged;
    }

    void OnDisable() {
        GameEvents.OnScoreChanged -= HandleScoreChanged;
    }

    void Start() {
        GameEvents.RaiseScoreChanged(score);
    }

    public void AddScore(int amount) {
        if (gameManager.IsGameOver) return;

        score += amount;
        GameEvents.RaiseScoreChanged(score);
    }

    public void ResetScore() {
        score = 0;
        GameEvents.RaiseScoreChanged(score);
    }

    void HandleScoreChanged(int newScore) {
        scoreText.text = newScore.ToString();
    }

    public void AddLeaderboardEntry(string playerName, float distance, int entryScore) {
        saveData.entries.Add(new LeaderboardEntry {
            name = playerName,
            distance = distance,
            score = entryScore
        });

        saveData.entries = saveData.entries
            .OrderByDescending(e => e.score)
            .ThenByDescending(e => e.distance)
            .Take(settings.leaderboard.maxEntries)
            .ToList();

        Save();
    }

    public List<LeaderboardEntry> GetScores() {
        return saveData.entries;
    }

    void Save() {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(FilePath, json);
    }

    void Load() {
        if (!File.Exists(FilePath)) {
            saveData = new LeaderboardSaveData();
            return;
        }

        ParseJson(File.ReadAllText(FilePath));
    }

    void ParseJson(string json) {
        if (string.IsNullOrWhiteSpace(json)) {
            saveData = new LeaderboardSaveData();
            return;
        }

        saveData = JsonUtility.FromJson<LeaderboardSaveData>(json);
        if (saveData == null || saveData.entries == null) {
            saveData = new LeaderboardSaveData();
        }
    }
}
