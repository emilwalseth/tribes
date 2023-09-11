using System.Collections;
using System.Collections.Generic;
using Characters;
using Tiles;
using UnityEngine;


namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        
        // Public Getter
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Link delegates
            EventManager.Instance.onMapGenerated += () => StartCoroutine(SpawnHero());
        }
        
        private static IEnumerator SpawnHero()
        {
            yield return null;
            
            // Spawn the hero at a random tile
            TileScript tile = MapManager.Instance.GetRandomGrassTile();
            TileManager.Instance.CreateTown(tile);

            List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(tile.gameObject);
            
            TileScript spawnTile = neighbors[Random.Range(0, neighbors.Count)];
            neighbors.Remove(spawnTile);
            TileManager.Instance.SetGrass(spawnTile);
            
            TileScript forestTile = neighbors[Random.Range(0, neighbors.Count)];
            TileManager.Instance.SetForest(forestTile);
            
            Character hero = UnitManager.Instance.SpawnHero(spawnTile);
            EventManager.Instance.onHeroSpawned?.Invoke(hero);
            
        }


    }
}
