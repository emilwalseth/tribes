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


        public Unit SplitUnit(Unit splitFrom)
        {
            
            Unit spawnedUnit = SpawnUnitInternal(splitFrom.transform.position, splitFrom.GetCurrentTile());
            
            spawnedUnit.RecentSplitUnit = splitFrom;
            splitFrom.RecentSplitUnit = spawnedUnit;
            
            return spawnedUnit;
        }

        private Unit SpawnUnitInternal(Vector3 spawnPos, TileScript startTile)
        {
            Unit spawnedUnit = Instantiate(_unitPrefab, spawnPos, Quaternion.identity);
            spawnedUnit.SetCurrentTile(startTile.transform.position);
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
            Unit unit = SpawnUnitInternal(spawnTile.transform.position, spawnTile);

            Character character = Instantiate(_characterPrefab, spawnTile.transform);
            character.SetCharacterData(characterData);
            unit.AddCharacter(character);
            character.CurrentUnit = unit;
            character.transform.parent = unit.transform;
            unit.SetNewOccupant(spawnTile);

            return character;
        }
    
    }
}
