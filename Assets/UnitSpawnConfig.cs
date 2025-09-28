using UnityEngine;

[CreateAssetMenu(fileName = "SpawnConfig", menuName = "Configs/Spawn Config")]
public class SpawnConfig : ScriptableObject
{
    [System.Serializable]
    public struct SpawnEntry
    {
        public UnitConfig config;       // Reference to the unit type
        public Vector2Int startPos;     // Starting position on the grid
    }

    public SpawnEntry[] spawnEntries;
}
