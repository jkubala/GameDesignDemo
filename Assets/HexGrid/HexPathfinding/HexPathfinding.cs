/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CM_Pathfinding {

    public class HexPathfinding {


        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;


        public static HexPathfinding Instance { get; private set; }


        private GridHexXZ<HexPathNode> grid;
        private List<HexPathNode> openList;
        private List<HexPathNode> closedList;


        public HexPathfinding(int width, int height, float cellSize) {
            Instance = this;
            grid = new GridHexXZ<HexPathNode>(width, height, cellSize, Vector3.zero, (GridHexXZ<HexPathNode> g, int x, int y) => new HexPathNode(g, x, y));
        }

        public GridHexXZ<HexPathNode> GetGrid() {
            return grid;
        }

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
            grid.GetXZ(startWorldPosition, out int startX, out int startY);
            grid.GetXZ(endWorldPosition, out int endX, out int endY);

            List<HexPathNode> path = FindPath(startX, startY, endX, endY);
            if (path == null) {
                return null;
            } else {
                List<Vector3> vectorPath = new List<Vector3>();
                foreach (HexPathNode pathNode in path) {
                    vectorPath.Add(grid.GetWorldPosition(pathNode.x, pathNode.y));
                    //vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f);
                }
                return vectorPath;
            }
        }

        public List<HexPathNode> FindPath(int startX, int startY, int endX, int endY) {
            HexPathNode startNode = grid.GetGridObject(startX, startY);
            HexPathNode endNode = grid.GetGridObject(endX, endY);

            if (startNode == null || endNode == null) {
                // Invalid Path
                return null;
            }

            openList = new List<HexPathNode> { startNode };
            closedList = new List<HexPathNode>();

            for (int x = 0; x < grid.GetWidth(); x++) {
                for (int y = 0; y < grid.GetHeight(); y++) {
                    HexPathNode pathNode = grid.GetGridObject(x, y);
                    pathNode.gCost = 99999999;
                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0) {
                HexPathNode currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode) {
                    // Reached final node
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (HexPathNode neighbourNode in GetNeighbourList(currentNode)) {
                    if (closedList.Contains(neighbourNode)) continue;
                    if (!neighbourNode.isWalkable) {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost) {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                        neighbourNode.CalculateFCost();

                        if (!openList.Contains(neighbourNode)) {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

            // Out of nodes on the openList
            return null;
        }

        private List<HexPathNode> GetNeighbourList(HexPathNode currentNode) {
            List<HexPathNode> neighbourList = new List<HexPathNode>();

            bool oddRow = currentNode.y % 2 == 1;

            if (currentNode.x - 1 >= 0) {
                // Left
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            }
            if (currentNode.x + 1 < grid.GetWidth()) {
                // Right
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            }
            if (currentNode.y - 1 >= 0) {
                // Down
                neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
            }
            if (currentNode.y + 1 < grid.GetHeight()) {
                // Up
                neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));
            }

            if (oddRow) {
                if (currentNode.y + 1 < grid.GetHeight() && currentNode.x + 1 < grid.GetWidth()) {
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
                }
                if (currentNode.y - 1 >= 0 && currentNode.x + 1 < grid.GetWidth()) {
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
                }
            } else {
                if (currentNode.y + 1 < grid.GetHeight() && currentNode.x - 1 >= 0) {
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
                }
                if (currentNode.y - 1 >= 0 && currentNode.x - 1 >= 0) {
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
                }
            }

            return neighbourList;
        }

        public HexPathNode GetNode(int x, int y) {
            return grid.GetGridObject(x, y);
        }

        private List<HexPathNode> CalculatePath(HexPathNode endNode) {
            List<HexPathNode> path = new List<HexPathNode>();
            path.Add(endNode);
            HexPathNode currentNode = endNode;
            while (currentNode.cameFromNode != null) {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(HexPathNode a, HexPathNode b) {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private HexPathNode GetLowestFCostNode(List<HexPathNode> pathNodeList) {
            HexPathNode lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++) {
                if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                    lowestFCostNode = pathNodeList[i];
                }
            }
            return lowestFCostNode;
        }

    }

}