using System;
using System.Collections.Generic;
using System.Linq;
using Data.Resources;
using Tiles;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    
    public class StatsManager : MonoBehaviour
    {
        
        public static StatsManager Instance { get; private set; }
        private void Awake() => Instance = this;

        [SerializeField] private List<ResourceKeyValuePair> _startingItems;

        public Dictionary<ResourceData, int> Resources { get; } = new();

        public UnityAction onStatsChanged;

        private void Start()
        {
            foreach (ResourceKeyValuePair item in _startingItems)
            {
                AddResource(item.Resource, item.Amount);
            }
        }

        public bool HasResource(ResourceData resourceData, int amount)
        {
            return Resources.ContainsKey(resourceData) && Resources[resourceData] >= amount;
        }
        

        private void AddResource(ResourceData resourceData, int amount)
        {

            if (Resources.ContainsKey(resourceData))
            {
                Resources[resourceData] += amount;
            }
            else
            {
                Resources.Add(resourceData, amount);
            }
            
            onStatsChanged?.Invoke();
        }

        public void RemoveResource(ResourceData resourceData, int amount)
        {
            if (!Resources.ContainsKey(resourceData)) return;
            Resources[resourceData] -= amount;
            onStatsChanged?.Invoke();
        }
        
        public void AddResources(List<ResourceKeyValuePair> resources)
        {
            foreach (ResourceKeyValuePair resource in resources)
            {
                AddResource(resource.Resource, resource.Amount);
            }
        }
    }
}
