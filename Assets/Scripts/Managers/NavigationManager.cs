using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Tiles;

using UnityEngine;
using World;

namespace Managers
{
    public class NavigationManager : MonoBehaviour
    {
        
        public static NavigationManager Instance { get; private set; }
        private void Awake() => Instance = this;

        [SerializeField] private PathRenderer _pathRendererPrefab;


        public void MakePathRenderer(Unit unit)
        {
            PathRenderer pathRenderer = unit.CurrentPathRenderer;
            if (pathRenderer)
            {
                pathRenderer.InitiatePath(unit);
                return;
            }

            pathRenderer = Instantiate(_pathRendererPrefab, new Vector3(0,0.1f,0), Quaternion.identity);
            pathRenderer.InitiatePath(unit);
        }
        
        
        
        
        public static List<NavigationNode> FindPath(NavigationNode startTile, NavigationNode endTile)
        {

            MapManager map = MapManager.Instance;

            if (!startTile || !endTile) return null;

            // Make two lists for open and closed nav nodes
            List<NavigationNode> openList = new();
            List<NavigationNode> closedList = new();

            // Add the starting tile to get started
            openList.Add(startTile);

            // While there are still nodes in the open list, we keep checking
            while (openList.Count > 0)
            {

                // Find node with lowest F-Cost in openList
                // This node will be the next node we check
                NavigationNode currentTile = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].FCost < currentTile.FCost)
                    {
                        currentTile = openList[i];
                    }
                }
                
                // Remove current node from open list and add it to the closed list, since this now has been checked.
                openList.Remove(currentTile);
                closedList.Add(currentTile);
                
                // If we found the end tile, return path
                if (currentTile == endTile) return RetracePath(startTile, endTile);
                
                // If not, we need to check all the neighbors of the current tile
                List<TileScript> neighbors = map.GetTileNeighbors(currentTile.gameObject);
                
                foreach (TileScript neighbor in neighbors)
                {
                    // If the neighbor is not valid, continue
                    if (!neighbor) continue;

                    // If the tile is not walkable or already in closed list, continue
                    if (!neighbor.IsWalkable || closedList.Contains(neighbor)) continue;
                    
                    bool isNeighborInOpenList = openList.Contains(neighbor);
                    
                    // If neighbor is further away and in the open list, we don't need to check it
                    int newMovementCost = currentTile.GCost + (int)Vector3.Distance(currentTile.transform.position, neighbor.transform.position) + neighbor.NodeWeight;
                    if (newMovementCost >= neighbor.GCost && isNeighborInOpenList) continue;
                    
                    // Calculate movement costs
                    neighbor.GCost = newMovementCost;
                    neighbor.HCost = (int)Vector3.Distance(neighbor.transform.position, endTile.transform.position);
                    neighbor.Parent = currentTile;
                    
                    // Add neighbor to open list
                    if (!isNeighborInOpenList) openList.Add(neighbor);
                }
            }

            return RetracePath(startTile, endTile);
        }

        private static List<NavigationNode> RetracePath(NavigationNode startTile, NavigationNode endTile)
        {
            List<NavigationNode> path = new();

            // Get the parent of all tiles from the endTile to get the path back to start
            NavigationNode currentTile = endTile;
            while (currentTile != startTile)
            {
                path.Add(currentTile);
                NavigationNode parent = currentTile.Parent;
                
                // If the parent is not valid, it means no successful path was found
                if (!parent) return null;
                currentTile = currentTile.Parent;
            }

            // Reverse it to get the correct path back to end
            path.Reverse();
            return path;
        }
    }
}