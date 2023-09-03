using Characters;
using Tiles;
using UnityEngine;

namespace Managers
{
    
    public class UnitManager : MonoBehaviour
    {
    
        public static UnitManager Instance { get; private set; }

        [SerializeField] private Unit _unitPrefab;
        [SerializeField] private Character _heroPrefab;

        private void Awake() => Instance = this;


        private Unit SpawnUnit(TileScript spawnTile)
        {
            Vector3 spawnPosition = spawnTile.transform.position;
            Unit spawnedUnit = Instantiate(_unitPrefab, spawnPosition, Quaternion.identity);
            spawnedUnit.SetCurrentTile(spawnPosition);
            return spawnedUnit;
        }

        public Character SpawnHero()
        {
            TileScript tile = MapManager.Instance.GetRandomGrassTile();
            Unit heroUnit = SpawnUnit(tile);

            Character hero = Instantiate(_heroPrefab, tile.transform);
            heroUnit.AddCharacter(hero);
            hero.CurrentUnit = heroUnit;
            hero.transform.parent = heroUnit.transform;
            EventManager.Instance.onHeroSpawned?.Invoke(hero);

            return hero;
        }
    
    }
}
