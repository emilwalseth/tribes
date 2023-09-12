using System.Collections.Generic;
using System.Linq;
using Data.Resources;
using Interfaces;
using Managers;
using UnityEngine;

namespace Tiles
{
    public class ResourceTileScript : MonoBehaviour, ITileInterface
    {

        
        [SerializeField] private List<ResourceKeyValuePair> _resources;
        public List<ResourceKeyValuePair> Resources => _resources;


        public void Harvest()
        {
            if (_resources.Count == 0) return;
            if (TryGetComponent(out TileScript tile))
            {
                tile.PlayInteractAnimation();
            }
            
            StatsManager.Instance.AddResources(Resources);
            InteractionManager.Instance.SpawnIndicator(transform.position, Resources.First().Resource);
        }

        public void OnInteract()
        {
            Harvest();
        }

        public int GetSelectionRadius()
        {
            return 0;
        }
    }
}
