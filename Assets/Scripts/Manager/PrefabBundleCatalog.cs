using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PrefabBundleCatalog",
    menuName = "Custom AssetBundle/Prefab Bundle Catalog"
)]
public class PrefabBundleCatalog : ScriptableObject
{
    [Header("Players")]
    public GameObject[] players;

    [Header("Segments")]
    public GameObject[] startingSegments;
    public GameObject[] roadSegments;
    public GameObject gate;

    [Header("Obstacles")]
    public GameObject[] obstacles;

    [Header("Segment Items")]
    public GameObject fence;
    public GameObject coin;
    public GameObject powerUp;

    public IEnumerable<GameObject> GetAllPrefabs() {
        foreach (GameObject go in AllFrom(players)) yield return go;
        foreach (GameObject go in AllFrom(startingSegments)) yield return go;
        foreach (GameObject go in AllFrom(roadSegments)) yield return go;
        if (gate != null) yield return gate;
        foreach (GameObject go in AllFrom(obstacles)) yield return go;
        if (fence != null) yield return fence;
        if (coin != null) yield return coin;
        if (powerUp != null) yield return powerUp;
    }

    static IEnumerable<GameObject> AllFrom(GameObject[] entries) {
        if (entries == null) yield break;
        for (int i = 0; i < entries.Length; i++) {
            if (entries[i] != null) {
                yield return entries[i];
            }
        }
    }
}
