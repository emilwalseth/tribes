using System;
using System.Collections.Generic;
using Data.GeneralTiles;
using Data.Resources;
using UnityEngine;

namespace Data.Buildings
{
    
    [Serializable]
    public struct Requirements
    {
        [SerializeField] private List<ResourceKeyValuePair> _resources;
        public List<ResourceKeyValuePair> Resources => _resources;
    }
    
    
    [Serializable]
    public struct BuildingData
    {
    
        [SerializeField] private TileData _tileData;
        [SerializeField] private GroundType _placedOn;
        [SerializeField] private Requirements _buildRequirements;
        
        public TileData TileData => _tileData;
        public GroundType PlacedOn => _placedOn;
        public Requirements BuildRequirements => _buildRequirements;

    }
    
    
    [CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/BuildingData", order = 1)]
    public class BuildingTileData : ScriptableObject
    {
    
        [SerializeField] private BuildingData _buildingData;
        [SerializeField] private List<BuildingTileData> _upgradeOptions;
        
        public BuildingData BuildingData => _buildingData;
        
    }
}
