using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;
    
    [NonSerialized] private Map _map;
    private int TileDistanceCost(Tile tileA, Tile tileB)
    {
        int cost = 0;
        if (tileA == null || tileB == null)
            return cost;

        int dx = Math.Abs(tileA.gridPosition.x - tileB.gridPosition.x);
        int dy = Math.Abs(tileA.gridPosition.y - tileB.gridPosition.y);
        int triangleSize = Math.Min(dx, dy);
        int distToTriangle = Math.Abs(dx - dy);

        cost = distToTriangle * STRAIGHT_COST + triangleSize * DIAGONAL_COST;
        return cost;
    }
    
    private List<Tile> BacktrackPath(Tile startTile, Tile targetTile)
    {
        List<Tile> tilePath = new List<Tile>();
        Tile currentTile = targetTile;
        
        // Add saved parents of each tile to tilePath
        while (currentTile != startTile)
        {
            tilePath.Add(currentTile);
            currentTile = currentTile.parentTile;
        }
        tilePath.Reverse();
        return tilePath;
    }
    
    public List<Tile> FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Tile startTile = _map.TileFromWorldPosition(startPosition);
        Tile targetTile = _map.TileFromWorldPosition(targetPosition);
        
        if (startTile == targetTile)
            return null;

        List<Tile> openList = new List<Tile>(); 
        HashSet<Tile> closedList = new HashSet<Tile>();
        openList.Add(startTile);

        while (openList.Count > 0)
        {
            // Iterate through Open List to get tile with lowest F Cost
            Tile currentTile = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentTile.fCost ||
                    openList[i].fCost == currentTile.fCost && openList[i].hCost < currentTile.hCost)
                {
                    currentTile = openList[i];
                }
            }
            
            // Moving tile from openList to closedList
            openList.Remove(currentTile);
            closedList.Add(currentTile);
            
            // If reached target tile, path was found
            if (currentTile == targetTile)
            {
                return BacktrackPath(startTile, targetTile);
            }

            // Evaluate all neighbour tiles
            foreach (Tile neighbourTile in currentTile.neighbourTiles)
            {
                if (closedList.Contains(neighbourTile) || !neighbourTile.walkable)
                    continue;

                int movementCost = currentTile.gCost + TileDistanceCost(currentTile, neighbourTile);
               
                // Neighbour tile is possible path?
                if (movementCost < neighbourTile.gCost || !openList.Contains(neighbourTile))
                {
                    // Update cost values
                    neighbourTile.gCost = movementCost;
                    startTile.hCost = TileDistanceCost(currentTile, targetTile);
                    neighbourTile.parentTile = currentTile;
                    
                    // Add neighbour tile to open list
                    if (!openList.Contains(neighbourTile))
                    {
                        openList.Add(neighbourTile);
                    }
                }
            }
        }
        return null;
    }

    private void Start()
    {
        _map = FindObjectOfType<Map>();
    }
}
