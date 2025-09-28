using UnityEngine;

[CreateAssetMenu(fileName = "UnitConfig", menuName = "Configs/Unit Config")]
public class UnitConfig : ScriptableObject
{
    public string unitName = "Infantry Squad";
    public GameObject unitPrefab;
    public Faction faction;
    public DoctrineTag doctrineTag = DoctrineTag.None;
    public int rationCost = 2;
    public int maxModels = 5;
    public int movementSpeed = 2;
    public int maxActionsPerTurn = 2;
    public int attackRange = 3;
    [Range(0f, 1f)] public float hitChance = 0.5f;
    [Range(0f, 1f)] public float suppressionChance = 0.3f;
}
