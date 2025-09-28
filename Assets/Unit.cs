using System;
using CM_Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Faction { Poland, Germany }
public enum DoctrineTag { None, MGCompany, SapperCompany, BorderGuard, Wehrmacht, Irregulars, SD }

public class Unit : MonoBehaviour
{
    public enum SuppressionLevel { Normal, Stressed, Suppressed }

    UnitConfig _config;

    [Header("State")]
    [SerializeField] private SuppressionLevel suppressionLevel = SuppressionLevel.Normal;
    [SerializeField] private int modelsLeft;
    [SerializeField] private int remainingActions;
    [SerializeField] GameObject selectionGO;
    [SerializeField] RawImage suppressionIndicator;
    Color transparent = new Color(0, 0, 0, 0);
    Color stressed = new Color(1, 1, 0, 1);
    Color suppressed = new Color(1, 0, 0, 1);
    public bool gotShotAt = false;
    TextMeshProUGUI infoText;

    [Header("Board Position")]
    public PathNodeHexXZ currentCell;

    public Action<Unit> onUnitDestroyed;

    public void Init(UnitConfig config)
    {
        _config = config;
        modelsLeft = _config.maxModels;
        remainingActions = _config.maxActionsPerTurn;
        infoText = transform.GetComponentInChildren<TextMeshProUGUI>();
        infoText.text = this.ToString();
        infoText.color = _config.faction == Faction.Germany ? Color.black : Color.blue;
        UpdateSuppressionIndicator();
        ToggleSelect(false);
    }

    public override string ToString()
    {
        return $"{_config.unitName}\n{modelsLeft}/{_config.maxModels}\n{remainingActions}/{_config.maxActionsPerTurn}";
    }

    // --- Properties ---
    public string UnitName => _config.unitName;
    public Faction Faction => _config.faction;
    public DoctrineTag Doctrine => _config.doctrineTag;
    public int AttackRange => _config.attackRange;
    public int ModelsLeft => modelsLeft;
    public SuppressionLevel CurrentSuppression => suppressionLevel;
    public int RemainingActions => remainingActions;

    public void ToggleSelect(bool value)
    {
        selectionGO.SetActive(value);
    }

    public bool UseAction(int cost)
    {
        if (remainingActions - cost < 0)
        {
            return false;
        }

        remainingActions -= cost;
        infoText.text = this.ToString();
        UpdateSuppressionIndicator();
        return true;
    }

    // --- Movement ---
    public bool CanMove(int distance)
    {
        return remainingActions >= distance && suppressionLevel != SuppressionLevel.Suppressed;
    }

    public int GetMovementRange()
    {
        switch (suppressionLevel)
        {
            case SuppressionLevel.Stressed: return Mathf.Max(1, _config.movementSpeed - 1);
            case SuppressionLevel.Suppressed: return 0;
            default: return _config.movementSpeed;
        }
    }

    public void PlaceOn(GridHexXZ<PathNodeHexXZ> grid, Vector2Int pos, int distance)
    {
        if (currentCell != null)
        {
            currentCell.occupant = null;
        }
        PathNodeHexXZ c = grid.GetGridObject(pos.x, pos.y);


        currentCell = c;
        if (c != null)
        {
            if (c.occupant != null)
            {
                Debug.LogWarning($"Cannot place unit on pos {pos}, it already is occupied!");
                return;
            }
            c.occupant = this;
        }
        transform.position = grid.GetWorldPosition(pos.x, pos.y);
        UseAction(distance);
    }

    public void Attack(Unit target)
    {
        if (remainingActions <= 0) return;
        //if (HexCoords.Distance(currentCell.coords, target.currentCell.coords) > attackRange) return;
        target.gotShotAt = true;
        for (int i = 0; i < modelsLeft; i++)
        {
            if (UnityEngine.Random.value <= _config.hitChance)
            {
                target.TakeDamage(1);
            }

            if (UnityEngine.Random.value <= _config.suppressionChance)
            {
                target.ApplySuppression();
            }
        }
        UseAction(1);
    }

    public void TakeDamage(int casualties)
    {
        modelsLeft -= casualties;
        infoText.text = this.ToString();
        if (modelsLeft <= 0)
        {
            Die();
        }
    }

    public void ApplySuppression()
    {
        if (suppressionLevel == SuppressionLevel.Normal)
        {
            suppressionLevel = SuppressionLevel.Stressed;
            remainingActions = _config.maxActionsPerTurn - 1;
        }
        else if (suppressionLevel == SuppressionLevel.Stressed)
        {
            suppressionLevel = SuppressionLevel.Suppressed;
            remainingActions = 1;
        }
        infoText.text = this.ToString();
        UpdateSuppressionIndicator();
    }

    private void UpdateSuppressionIndicator()
    {
        switch (suppressionLevel)
        {
            case SuppressionLevel.Normal:
                suppressionIndicator.color = transparent;
                break;
            case SuppressionLevel.Stressed:
                suppressionIndicator.color = stressed;
                break;
            case SuppressionLevel.Suppressed:
                suppressionIndicator.color = suppressed;
                break;
        }
    }

    public void Rally()
    {
        if (remainingActions <= 0) return;

        if (suppressionLevel == SuppressionLevel.Suppressed)
            suppressionLevel = SuppressionLevel.Stressed;
        else if (suppressionLevel == SuppressionLevel.Stressed)
            suppressionLevel = SuppressionLevel.Normal;

        UseAction(1);
    }

    void Die()
    {
        Debug.Log($"{_config.unitName} eliminated.");
        if (currentCell != null) currentCell.occupant = null;
        onUnitDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    public void ResetTurn()
    {
        if (gotShotAt)
        {
            gotShotAt = false;
        }
        else
        {
            switch (suppressionLevel)
            {
                case SuppressionLevel.Suppressed:
                    suppressionLevel = SuppressionLevel.Stressed;
                    break;
                case SuppressionLevel.Stressed:
                    suppressionLevel = SuppressionLevel.Normal;
                    break;
                default:
                    break;
            }
        }

        switch (suppressionLevel)
        {
            case SuppressionLevel.Suppressed:
                remainingActions = 1;
                break;
            case SuppressionLevel.Stressed:
                remainingActions = _config.maxActionsPerTurn - 1;
                break;
            default:
                remainingActions = _config.maxActionsPerTurn;
                break;
        }

        UpdateSuppressionIndicator();
        infoText.text = this.ToString();
    }
}
