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

namespace CM_Pathfinding
{

    public class PathNodeHexXZ
    {

        private GridHexXZ<PathNodeHexXZ> grid;
        public int x;
        public int y;

        public Vector2Int Position
        {
            get
            {
                return new Vector2Int(x, y);
            }
        }

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;
        public Unit occupant;
        public PathNodeHexXZ cameFromNode;

        public PathNodeHexXZ(GridHexXZ<PathNodeHexXZ> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            isWalkable = true;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
            grid.TriggerGridObjectChanged(x, y);
        }

        public override string ToString()
        {
            return x + "," + y;
        }

    }

}