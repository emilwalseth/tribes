using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using UnityEngine;
using UnityEngine.Events;

namespace World
{
    public class GameManager : MonoBehaviour
    {

        [SerializeField] private MapGenerator _map;
        [SerializeField] private Character _heroCharacter;
        [SerializeField] private PathRenderer _pathRenderer;
        [SerializeField] private GameObject _flagPrefab;


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

        public PathRenderer MakePathRenderer(List<Vector3> pathPoints)
        {
            PathRenderer pathRenderer = Instantiate(_pathRenderer, new Vector3(0,0.1f,0), Quaternion.identity);
            pathRenderer.MakePath(pathPoints);
            
            GameObject flag = Instantiate(_flagPrefab, pathPoints[0], _flagPrefab.transform.rotation);
            flag.transform.parent = pathRenderer.transform;
            
            return pathRenderer;
        }
    
    }
}
