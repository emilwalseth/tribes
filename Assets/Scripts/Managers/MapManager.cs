using System.Collections.Generic;
using System.Linq;
using Data.GeneralTiles;
using Tiles;
using UnityEngine;
using UnityEngine.Serialization;
using World;
using Random = UnityEngine.Random;

namespace Managers
{
    public class MapManager : MonoBehaviour
    {
        
        public static MapManager Instance { get; private set; }

        [SerializeField] private Vector2Int _mapSize = new Vector2Int(10, 15);
        [SerializeField] private int _tileSize = 2;
        [SerializeField] private bool _startExplored = false;
        [SerializeField] private GameObject _testObj;
        
    
        // Noise Settings
        [SerializeField] private int _noiseSeed = 1276473;
        [SerializeField] private float _noiseFrequency = 100f;
        [SerializeField] private float _waterThreshold = 0.4f;
        [SerializeField] private float _forestThreshold = 0.7f;
        [SerializeField] private float _mountainThreshold = 0.85f;
            
        
        public int TileSize => _tileSize;
    
        
        // Private
        private readonly Dictionary<Vector3, TileScript> _mapTiles = new();

        private void Awake() => Instance = this;
        
        // Start is called before the first frame update
        void Start()
        {
            MakeMapGrid();

        }


        private Vector2 GetHexCoords(int x, int z)
        {
            float xPos = x * _tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);
            float zPos = z * _tileSize + ((x % 2 == 1) ? _tileSize * 0.5f : 0);

            return new Vector2(xPos, zPos);
        }
    
        public Vector3 GetMapMax()
        {
            Vector2 hexCoordsMax = GetHexCoords(_mapSize.x, _mapSize.y);
            return new Vector3(hexCoordsMax.x, 0, hexCoordsMax.y) + transform.position;
        }
    
        public Vector3 GetMapMin()
        {
            return transform.position;
        }

        public Vector3 GetMapCenter()
        {
            Vector3 mapMax = GetMapMax();
            Vector3 mapMin = GetMapMin();
            return (mapMax - mapMin) / 2;
        }


        void MakeMapGrid()
        {

            for (int x = 0; x < _mapSize.x; x++)
            {
                for (int z = 0; z < _mapSize.y; z++)
                {
                
                    Vector2 hexCoords = GetHexCoords(x, z);
                    Vector3 position = new (hexCoords.x, 0, hexCoords.y);
                
                    // If the noiseSeed is -1, make random seed
                    if (_noiseSeed == -1) _noiseSeed = Random.Range(0, 10000000);
     
                    // Get noise values (0-1)
                    float noiseValue = Mathf.PerlinNoise((hexCoords.x + _noiseSeed) / _noiseFrequency, (hexCoords.y + _noiseSeed) / _noiseFrequency);

                    
                    Vector3 tilePosition = position + transform.position;

                    TileScript newTile = TileManager.Instance.CreateTile(tilePosition);
                    newTile.SetExplored(_startExplored);

                    TileData tileData;
                    
                    // If the noiseValue is above the treesThreshold, make trees tile
                    if (noiseValue > _mountainThreshold)
                        tileData = TileManager.Instance.MountainTileData;
                    else if (noiseValue > _forestThreshold)
                        tileData = TileManager.Instance.ForestTileData;
                    else if (noiseValue < _waterThreshold)
                        tileData = TileManager.Instance.WaterTileData;
                    else
                        tileData = TileManager.Instance.GrassTileData;
                    
                    newTile.SetTileData(tileData);
                    newTile.transform.SetParent(transform);
                    
                    // Add tile to dictionary over all tiles
                    _mapTiles.Add(Vector3Int.RoundToInt(tilePosition), newTile);
                
                }
            }
        
            // Tell listeners that the map is done generating
            EventManager.Instance.onMapGenerated?.Invoke();
        }
        
        private Vector2Int GetSquareCoords(Vector2 hexCoords)
        {
            int x = Mathf.RoundToInt(hexCoords.x / (_tileSize * Mathf.Cos(Mathf.Deg2Rad * 30)));
            int z = Mathf.RoundToInt(hexCoords.y / _tileSize - ((x % 2 == 1) ? 0.5f : 0));

            return new Vector2Int(x, z);
        }

        public TileScript GetRandomTile(GroundType groundType)
        {
            List<TileScript> mapTiles = _mapTiles.Values.Where(tile => tile.TileData.GroundType == groundType).ToList();
            return mapTiles.Count > 0 ? mapTiles[Random.Range(0, mapTiles.Count)] : null;
        }

        public List<TileScript> GetTileNeighbors(GameObject tile, bool includeNull = false)
        {
            Vector3 worldPosition = tile.transform.position;

            List<TileScript> neighbors = new();

            Vector3 offset = new (_tileSize, 0, 0);
            const float angleBetween = 60;
            
            for (int i = 0; i < 6; i++)
            {
                Quaternion rotation = Quaternion.Euler(0f, (angleBetween * i) + 30f, 0f);
                Vector3 neighborPos = rotation * offset;
                neighborPos += worldPosition;
                
                neighborPos = Vector3Int.RoundToInt(neighborPos);
                

                if (_mapTiles.TryGetValue(neighborPos, out TileScript mapTile))
                {
                    neighbors.Add(mapTile);
                    //GameObject sphere = Instantiate(_testObj, mapTile.transform.position, Quaternion.identity);
                    //sphere.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.black, Color.white,  i / 6f);
                }
                else if (includeNull)
                {
                    neighbors.Add(null);
                }
            }

            return neighbors;
        }



        public TileScript GetTileAtPosition(Vector3 worldPos)
        {
            Vector3Int tileIndex = Vector3Int.RoundToInt(worldPos);
            return _mapTiles.TryGetValue(tileIndex, out TileScript tile) ? tile : null;
        }
        

    }
}
