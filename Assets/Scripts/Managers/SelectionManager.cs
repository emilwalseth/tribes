using System.Collections.Generic;
using System.Linq;
using Characters;
using Selection;
using Tiles;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class SelectionManager : MonoBehaviour
    {
    
        public static SelectionManager Instance { get; private set; }

        private void Awake() => Instance = this;


        [SerializeField] private SelectionObject _tileSelectionPrefab;
        [SerializeField] private GameObject _teamCharacterMarkerPrefab;
        [SerializeField] private GameObject _enemyCharacterMarkerPrefab;
        [SerializeField] private GameObject _teamUnitMarkerPrefab;
        [SerializeField] private GameObject _enemyUnitMarkerPrefab;

        public List<TileScript> SelectedTiles { get; private set; } = new();
        public Character SelectedCharacter { get; private set; }
        public Unit SelectedUnit { get; private set; }
        public GameObject MarkedEnemy { get; private set; }


        private readonly List<SelectionObject> _currentTileSelectionObjects = new();
        private GameObject _currentTeamMarker;
        private GameObject _currentEnemyMarker;

        

        public void DeselectAll()
        {
            DeselectTiles();
            DeselectCharacter();
            DeselectUnit();
            DeselectEnemy();
        }

        public void DeselectTiles()
        {
            if (SelectedTiles.Count != 0)
            {
                TileScript tile = SelectedTiles[0];
                if (tile)
                {
                    tile.OnDeselected();
                }
            }
            DestroySelectionObjects();
        }
        
        public void DeselectCharacter()
        {
            
            DeselectEnemy();
            if (!SelectedCharacter) return;
            SelectedCharacter.OnDeselected();
            SelectedCharacter = null;
            UIManager.instance.CloseMenu();
            ClearTeamMarker();
        }
        
        public void DeselectUnit()
        {
            DeselectEnemy();
            if (!SelectedUnit) return;
            SelectedUnit = null;
            UIManager.instance.CloseMenu();
            ClearTeamMarker();
            
        }

        public void DeselectEnemy()
        {
            ClearEnemyMarker();
            MarkedEnemy = null;
        }

        public void SelectTilesInRadius(int radius, TileScript centerTile, bool onlyWalkable = false)
        {
            List<TileScript> selectTiles = new() { centerTile };
            if (radius != 0)
            {
                selectTiles = GetRadius(radius, centerTile);
                if (onlyWalkable)
                {
                    selectTiles = selectTiles.Where(tile => tile.TileData.IsWalkable).ToList();
                }
            }
            SelectTiles(selectTiles);
        }

        private void DestroySelectionObjects()
        {
            foreach (SelectionObject selectionObject in _currentTileSelectionObjects.Where(selectionObject => selectionObject))
            {
                Destroy(selectionObject.gameObject);
            }
            
            _currentTileSelectionObjects.Clear();
        }
        
        private void MarkEnemyCharacter(Character enemyCharacter)
        {
            ClearEnemyMarker();
            
            Vector3 pos = enemyCharacter.CurrentUnit.transform.position + enemyCharacter.transform.localPosition + new Vector3(0,0.01f, 0);;
            GameObject selectionObject = Instantiate(_enemyCharacterMarkerPrefab, pos, Quaternion.identity);
            selectionObject.transform.SetParent(enemyCharacter.transform);
            _currentEnemyMarker = selectionObject;
        }
        
        private void MarkEnemyUnit(Unit enemyUnit)
        {
            ClearEnemyMarker();
            
            Vector3 pos = enemyUnit.transform.position + new Vector3(0,0.01f, 0);;
            GameObject selectionObject = Instantiate(_enemyUnitMarkerPrefab, pos, Quaternion.identity);
            selectionObject.transform.SetParent(enemyUnit.transform);
            _currentEnemyMarker = selectionObject;
        }

        private void MarkTeamCharacter(Character character)
        {
            ClearTeamMarker();
            
            Vector3 pos = character.CurrentUnit.transform.position + character.transform.localPosition + new Vector3(0,0.01f, 0);;
            GameObject selectionObject = Instantiate(_teamCharacterMarkerPrefab, pos, Quaternion.identity);
            selectionObject.transform.SetParent(character.transform);
            _currentTeamMarker = selectionObject;
        }
        
        private void MarkTeamUnit(Unit unit)
        {
            ClearTeamMarker();
            
            Vector3 pos = unit.transform.position + new Vector3(0,0.01f, 0);
            GameObject selectionObject = Instantiate(_teamUnitMarkerPrefab, pos, Quaternion.identity);
            selectionObject.transform.SetParent(unit.transform);
            _currentTeamMarker = selectionObject;
        }

        private void ClearTeamMarker()
        {
            if (_currentTeamMarker)
                Destroy(_currentTeamMarker);
        }

        private void ClearEnemyMarker()
        {
            if (_currentEnemyMarker)
                Destroy(_currentEnemyMarker);
        }

        public Unit GetSelectedUnit()
        {
            if (SelectedUnit) return SelectedUnit;
            return SelectedCharacter ? SelectedCharacter.CurrentUnit : null;
        }
        
        public List<TileScript> GetRadius(int radius, TileScript centerTile)
        {

            List<TileScript> tiles = new(){centerTile};
        
            List<TileScript> checkedTiles = new();
        

            for (int i = 0; i < radius; i++)
            {
                int tileCount = tiles.Count;
                for (int j = 0; j < tileCount; j++)
                { 
                    TileScript tile = tiles[j];
                    if (checkedTiles.Contains(tile)) continue;
                
                    List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(tile.gameObject);
                
                    foreach (TileScript neighbor in neighbors)
                    {
                        if (neighbor && !tiles.Contains(neighbor))
                        {
                            tiles.Add(neighbor);
                        }
                    }
                    checkedTiles.Add(tile);
                }
            }
        
            return tiles;
        }

        public TileScript GetSelectedTile()
        {
            if (SelectedTiles.Count == 0) return null;
            return SelectedTiles[0];
        }

        public void SelectUnit(Unit unit)
        {
            DeselectUnit();
            TileScript currentTile = unit.GetCurrentTile();
            
            if (currentTile)
            {   
                SelectTilesInRadius(unit.GetUnitWalkRadius(), currentTile, true);
            }
            
            SelectedUnit = unit;
            AnimationManager.Instance.DoBounceAnim(unit.gameObject);
            MarkTeamUnit(SelectedUnit);
        }
    
        public void SelectCharacter(Character character)
        {
            // Deselect if already selected or not valid
            if (!character || character == SelectedCharacter)
            {
                DeselectAll();
                return;
            }
            // If this character is on our team, do this
            if (character.CurrentUnit.TeamIndex == 0)
            {
                SelectTeamCharacter(character);
            }
            // This means the character is not on our team, do other
            else
            {
                SelectEnemyCharacter(character);
            }
        }

        private void SelectTeamCharacter(Character character)
        {
            
            TileScript currentTile = character.GetCurrenTile();
            
            // If we do not have a selected unit, select the unit instead of the character
            if (!SelectedUnit && SelectedUnit != character.CurrentUnit && character.CurrentUnit.CharactersInUnit.Count > 1)
            {
                DeselectCharacter();
                SelectUnit(character.CurrentUnit);
            }
            // If the unit is selected, select the character instead
            else
            {
                DeselectUnit();

                if (currentTile)
                {
                    SelectTilesInRadius(character.CharacterData.WalkRadius, currentTile, true);
                }
                
                character.OnSelected();
                DeselectCharacter();
                SelectedCharacter = character;
                TryOpenTeamMenu(character);
                MarkTeamCharacter(character);
            }
        }

        private void SelectEnemyCharacter(Character character)
        {
            
            if (MarkedEnemy && MarkedEnemy == character.gameObject)
            {
                DeselectEnemy();
                return;
            }
            
            // If we do not have a selected unit, select the unit instead of the character
            if (!MarkedEnemy && MarkedEnemy != character.CurrentUnit.gameObject && character.CurrentUnit.CharactersInUnit.Count > 1)
            {
                
                print("Selecting Unit");
                MarkedEnemy = character.CurrentUnit.gameObject;
                TryOpenEnemyMenu(character);
                MarkEnemyUnit(character.CurrentUnit);
            }
            // If the unit is selected, select the character instead
            else
            {
                print("Selecting Character");

                DeselectUnit();
                MarkedEnemy = character.gameObject;
                TryOpenEnemyMenu(character);
                MarkEnemyCharacter(character);
            }
        }
        
        
        private void TryOpenTeamMenu(Character character)
        {
            if (!character) return;
            TileScript currentTile = character.GetCurrenTile();
            if (!currentTile) return;
            
            if (character.CurrentUnit.State == UnitState.Idle)
            {
                UIManager.instance.OpenContextMenu(currentTile.TileData.MenuOptions);
            }
        }
        
        private void TryOpenEnemyMenu(Character character)
        {
            if (!character) return;

            if (SelectedCharacter && IsTileSelected(SelectedCharacter.GetCurrenTile()))
            {
                UIManager.instance.OpenContextMenu(character.EnemyMenuOptions);
            }
        }
        
        public void UpdateCharacterSelection()
        {
            if (SelectedCharacter)
            {
                TileScript currentTile = SelectedCharacter.GetCurrenTile();
                
                if (currentTile) SelectTilesInRadius(SelectedCharacter.CharacterData.WalkRadius, currentTile, true);
                TryOpenTeamMenu(SelectedCharacter);
            }
            else if (SelectedUnit)
            {
                TileScript currentTile = SelectedUnit.GetCurrentTile();
                
                if (currentTile) SelectTilesInRadius(SelectedUnit.GetUnitWalkRadius(), currentTile, true);
            }
        }
        public bool IsTileSelected(TileScript tile)
        {
            return SelectedTiles.Contains(tile);
        }

        private void SelectTiles(List<TileScript> tiles)
        {
            DeselectTiles();

            SelectedTiles = tiles;

            foreach (TileScript tile in tiles)
            {
            
                List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(tile.gameObject, true);

                int selectionIndex = 0;

                Vector3 offset = new Vector3(0, 0.01f, 0);
            
                // Make a selection object
                SelectionObject selectionObject = Instantiate(_tileSelectionPrefab, tile.transform.position + offset, Quaternion.identity);
                selectionObject.transform.parent = tiles.Count == 1 ? tiles[0].transform : MapManager.Instance.transform;
                
                _currentTileSelectionObjects.Add(selectionObject);
            
            
                // Get the selection index by checking if the neighbors are in the selection
                for (int i = 0; i < neighbors.Count; i++)
                {
                    TileScript neighbor = neighbors[i];
                    if (!neighbor || !tiles.Contains(neighbor))
                    {
                        selectionIndex |= 1 << i;
                    }
                }
                selectionObject.SetSelectionIndex(selectionIndex);
            }
        }
    
    
    }
}
