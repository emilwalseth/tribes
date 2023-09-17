using System.Collections.Generic;
using Characters;
using Data.Buildings;
using Interfaces;
using Managers;
using UnityEngine;

namespace Tiles
{
    public class BuildingTileScript : MonoBehaviour, ITileInterface
    {

        [SerializeField] [Range(1, 100)] private int _level = 1;
        [SerializeField] private BuildingTileData _buildingTileData;

        private TileScript _owningTile;
        
        public int Level => _level;
        public BuildingTileData BuildingTileData => _buildingTileData;
       

        
        public void OnInteract(Character character){}
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
