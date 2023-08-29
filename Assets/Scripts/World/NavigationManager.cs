using System.Collections.Generic;
using Tiles;

using UnityEngine;

namespace World
{
    public class NavigationManager
    {
        public static List<GameObject> FindPath(MapGenerator map, GameObject startTile, GameObject endTile)
        {

            NavigationNode startNavNode = startTile.GetComponent<NavigationNode>();
            NavigationNode endNavNode = endTile.GetComponent<NavigationNode>();

            if (!startNavNode || !endNavNode) return null;

                // Make two lists for open and closed nav nodes
            List<NavigationNode> openList = new();
            List<NavigationNode> closedList = new();

            // Add the starting tile to get started
            openList.Add(startNavNode);

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
                if (currentTile.gameObject == endTile) return RetracePath(startNavNode, endNavNode);
                
                // If not, we need to check all the neighbors of the current tile
                List<GameObject> neighbors = map.GetTileNeighbors(currentTile.gameObject);
                
                foreach (GameObject neighbor in neighbors)
                {
                    // If the neighbor is not valid, continue
                    if (!neighbor) continue;

                    // Get tile nav script, if it is not valid, continue
                    NavigationNode neighborNavNode = neighbor.GetComponent<NavigationNode>();
                    if (!neighborNavNode) continue;
                    
                    // If the tile is not walkable or already in closed list, continue
                    if (!neighborNavNode.IsWalkable || closedList.Contains(neighborNavNode)) continue;
                    
                    bool isNeighborInOpenList = openList.Contains(neighborNavNode);
                    
                    // If neighbor is further away and in the open list, we don't need to check it
                    int newMovementCost = currentTile.GCost + (int)Vector3.Distance(currentTile.transform.position, neighbor.transform.position);
                    if (newMovementCost >= neighborNavNode.GCost && isNeighborInOpenList) continue;
                    
                    // Calculate movement costs
                    neighborNavNode.GCost = newMovementCost;
                    neighborNavNode.HCost = (int)Vector3.Distance(neighbor.transform.position, endTile.transform.position);
                    neighborNavNode.Parent = currentTile;
                    
                    // Add neighbor to open list
                    if (!isNeighborInOpenList) openList.Add(neighborNavNode);
                }
            }

            return RetracePath(startNavNode, endNavNode);
        }

        private static List<GameObject> RetracePath(NavigationNode startTile, NavigationNode endTile)
        {
            List<GameObject> path = new();

            // Get the parent of all tiles from the endTile to get the path back to start
            NavigationNode currentTile = endTile;
            while (currentTile != startTile)
            {
                path.Add(currentTile.gameObject);
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