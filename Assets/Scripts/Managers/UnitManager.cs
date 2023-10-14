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
            TileScript startTile = 
                splitFrom.CurrentNavigationPath.Count == 0 ? 
                    splitFrom.GetCurrentTile() : 
                    MapManager.Instance.GetTileAtPosition(splitFrom.CurrentNavigationPath[0]);
            
            Unit spawnedUnit = SpawnUnitInternal(splitFrom.transform.position, startTile, splitFrom.TeamIndex);
            
            spawnedUnit.RecentSplitUnit = splitFrom;
            splitFrom.RecentSplitUnit = spawnedUnit;
            
            return spawnedUnit;
        }

        private Unit SpawnUnitInternal(Vector3 spawnPos, TileScript startTile, int teamIndex)
        {
            Unit spawnedUnit = Instantiate(_unitPrefab, spawnPos, Quaternion.identity);
            spawnedUnit.TeamIndex = teamIndex;
            TeamManager.Instance.GetTeam(teamIndex).Units.Add(spawnedUnit);
            spawnedUnit.SetLastTile(startTile.transform.position);
            spawnedUnit.SetCurrentTile(startTile.transform.position);
            return spawnedUnit;
            
        }

        public Character SpawnHero(TileScript spawnTile, int teamIndex)
        {
            return SpawnCharacter(spawnTile, _heroData, teamIndex);
        }
        
        public Character SpawnMinion(TileScript spawnTile, int teamIndex)
        {
            return SpawnCharacter(spawnTile, _minionData, teamIndex);
        }

        private Character SpawnCharacter(TileScript spawnTile, CharacterData characterData, int teamIndex)
        {
            Vector3 spawnPos = spawnTile.transform.position;
            Unit unit = SpawnUnitInternal(spawnPos, spawnTile, teamIndex);

            Character character = Instantiate(_characterPrefab, spawnTile.transform);
            character.transform.localRotation = Quaternion.Euler(0, -135, 0);

            character.SetCharacterData(characterData);
            character.CurrentUnit = unit;
            character.transform.parent = unit.transform;
            
            unit.AddCharacter(character);
            unit.SetNewOccupant(spawnTile);
            unit.SetCurrentTile(spawnPos);

            AnimationManager.Instance.DoBounceAnim(character.gameObject);

            return character;
        }

    }
}
