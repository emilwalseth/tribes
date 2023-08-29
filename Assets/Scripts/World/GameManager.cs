using System;
using System.Collections;
using Characters;
using UnityEngine;
using UnityEngine.Events;

namespace World
{
    public class GameManager : MonoBehaviour
    {

        [SerializeField] private MapGenerator _map;
        [SerializeField] private Character _heroCharacter;


        public UnityAction<GameObject> onHeroSpawned;
    
        // Public Getter
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Link delegates
            _map.onMapGenerated += () => StartCoroutine(SpawnHero());
        }
    
    
        private IEnumerator SpawnHero()
        {
            yield return null;
            // Spawn the hero at a random tile
            TileScript tile = _map.GetRandomGrassTile();
            Vector3 pos = tile.transform.position;
            Character hero = Instantiate(_heroCharacter, pos, Quaternion.identity);
            hero.CurrentTile = tile.gameObject;
            onHeroSpawned?.Invoke(hero.gameObject);

        }
    
    }
}
