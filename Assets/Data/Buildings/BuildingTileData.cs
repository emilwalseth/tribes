using System.Collections.Generic;
using UnityEngine;

namespace Data.Buildings
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/BuildingData", order = 1)]
    public class BuildingTileData : ScriptableObject
    {
    
        [SerializeField] private List<BuildingLevelData> _buildingLevels;
        
        public List<BuildingLevelData> BuildingLevels => _buildingLevels;
        
    }
}
