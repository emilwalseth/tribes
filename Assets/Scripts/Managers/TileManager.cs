using System;
using Data.Buildings;
using Data.GeneralTiles;
using Tiles;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class TileManager : MonoBehaviour
    {
        
        public static TileManager Instance { get; private set; }
        private void Awake() => Instance = this;
        

        [SerializeField] private TileScript _baseTile;
        [SerializeField] private TownData _townData;
        [Space(10)]
        [Header("Ground Meshes")]
        [SerializeField] private Mesh _unexploredGround;
        [SerializeField] private Mesh _grassGround;
        [SerializeField] private Mesh _townGround;
        [SerializeField] private Mesh _waterGround;
        [SerializeField] private Mesh _stoneGround;

        [Space(10)] [Header("Tile Data")] 
        [SerializeField] private TileData _grassTileData;
        [SerializeField] private TileData _waterTileData;
        [SerializeField] private TileData _forestTileData;
        [SerializeField] private TileData _mountainTileData;
        [SerializeField] private TileData _campsiteTileData;
        
        
        public TownData TownData => _townData;
        public TileData GrassTileData => _grassTileData;
        public TileData WaterTileData => _waterTileData;
        public TileData ForestTileData => _forestTileData;
        public TileData MountainTileData => _mountainTileData;
        public TileData CampsiteTileData => _campsiteTileData;


        public TileScript CreateTile(Vector3 position)
        {
            TileScript tile = Instantiate(_baseTile, position, Quaternion.identity);
            return tile;
        }
        
        public void SetTileGround(TileScript tile)
        {
            if (tile.TownTile && tile.TileData.GroundType != GroundType.Water)
            {
                tile.SetGround(_townGround);
                return;
            }
            
            switch(tile.TileData.GroundType)
            {
                case GroundType.Grass:
                    tile.SetGround(_grassGround);
                    break;
                case GroundType.Town:
                    tile.SetGround(_townGround);
                    break;
                case GroundType.Water:
                    tile.SetGround(_waterGround);
                    break;
                case GroundType.Stone:
                    tile.SetGround(_stoneGround);
                    break;
                default:
                    tile.SetGround(_grassGround);
                    break;
            }
            
        }
        
        public void PlaceBuilding(int teamIndex, TileScript tile, BuildingTileData buildingData)
        {
            if (!(tile && buildingData)) return;
            
            TileData tileData = buildingData.BuildingData.TileData;
            if (!tileData) return;
            
            TeamManager.Instance.GetTeam(teamIndex).Buildings.Add(tile);
            
            tile.SetTileData(tileData, teamIndex);
        }
        
        public Mesh GetUnexploredMesh()
        {
            return _unexploredGround;
        }
        
        public void CreateTown(TileScript tile, int teamIndex)
        {
            tile.SetTileData(CampsiteTileData, teamIndex);
            MapManager.Instance.AddTownTile(tile);
            TeamManager.Instance.GetTeam(teamIndex).Towns.Add(tile);
        }
        
        public void SetGrass(TileScript tile)
        {
            tile.SetTileData(GrassTileData);
        }
        public void SetForest(TileScript tile)
        {
            tile.SetTileData(ForestTileData);
        }
        
    }
}
