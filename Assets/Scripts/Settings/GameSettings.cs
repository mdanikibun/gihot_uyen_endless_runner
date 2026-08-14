using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Uyen Runner/Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Gameplay")]
    public GameplaySettings gameplay = new GameplaySettings();

    [Header("Level")]
    public LevelSettings level = new LevelSettings();

    [Header("Segment")]
    public SegmentSettings segment = new SegmentSettings();

    [Header("Player")]
    public PlayerSettings player = new PlayerSettings();

    [Header("Player Collision")]
    public PlayerCollisionSettings playerCollision = new PlayerCollisionSettings();

    [Header("Camera")]
    public CameraSettings camera = new CameraSettings();

    [Header("Obstacle Spawn")]
    public ObstacleSpawnSettings obstacleSpawn = new ObstacleSpawnSettings();

    [Header("Pickup")]
    public PickupSettings pickup = new PickupSettings();

    [Header("Coin")]
    public CoinSettings coin = new CoinSettings();

    [Header("Power Up")]
    public PowerUpSettings powerUp = new PowerUpSettings();

    [Header("Character Select")]
    public CharacterSelectSettings characterSelect = new CharacterSelectSettings();

    [Header("Leaderboard")]
    public LeaderboardSettings leaderboard = new LeaderboardSettings();

    [Header("Music")]
    public MusicSettings music = new MusicSettings();

    [Header("SFX")]
    public SfxSettings sfx = new SfxSettings();
}

[Serializable]
public class GameplaySettings
{
    public int startingHealth = 5;
    public int damageAmount = 1;
    public float gameOverSlowMoDuration = 10f;
}

[Serializable]
public class LevelSettings
{
    public int segmentCount = 12;
    public int segmentGateInterval = 8;
    public float segmentLength = 10f;
    public float speedDefault = 10f;
    public float minMoveSpeed = 2f;
    public float maxMoveSpeed = 20f;
    public float minGravityZ = -22f;
    public float maxGravityZ = -2f;
    public float buffDuration = 5f;
    public float stumbleDuration = 1f;
    public int segmentPoolSize = 10;
    public int gatePoolSize = 4;
    public int startingPoolSize = 2;
}

[Serializable]
public class SegmentSettings
{
    public float powerUpItemSpawnChance = 0.3f;
    public float coinSpawnChance = 0.5f;
    public float coinSpacing = 2f;
    public float[] lanes = { -3f, 0f, 3f };
    public int fencePoolSize = 30;
    public int coinPoolSize = 100;
    public int powerUpPoolSize = 15;
}

[Serializable]
public class PlayerSettings
{
    public Vector3 startPosition = new Vector3(0f, 1.2f, 0f);
    public float speedMoveLeftRight = 10f;
    public float jumpForce = 4f;
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.12f;
}

[Serializable]
public class PlayerCollisionSettings
{
    public float collisionCooldown = 1f;
    public float speedChangeAmount = -1f;
}

[Serializable]
public class CameraSettings
{
    public float minFOV = 35f;
    public float maxFOV = 85f;
    public float zoomDuration = 1f;
    public float zoomSpeed = 5f;
}

[Serializable]
public class ObstacleSpawnSettings
{
    public float spawnTime = 3f;
    public float spawnWidth = 4f;
    public int poolSize = 8;
}

[Serializable]
public class PickupSettings
{
    public float rotationSpeed = 100f;
}

[Serializable]
public class CoinSettings
{
    public int scoreAmount = 10;
}

[Serializable]
public class PowerUpSettings
{
    public float speedChangeAmount = 2f;
    public float runAnimSpeedMax = 10f;
}

[Serializable]
public class CharacterSelectSettings
{
    public Color normalSlotColor = Color.white;
    public Color selectedSlotColor = new Color(1f, 0.85f, 0.2f, 1f);
}

[Serializable]
public class LeaderboardSettings
{
    public int maxEntries = 20;
    public string fileName = "leaderboard.json";
}

[Serializable]
public class MusicSettings
{
    [Range(0f, 1f)] public float menuVolume = 0.75f;
    [Range(0f, 1f)] public float gameplayVolume = 1f;
    public float fadeDuration = 0.35f;
}

[Serializable]
public class SfxSettings
{
    [Range(0f, 1f)] public float hitVolume = 1f;
    [Range(0f, 1f)] public float jumpVolume = 1f;
    [Range(0f, 1f)] public float dieVolume = 1f;
    [Range(0f, 1f)] public float coinVolume = 1f;
    [Range(0f, 1f)] public float powerUpVolume = 1f;
}
