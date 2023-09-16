using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Data.GeneralTiles;
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
            TileScript tile = MapManager.Instance.GetRandomTile(GroundType.Grass);
            TileManager.Instance.CreateTown(tile);

            List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(tile.gameObject);
            
            List<TileScript> radius = SelectionManager.Instance.GetRadius(3, tile);
            radius.RemoveAll(item => neighbors.Contains(item) || !item.TileData.IsWalkable);
            
            TileScript spawnTile = neighbors[Random.Range(0, neighbors.Count)];
            neighbors.Remove(spawnTile);
            TileManager.Instance.SetGrass(spawnTile);
            
            TileScript forestTile = neighbors[Random.Range(0, neighbors.Count)];
            TileManager.Instance.SetForest(forestTile);
            
            Character hero = UnitManager.Instance.SpawnHero(spawnTile, 0);
            EventManager.Instance.onHeroSpawned?.Invoke(hero);

            TileScript enemyTile = radius[Random.Range(0, radius.Count)];
            Character enemy = UnitManager.Instance.SpawnHero(enemyTile, 1);
            
            
        }


    }
}
