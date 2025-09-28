using System.Collections.Generic;
using UnityEngine;
using CM_Pathfinding;

public class TestingHexPathfinding : MonoBehaviour
{
    [SerializeField] private Transform pfHex;
    [SerializeField] private SpawnConfig spawnConfig;

    private GridHexXZ<GridObject> gridHexXZ;
    private PathfindingHexXZ pathfindingHexXZ;

    private readonly List<Unit> germanUnits = new();
    private readonly List<Unit> polishUnits = new();
    private Unit _selectedUnit;
    private Faction currentPlayerTurn = Faction.Germany;

    private class GridObject
    {
        public Transform visualTransform;
    }

    private void Awake()
    {
        int width = 35, height = 26;
        float cellSize = 1f;

        gridHexXZ = new GridHexXZ<GridObject>(
            width, height, cellSize, Vector3.zero,
            (GridHexXZ<GridObject> g, int x, int y) => new GridObject()
        );

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Transform visual = Instantiate(pfHex, gridHexXZ.GetWorldPosition(x, z), Quaternion.identity);
                gridHexXZ.GetGridObject(x, z).visualTransform = visual;
            }
        }

        pathfindingHexXZ = new PathfindingHexXZ(width, height, cellSize);
    }

    private void Start()
    {
        foreach (var entry in spawnConfig.spawnEntries)
            InitUnit(entry);
    }

    private void InitUnit(SpawnConfig.SpawnEntry entry)
    {
        GameObject go = Instantiate(entry.config.unitPrefab, Vector3.zero, Quaternion.identity);
        Unit unit = go.GetComponent<Unit>();
        unit.Init(entry.config);
        unit.onUnitDestroyed += RemoveUnitFromList;
        unit.PlaceOn(pathfindingHexXZ.GetGrid(), entry.startPos, 0);

        if (unit.Faction == Faction.Poland) polishUnits.Add(unit);
        else germanUnits.Add(unit);
    }

    void RemoveUnitFromList(Unit unitToRemove)
    {
        unitToRemove.onUnitDestroyed -= RemoveUnitFromList;
        if(unitToRemove.Faction == Faction.Poland)
        {
            polishUnits.Remove(unitToRemove);
        }
        else
        {
            germanUnits.Remove(unitToRemove);
        }

    }

    public void EndTurn()
    {
        DeselectUnit();
        currentPlayerTurn = currentPlayerTurn == Faction.Germany ? Faction.Poland : Faction.Germany;

        foreach (Unit unit in GetUnitsFor(currentPlayerTurn))
            unit.ResetTurn();
    }

    private void DeselectUnit()
    {
        if (_selectedUnit == null) return;
        _selectedUnit.ToggleSelect(false);
        _selectedUnit = null;
    }

    private List<Unit> GetUnitsFor(Faction f) =>
        f == Faction.Germany ? germanUnits : polishUnits;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        if (Input.GetMouseButtonDown(1)) HandleRightClick();
    }

    private void HandleLeftClick()
    {
        if (_selectedUnit != null) DeselectUnit();

        PathNodeHexXZ clicked = pathfindingHexXZ.GetGrid().GetGridObject(Mouse3D.GetMouseWorldPosition());
        if (clicked != null && clicked.occupant != null && clicked.occupant.Faction == currentPlayerTurn)
        {
            _selectedUnit = clicked.occupant;
            _selectedUnit.ToggleSelect(true);
        }
    }

    private void HandleRightClick()
    {
        if (_selectedUnit == null) return;

        var clicked = pathfindingHexXZ.GetGrid().GetGridObject(Mouse3D.GetMouseWorldPosition());
        if (clicked == null) return;

        // --- ATTACK ---
        if (clicked.occupant != null && clicked.occupant.Faction != _selectedUnit.Faction)
        {
            List<Vector3> attackPath = pathfindingHexXZ.FindPath(
                pathfindingHexXZ.GetGrid().GetWorldPosition(_selectedUnit.currentCell.Position),
                pathfindingHexXZ.GetGrid().GetWorldPosition(clicked.occupant.currentCell.Position)
            );

            if (attackPath == null) return; // unreachable

            int distance = attackPath.Count - 1;
            if (distance <= _selectedUnit.AttackRange)
            {
                _selectedUnit.Attack(clicked.occupant);
            }
            else
            {
                Debug.Log("Target out of range!");
            }
            return;
        }

        // --- MOVE ---
        List<Vector3> path = pathfindingHexXZ.FindPath(
            pathfindingHexXZ.GetGrid().GetWorldPosition(_selectedUnit.currentCell.Position),
            Mouse3D.GetMouseWorldPosition()
        );
        if (path == null || path.Count < 2) return;

        int moveDistance = path.Count - 1;
        if (_selectedUnit.CanMove(moveDistance))
        {
            Vector2Int targetPos = clicked.Position;
            _selectedUnit.PlaceOn(pathfindingHexXZ.GetGrid(), targetPos, moveDistance);
        }
    }
}
