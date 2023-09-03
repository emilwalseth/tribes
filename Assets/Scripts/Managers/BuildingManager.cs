using System;
using Tiles;
using UnityEngine;

namespace Managers
{
    public class BuildingManager : MonoBehaviour
    {
        
        public static BuildingManager Instance { get; private set; }
        private void Awake() => Instance = this;
        

        [SerializeField] private TileScript _campsitePrefab;
        [SerializeField] private TileScript _grassPrefab;
     
        public void MakeCampsite()
        {
            TileScript tile = SelectionManager.Instance.GetSelectedTile();
            MapManager.Instance.ReplaceTile(tile, _campsitePrefab);
            SelectionManager.Instance.Deselect();
        }
        
        public void ClearTile()
        {
            TileScript tile = SelectionManager.Instance.GetSelectedTile();
            MapManager.Instance.ReplaceTile(tile, _grassPrefab);
            SelectionManager.Instance.Deselect();
        }
        
    }
}
