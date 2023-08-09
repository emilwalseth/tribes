using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    
    [SerializeField] private int _mapWidth = 10;
    [SerializeField] private int _mapHeight = 10;
    [SerializeField] private int _tileSize = 1;
    [SerializeField] private GameObject _grassTilePrefab;
    [SerializeField] private GameObject _treesTilePrefab;
    
    // Noise Settings
    [SerializeField] private float _noiseFrequency = 100f;
    [SerializeField] private float _waterThreshold = 0.4f;
    [SerializeField] private float _treesThreshold = 0.7f;
    
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


    void MakeMapGrid()
    {

        for (int x = 0; x < _mapWidth; x++)
        {
            for (int z = 0; z < _mapHeight; z++)
            {

                Vector2 hexCoords = GetHexCoords(x, z);
                Vector3 position = new Vector3(hexCoords.x, 0, hexCoords.y);
                
                // Get noise values (0-1)
                float noiseValue = Mathf.PerlinNoise(hexCoords.x / _noiseFrequency, hexCoords.y / _noiseFrequency);

                // Initiate default tile as grass
                GameObject prefab = _grassTilePrefab;
                
                // If the noiseValue is less than the waterThreshold, do not spawn a tile, just continue
                if (noiseValue < _waterThreshold) continue;
                
                // If the noiseValue is above the treesThreshold, make the prefab trees instead
                if (noiseValue > _treesThreshold) prefab = _treesTilePrefab;
                
                // Only instantiate the tile if it is not water
                GameObject tile = Instantiate(prefab, position, Quaternion.identity);
            }
        }
    }
    
}
