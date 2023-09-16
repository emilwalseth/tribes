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
        
        public int Level => _level;
        public BuildingTileData BuildingTileData => _buildingTileData;
        
        
        public void OnInteract(Character character){}
        
    }
}
