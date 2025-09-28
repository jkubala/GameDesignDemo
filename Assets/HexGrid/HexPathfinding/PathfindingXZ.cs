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

    public class PathfindingXZ {

        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        public static PathfindingXZ Instance { get; private set; }

        private GridXZ<PathNodeXZ> grid;
        private List<PathNodeXZ> openList;
        private List<PathNodeXZ> closedList;

        public PathfindingXZ(int width, int height, float cellSize) {
            Instance = this;
            grid = new GridXZ<PathNodeXZ>(width, height, cellSize, Vector3.zero, (GridXZ<PathNodeXZ> g, int x, int y) => new PathNodeXZ(g, x, y));
        }

        public GridXZ<PathNodeXZ> GetGrid() {
            return grid;
        }

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
            grid.GetXZ(startWorldPosition, out int startX, out int startY);
            grid.GetXZ(endWorldPosition, out int endX, out int endY);

            List<PathNodeXZ> path = FindPath(startX, startY, endX, endY);
            if (path == null) {
                return null;
            } else {
                List<Vector3> vectorPath = new List<Vector3>();
                foreach (PathNodeXZ pathNode in path) {
                    vectorPath.Add(grid.GetWorldPosition(pathNode.x, pathNode.y));
                    //vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f);
                }
                return vectorPath;
            }
        }

        public List<PathNodeXZ> FindPath(int startX, int startY, int endX, int endY) {
            PathNodeXZ startNode = grid.GetGridObject(startX, startY);
            PathNodeXZ endNode = grid.GetGridObject(endX, endY);

            if (startNode == null || endNode == null) {
                // Invalid Path
                return null;
            }

            openList = new List<PathNodeXZ> { startNode };
            closedList = new List<PathNodeXZ>();

            for (int x = 0; x < grid.GetWidth(); x++) {
                for (int y = 0; y < grid.GetHeight(); y++) {
                    PathNodeXZ pathNode = grid.GetGridObject(x, y);
                    pathNode.gCost = 99999999;
                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0) {
                PathNodeXZ currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode) {
                    // Reached final node
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNodeXZ neighbourNode in GetNeighbourList(currentNode)) {
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

        private List<PathNodeXZ> GetNeighbourList(PathNodeXZ currentNode) {
            List<PathNodeXZ> neighbourList = new List<PathNodeXZ>();

            if (currentNode.x - 1 >= 0) {
                // Left
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
                // Left Down
                if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
                // Left Up
                if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
            if (currentNode.x + 1 < grid.GetWidth()) {
                // Right
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
                // Right Down
                if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
                // Right Up
                if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
            // Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
            // Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

            return neighbourList;
        }

        public PathNodeXZ GetNode(int x, int y) {
            return grid.GetGridObject(x, y);
        }

        private List<PathNodeXZ> CalculatePath(PathNodeXZ endNode) {
            List<PathNodeXZ> path = new List<PathNodeXZ>();
            path.Add(endNode);
            PathNodeXZ currentNode = endNode;
            while (currentNode.cameFromNode != null) {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNodeXZ a, PathNodeXZ b) {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private PathNodeXZ GetLowestFCostNode(List<PathNodeXZ> pathNodeList) {
            PathNodeXZ lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++) {
                if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                    lowestFCostNode = pathNodeList[i];
                }
            }
            return lowestFCostNode;
        }

    }

}