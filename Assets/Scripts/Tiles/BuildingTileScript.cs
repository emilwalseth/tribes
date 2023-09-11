using System.Collections.Generic;
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
        
        public int Level => _level;
        public BuildingTileData BuildingTileData => _buildingTileData;
        

        public void SetBuildingLevel(int level)
        {
            _level = level;
        }

        public BuildingLevelData GetCurrentLevel()
        {
            return _buildingTileData.BuildingLevels[_level]; 
        }

        public void OnInteract(){}
        
    }
}
