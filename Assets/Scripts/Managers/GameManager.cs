using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Characters;
using Data.GeneralTiles;
using Tiles;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        
        // Public Getter
        public static GameManager Instance { get; private set; }
        
        [SerializeField] private AIController _aiControllerPrefab;
        [SerializeField] private GameObject _graveStonePrefab;

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
        
        private IEnumerator SpawnHero()
        {
            yield return null;

            for (int i = 0; i < 3; i++)
            {
                // Create Team
                TeamManager.Instance.AddTeam(i);
                
                if (i != 0)
                {
                    // Spawn AI controller
                    AIController aiController = Instantiate(_aiControllerPrefab, Vector3.zero, Quaternion.identity);
                    aiController.SetTeamIndex(i);
                }
                
                TileScript spawnTile = MapManager.Instance.GetBestSpawnPoint();
                TileScript characterSpawn = spawnTile;
                
                List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(spawnTile.gameObject);
                foreach (TileScript neighbor in neighbors.Where(neighbor => neighbor && neighbor.TileData.IsWalkable))
                {
                    characterSpawn = neighbor;
                    break;
                }
                
                TileManager.Instance.CreateTown(spawnTile, i);
                Character character = UnitManager.Instance.SpawnHero(characterSpawn, i);
            }
            EventManager.Instance.onHeroSpawned?.Invoke(TeamManager.Instance.GetTeam(0).Units[0].CharactersInUnit[0]);
        }

        public void SpawnGravestone(Vector3 position)
        {
            Quaternion random = Quaternion.Euler(0, Random.Range(0, 360), 0);
            GameObject graveStone = Instantiate(_graveStonePrefab, position, random);
        }
        
        public void GameOver(int forTeam)
        {
            if (forTeam == 0)
            {
                // Restart game
                StartCoroutine(GameOverSequence());   
            }
            else if (TeamManager.Instance.TeamCount == 1)
            {
                print("Team " + forTeam + " wins!");
            }

        }
        
        private IEnumerator GameOverSequence()
        {
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
