using System.Collections.Generic;
using System.Linq;
using Characters;
using Data.Resources;
using Interfaces;
using Managers;
using UnityEngine;

namespace Tiles
{
    public class ResourceTileScript : MonoBehaviour, ITileInterface
    {


        
        [SerializeField] private ToolType _toolType;
        [SerializeField] private List<ResourceKeyValuePair> _resources;
        
        private TileScript _owningTile;
        
        public ToolType ToolType => _toolType;
        public List<ResourceKeyValuePair> Resources => _resources;
        
        private void Harvest(Character character)
        {
            if (_resources.Count == 0) return;
   
            character.SetTool(ToolType);
            TeamManager.Instance.GetTeam(character.CurrentUnit.TeamIndex).AddResources(_resources);
            InteractionManager.Instance.SpawnIndicator(transform.position, Resources.First().Resource.ResourceIcon);
        }

        public void OnInteract(Character character)
        {
            Harvest(character);
        }
        public void SetOwningTile(TileScript tile)
        {
            _owningTile = tile;
        }

        public TileScript GetOwningTile()
        {
            return _owningTile;
        }
    }
}
