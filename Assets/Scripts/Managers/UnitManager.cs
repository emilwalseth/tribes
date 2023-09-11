using Characters;
using Tiles;
using UnityEngine;

namespace Managers
{
    
    public class UnitManager : MonoBehaviour
    {
    
        public static UnitManager Instance { get; private set; }

        [SerializeField] private Unit _unitPrefab;
        [SerializeField] private Character _characterPrefab;
        [SerializeField] private CharacterData _heroData;
        [SerializeField] private CharacterData _minionData;

        private void Awake() => Instance = this;


        public Unit SpawnUnit(TileScript spawnTile, Unit splitFrom = null)
        {
            Unit spawnedUnit = SpawnUnitInternal(spawnTile);

            if (splitFrom)
            {
                spawnedUnit.RecentSplitUnit = splitFrom;
                splitFrom.RecentSplitUnit = spawnedUnit;
            }
            
            spawnedUnit.SetCurrentTile(spawnedUnit.transform.position);
            return spawnedUnit;
        }

        private Unit SpawnUnitInternal(TileScript spawnTile)
        {
            Vector3 spawnPosition = spawnTile.transform.position;
            Unit spawnedUnit = Instantiate(_unitPrefab, spawnPosition, Quaternion.identity);
            return spawnedUnit;
            
        }

        public Character SpawnHero(TileScript spawnTile)
        {
            return SpawnCharacter(spawnTile, _heroData);
        }
        
        public Character SpawnMinion(TileScript spawnTile)
        {
            return SpawnCharacter(spawnTile, _minionData);
        }

        private Character SpawnCharacter(TileScript spawnTile, CharacterData characterData)
        {
            Unit unit = SpawnUnitInternal(spawnTile);

            Character character = Instantiate(_characterPrefab, spawnTile.transform);
            character.SetCharacterData(characterData);
            unit.AddCharacter(character);
            character.CurrentUnit = unit;
            character.transform.parent = unit.transform;
            
            unit.SetCurrentTile(spawnTile.transform.position);

            return character;
        }
    
    }
}
