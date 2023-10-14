using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Data.Resources;
using Managers;
using Tiles;
using UnityEngine.Events;

namespace Player
{
    [Serializable]
    public class TeamState
    {
        private List<TileScript> _towns = new();
        private List<Unit> _units = new();


        private int _teamIndex = 0;
        private List<TileScript> _buildings = new List<TileScript>();
        private List<ResourceTileScript> _seenResourceTiles = new List<ResourceTileScript>();
        private List<BuildingTileScript> _seenBuildingTiles = new List<BuildingTileScript>();
    
        public int TeamIndex => _teamIndex;
        public List<TileScript> Towns => _towns;
        public List<TileScript> Buildings => _buildings;
        public List<Unit> Units => _units;
        public Dictionary<ResourceType, int> Resources { get; } = new();
        public List<ResourceTileScript> SeenResourceTiles { get => _seenResourceTiles; set => _seenResourceTiles = value; }
        public List<BuildingTileScript> SeenBuildingTiles { get => _seenBuildingTiles; set => _seenBuildingTiles = value; }


        public UnityAction onStatsChanged;

        public TeamState(int index)
        {
            _teamIndex = index;
        }
        
        public bool HasResource(ResourceType resourceType, int amount)
        {
            return Resources.ContainsKey(resourceType) && Resources[resourceType] >= amount;
        }
        
        public bool HasResources(List<ResourceAmount> resourceType)
        {
            
            foreach (ResourceAmount type in resourceType)
            {
                if (!HasResource(type.ResourceData.ResourceType, type.Amount))
                    return false;
            }

            return true;
        }

        public void AddSeenResourceTile(ResourceTileScript resourceTile)
        {
            if (_seenResourceTiles.Contains(resourceTile)) return;
            _seenResourceTiles.Add(resourceTile);
        }
        
        public void AddSeenTile(TileScript tile)
        {
            if (tile.CurrentTile.TryGetComponent(out ResourceTileScript resourceTile))
            {
                if (SeenResourceTiles.Contains(resourceTile))return;
                SeenResourceTiles.Add(resourceTile);
            }
            else if (tile.CurrentTile.TryGetComponent(out BuildingTileScript buildingTile))
            {
                if (SeenBuildingTiles.Contains(buildingTile))return;
                SeenBuildingTiles.Add(buildingTile);
            }
        }
        
        public void AddResource(ResourceType resourceType, int amount)
        {

            if (Resources.ContainsKey(resourceType))
            {
                Resources[resourceType] += amount;
            }
            else
            {
                Resources.Add(resourceType, amount);
            }
            
            onStatsChanged?.Invoke();
        }
        
        
        public void RemoveUnit(Unit unit)
        {
            if (Units.Contains(unit))
            {
                Units.Remove(unit);
            }
            if (Units.Count == 0)
            {
                GameManager.Instance.GameOver(_teamIndex);
            }
        }

        public void RemoveResource(ResourceType resourceType, int amount)
        {
            if (!Resources.ContainsKey(resourceType)) return;
            Resources[resourceType] -= amount;
            onStatsChanged?.Invoke();
        }
        
        public void RemoveResources(List<ResourceAmount> resourceType)
        {
            foreach (ResourceAmount type in resourceType)
            {
                RemoveResource(type.ResourceData.ResourceType, type.Amount);
            }
        }
        
        public void AddResources(List<ResourceKeyValuePair> resources)
        {
            foreach (ResourceKeyValuePair resource in resources)
            {
                AddResource(resource.Resource.ResourceType, resource.Amount);
            }
        }

    }
}
