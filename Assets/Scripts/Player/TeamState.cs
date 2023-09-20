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
        private List<ResourceTileScript> _seenResources = new List<ResourceTileScript>();
        private List<BuildingTileScript> _seenBuildings = new List<BuildingTileScript>();
    
        public int TeamIndex => _teamIndex;
        public List<TileScript> Towns => _towns;
        public List<TileScript> Buildings => _buildings;
        public List<Unit> Units => _units;
        public Dictionary<ResourceType, int> Resources { get; } = new();
        public List<ResourceTileScript> SeenResources { get => _seenResources; set => _seenResources = value; }
        public List<BuildingTileScript> SeenBuildings { get => _seenBuildings; set => _seenBuildings = value; }


        public UnityAction onStatsChanged;

        public TeamState(int index)
        {
            _teamIndex = index;
        }
        
        public bool HasResource(ResourceType resourceType, int amount)
        {
            return Resources.ContainsKey(resourceType) && Resources[resourceType] >= amount;
        }

        public void AddSeenResource(ResourceTileScript resourceTile)
        {
            if (_seenResources.Contains(resourceTile)) return;
            _seenResources.Add(resourceTile);
        }
        
        public void AddSeenTile(TileScript tile)
        {
            if (tile.CurrentTile.TryGetComponent(out ResourceTileScript resourceTile))
            {
                if (SeenResources.Contains(resourceTile))return;
                SeenResources.Add(resourceTile);
            }
            else if (tile.CurrentTile.TryGetComponent(out BuildingTileScript buildingTile))
            {
                if (SeenBuildings.Contains(buildingTile))return;
                SeenBuildings.Add(buildingTile);
            }
        }
        
        private void AddResource(ResourceType resourceType, int amount)
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
        
        public void AddResources(List<ResourceKeyValuePair> resources)
        {
            foreach (ResourceKeyValuePair resource in resources)
            {
                AddResource(resource.Resource.ResourceType, resource.Amount);
            }
        }

    }
}
