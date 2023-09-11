using System;
using Data.Buildings;
using Data.GeneralTiles;
using Tiles;
using UnityEngine;

namespace Managers
{
    public class TileManager : MonoBehaviour
    {
        
        public static TileManager Instance { get; private set; }
        private void Awake() => Instance = this;
        

        [SerializeField] private TileScript _baseTile;
        [Space(10)]
        [Header("Ground Meshes")]
        [SerializeField] private Mesh _grassGround;
        [SerializeField] private Mesh _waterGround;
        [Space(10)]
        [Header("Tile Data")]
        [SerializeField] private TileData _grassTileData;
        [SerializeField] private TileData _waterTileData;
        [SerializeField] private TileData _treesTileData;
        [SerializeField] private TileData _campsiteTileData;


        public TileScript CreateTile(Vector3 position, TileData tileData)
        {
            TileScript tile = Instantiate(_baseTile, position, Quaternion.identity);
            tile.SetTileData(tileData);
            return tile;
        }
        
        public void SetTileData(TileScript tile, TileData tileData)
        {
            if (!(tile && tileData)) return;
            tile.SetTileData(tileData);
        }
        
        public void SetTileGround(TileScript tile)
        {
            
            switch(tile.TileData.GroundType)
            {
                case GroundType.Grass:
                    tile.SetGround(_grassGround);
                    break;
                case GroundType.Water:
                    tile.SetGround(_waterGround);
                    break;
            }
            
            SelectionManager.Instance.Deselect();
        }
        
        public void PlaceBuilding(TileScript tile, BuildingTileData buildingData)
        {
            if (!(tile && buildingData)) return;
            if (buildingData.BuildingLevels.Count == 0) return;
            
            TileData tileData = buildingData.BuildingLevels[0].TileData;
            if (!tileData) return;
            
            tile.SetTileData(tileData);
            SelectionManager.Instance.Deselect();
        }
        
        public void CreateTown(TileScript tile)
        {
            SetTileData(tile, _campsiteTileData);
            SelectionManager.Instance.Deselect();
        }
        
        public TileScript CreateGrassTile(Vector3 position)
        {
            return CreateTile(position, _grassTileData);
        }
        public TileScript CreateWaterTile(Vector3 position)
        {
            return CreateTile(position, _waterTileData);
        }
        public TileScript CreateTreesTile(Vector3 position)
        {
            return CreateTile(position, _treesTileData);
        }
        
        public void SetGrass(TileScript tile)
        {
            SetTileData(tile, _grassTileData);
            SelectionManager.Instance.Deselect();
        }
        public void SetForest(TileScript tile)
        {
            SetTileData(tile, _treesTileData);
            SelectionManager.Instance.Deselect();
        }
        
    }
}
