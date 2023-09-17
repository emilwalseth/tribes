using System;
using System.Collections.Generic;
using Data.GeneralTiles;
using Data.Resources;
using UnityEngine;

namespace Data.Buildings
{
    
    
    [CreateAssetMenu(fileName = "TownData", menuName = "ScriptableObjects/TownData", order = 1)]
    public class TownData : ScriptableObject
    {

        [SerializeField] private BuildingTileData _townHall;
        [SerializeField] private BuildingTileData _house;
        [SerializeField] private BuildingTileData _port;
        
        public BuildingTileData TownHall => _townHall;
        public BuildingTileData House => _house;
        public BuildingTileData Port => _port;

    }
}
