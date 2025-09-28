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

    public class PathfindingHexXZ {

        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        public static PathfindingHexXZ Instance { get; private set; }

        private GridHexXZ<PathNodeHexXZ> grid;
        private List<PathNodeHexXZ> openList;
        private List<PathNodeHexXZ> closedList;

        public PathfindingHexXZ(int width, int height, float cellSize) {
            Instance = this;
            grid = new GridHexXZ<PathNodeHexXZ>(width, height, cellSize, Vector3.zero, (GridHexXZ<PathNodeHexXZ> g, int x, int y) => new PathNodeHexXZ(g, x, y));
        }

        public GridHexXZ<PathNodeHexXZ> GetGrid() {
            return grid;
        }

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
            grid.GetXZ(startWorldPosition, out int startX, out int startY);
            grid.GetXZ(endWorldPosition, out int endX, out int endY);

            List<PathNodeHexXZ> path = FindPath(startX, startY, endX, endY);
            if (path == null) {
                return null;
            } else {
                List<Vector3> vectorPath = new List<Vector3>();
                foreach (PathNodeHexXZ pathNode in path) {
                    vectorPath.Add(grid.GetWorldPosition(pathNode.x, pathNode.y));
                    //vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f);
                }
                return vectorPath;
            }
        }

        public List<PathNodeHexXZ> FindPath(int startX, int startY, int endX, int endY) {
            PathNodeHexXZ startNode = grid.GetGridObject(startX, startY);
            PathNodeHexXZ endNode = grid.GetGridObject(endX, endY);

            if (startNode == null || endNode == null) {
                // Invalid Path
                return null;
            }

            openList = new List<PathNodeHexXZ> { startNode };
            closedList = new List<PathNodeHexXZ>();

            for (int x = 0; x < grid.GetWidth(); x++) {
                for (int y = 0; y < grid.GetHeight(); y++) {
                    PathNodeHexXZ pathNode = grid.GetGridObject(x, y);
                    pathNode.gCost = 99999999;
                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0) {
                PathNodeHexXZ currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode) {
                    // Reached final node
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNodeHexXZ neighbourNode in GetNeighbourList(currentNode)) {
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

        private List<PathNodeHexXZ> GetNeighbourList(PathNodeHexXZ currentNode) {
            List<PathNodeHexXZ> neighbourList = new List<PathNodeHexXZ>();

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

        public PathNodeHexXZ GetNode(int x, int y) {
            return grid.GetGridObject(x, y);
        }

        private List<PathNodeHexXZ> CalculatePath(PathNodeHexXZ endNode) {
            List<PathNodeHexXZ> path = new List<PathNodeHexXZ>();
            path.Add(endNode);
            PathNodeHexXZ currentNode = endNode;
            while (currentNode.cameFromNode != null) {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNodeHexXZ a, PathNodeHexXZ b) {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_STRAIGHT_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private PathNodeHexXZ GetLowestFCostNode(List<PathNodeHexXZ> pathNodeList) {
            PathNodeHexXZ lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++) {
                if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                    lowestFCostNode = pathNodeList[i];
                }
            }
            return lowestFCostNode;
        }

    }

}