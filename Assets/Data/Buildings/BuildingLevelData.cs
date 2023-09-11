using System;
using System.Collections;
using System.Collections.Generic;
using Data.GeneralTiles;
using Data.Resources;
using Tiles;
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
    public struct BuildingLevelData
    {
    
        [SerializeField] private TileData _tileData;
        [SerializeField] private Requirements _buildRequirements;
        
        public TileData TileData => _tileData;
        public Requirements BuildRequirements => _buildRequirements;

    }
}
