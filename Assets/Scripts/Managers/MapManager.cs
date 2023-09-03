using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;
using World;
using Random = UnityEngine.Random;

namespace Managers
{
    public class MapManager : MonoBehaviour
    {
        
        public static MapManager Instance { get; private set; }

        [SerializeField] private Vector2Int _mapSize = new Vector2Int(10, 15);
        [SerializeField] private int _tileSize = 1;
        [SerializeField] private TileScript _grassTilePrefab;
        [SerializeField] private TileScript _treesTilePrefab;
        [SerializeField] private TileScript _waterTilePrefab;
        [SerializeField] private GameObject _testObj;
        
    
        // Noise Settings
        [SerializeField] private int _noiseSeed = 1276473;
        [SerializeField] private float _noiseFrequency = 100f;
        [SerializeField] private float _waterThreshold = 0.4f;
        [SerializeField] private float _treesThreshold = 0.7f;
    
    
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

                    // Initiate default tile as grass
                    TileScript prefab = _grassTilePrefab;
                
                    // If the noiseValue is less than the waterThreshold, make the prefab water instead
                    if (noiseValue < _waterThreshold) prefab = _waterTilePrefab;
                
                    // If the noiseValue is above the treesThreshold, make the prefab trees instead
                    if (noiseValue > _treesThreshold) prefab = _treesTilePrefab;
                
                    Vector3 tilePosition = position + transform.position;

                    // Only instantiate the tile if it is not water
                    TileScript tile = Instantiate(prefab, tilePosition, Quaternion.identity);
                    tile.transform.SetParent(transform);
                
                    // Add tile to dictionary over all tiles
                    _mapTiles.Add(Vector3Int.RoundToInt(tilePosition), tile);
                
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

        public TileScript GetRandomGrassTile()
        {

            List<Vector3> openTiles = _mapTiles.Keys.ToList();

            while (openTiles.Count > 0)
            {
                Vector3 pos = openTiles[Random.Range(0, openTiles.Count)];
                TileScript tile = _mapTiles[pos].GetComponent<TileScript>();
                openTiles.Remove(pos);
                if (!tile) continue;
                if (tile.TileData.TileType == TileTypes.Grass) return tile;
            }

            return null;
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

        public void ReplaceTile(TileScript oldTile, TileScript newTile)
        {
            Vector3 oldTilePos = oldTile.transform.position; 
            Vector3Int oldTileIndex = Vector3Int.RoundToInt(oldTilePos);
            Destroy(oldTile.gameObject);
            TileScript tile = Instantiate(newTile, oldTilePos, Quaternion.identity);
            tile.transform.SetParent(transform);
            _mapTiles[oldTileIndex] = tile;
        }

    }
}
