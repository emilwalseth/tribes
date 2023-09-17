using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
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
        
        [SerializeField] private AIController _aiControllerPrefab;

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

            for (int i = 0; i < 4; i++)
            {
                // Create Team
                TeamManager.Instance.AddTeam(i);
                // Spawn AI controller
                AIController aiController = Instantiate(_aiControllerPrefab, Vector3.zero, Quaternion.identity);
                aiController.SetTeamIndex(i);
                
                TileScript spawnTile = MapManager.Instance.GetBestSpawnPoint();
                TileManager.Instance.CreateTown(spawnTile, i);
                Character character = UnitManager.Instance.SpawnHero(spawnTile, i);
            }
            EventManager.Instance.onHeroSpawned?.Invoke(TeamManager.Instance.GetTeam(0).Units[0].CharactersInUnit[0]);
        }


    }
}
