using System.Collections.Generic;
using System.Linq;
using Characters;
using Interfaces;
using Selection;
using Tiles;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

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
        

        private GameObject _currentSelected;
        private readonly List<SelectionObject> _currentTileSelectionObjects = new();
        private GameObject _currentMarker;

        

        public void DeselectAll()
        {
            DeselectSelected();
            DeselectTiles();
        }

        public void DeselectSelected()
        {
            ClearMarker();
            SetAsNewSelected(null);
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
        
        public GameObject GetSelected()
        {
            return _currentSelected;
        }

        public bool IsSelected(GameObject checkObject)
        {
            return _currentSelected == checkObject;
        }

        public Unit GetSelectedUnit()
        {
            if (!_currentSelected) return null;
            
            if (_currentSelected.TryGetComponent(out Unit unit))
                return unit;
            if (_currentSelected.TryGetComponent(out Character character))
                return character.CurrentUnit;
            
            return null;
        }

        public Character GetSelectedCharacter()
        {
            if (!_currentSelected)
                return null;
            
            return _currentSelected.TryGetComponent(out Character character) ? character : null;
        }

        private void SetAsNewSelected(GameObject newSelected)
        {
            if (_currentSelected && _currentSelected.TryGetComponent(out IInteractable oldInteractable))
            {
                oldInteractable.OnDeselected();
            }

            if (newSelected)
            {
                if (newSelected.TryGetComponent(out IInteractable newInteractable))
                {
                    newInteractable.OnSelected();
                }
                UIManager.instance.OpenContextMenu(newSelected);
            }
            else
                UIManager.instance.CloseContextMenu();
            
            _currentSelected = newSelected;
        }


        public void Select(GameObject newSelected)
        {
            
            GameObject oldSelected = _currentSelected;
            
            
            // Check if selecting a tile
            if (newSelected.TryGetComponent(out TileScript tile))
            {
                // We are selecting a tile
                
                // If it is the same tile as before, deselect
                if (oldSelected == tile.gameObject)
                {
                    DeselectAll();
                    return;
                }
                
                // If this tile is already marked as selected, check if we can move here
                if (SelectedTiles.Contains(tile))
                {
                    Unit selectedUnit = GetSelectedUnit();
                    
                    Character selectedCharacter = GetSelectedCharacter();
                    if (selectedCharacter && selectedCharacter.CurrentUnit.TeamIndex == 0)
                    {
                        if (tile != selectedCharacter.GetCurrentTile() || selectedCharacter.State == CharacterState.Moving)
                        {
                            selectedCharacter.CurrentUnit.SplitFromUnit(selectedCharacter);
                            selectedUnit = selectedCharacter.CurrentUnit;
                        }
                    }
                    
                    if (selectedUnit && selectedUnit.TeamIndex == 0)
                    {
                        if (tile != selectedUnit.GetCurrentTile() || selectedUnit.CharactersInUnit[0].State == CharacterState.Moving)
                        {
                            selectedUnit.NavigateToTile(tile);
                            UIManager.instance.CloseContextButtons();
                            return;
                        }
                    }
                }
                
                ClearMarker();
                SetAsNewSelected(tile.gameObject);
                SelectTilesInRadius(tile.GetSelectionRadius(), tile);
            }
            
            
            // Check if selecting a character
            else if (newSelected.TryGetComponent(out Character character))
            {
                // We are selecting a character
                
                bool isEnemy = character.CurrentUnit.TeamIndex != 0;
                
                // If it is the same character as before, we deselect
                if (oldSelected == newSelected)
                {
                    DeselectAll();
                }
                // If the old selected is a unit
                else if (oldSelected && oldSelected.TryGetComponent(out Unit oldSelectedUnit) && oldSelectedUnit == character.CurrentUnit)
                {
                    // Then select the character instead
                    MarkCharacter(character, isEnemy);
                    SetAsNewSelected(character.gameObject);
                        
                    // Also select the walkable tiles if it is not an enemy
                    if (!isEnemy)
                        SelectTilesInRadius(character.CharacterData.WalkRadius, character.CurrentUnit.GetCurrentTile(), true);
                    else
                        DeselectTiles();
                }
                // If it is not, we select the unit of this character
                else
                {
                    Unit characterUnit = character.CurrentUnit;

                    if (characterUnit.CharactersInUnit.Count > 1)
                    {
                        MarkUnit(characterUnit, isEnemy);
                        SetAsNewSelected(characterUnit.gameObject);
                    }
                    else
                    {
                        Character unitCharacter = characterUnit.CharactersInUnit[0];
                        MarkCharacter(unitCharacter, isEnemy);
                        SetAsNewSelected(unitCharacter.gameObject);
                    }
                    
                    // Also select the walkable tiles if it is not an enemy
                    if (!isEnemy)
                        SelectTilesInRadius(characterUnit.Stats.WalkRadius, characterUnit.GetCurrentTile(), true);
                    else
                        DeselectTiles();
                }
                
            }
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
        
        private void MarkCharacter(Character character, bool isEnemy)
        {
            ClearMarker();
            
            Vector3 pos = character.CurrentUnit.transform.position + character.transform.localPosition + new Vector3(0,0.01f, 0);;
            GameObject selectionObject = Instantiate(isEnemy ? _enemyCharacterMarkerPrefab : _teamCharacterMarkerPrefab, pos, Quaternion.identity);
            selectionObject.transform.SetParent(character.transform);
            _currentMarker = selectionObject;
        }
        
        private void MarkUnit(Unit unit, bool isEnemy)
        {
            ClearMarker();

            // If there is only one character, we just mark that one instead
            if (unit.CharactersInUnit.Count == 1)
            {
                MarkCharacter(unit.CharactersInUnit[0], isEnemy);
                return;
            }
            
            Vector3 pos = unit.transform.position + new Vector3(0,0.01f, 0);;
            GameObject selectionObject = Instantiate(isEnemy ? _enemyUnitMarkerPrefab : _teamUnitMarkerPrefab, pos, Quaternion.identity);
            selectionObject.transform.SetParent(unit.transform);
            _currentMarker = selectionObject;
        }

       

        private void ClearMarker()
        {
            if (_currentMarker)
                Destroy(_currentMarker);
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
        
        public void UpdateCharacterSelection()
        {
            Character selectedCharacter = GetSelectedCharacter();
            Unit selectedUnit = GetSelectedUnit();
            if (selectedCharacter)
            {
                TileScript currentTile = selectedCharacter.GetCurrentTile();
                
                if (currentTile) SelectTilesInRadius(selectedCharacter.CharacterData.WalkRadius, currentTile, true);
            }
            
            else if (selectedUnit)
            {
                TileScript currentTile = selectedUnit.GetCurrentTile();
                
                if (currentTile) SelectTilesInRadius(selectedUnit.Stats.WalkRadius, currentTile, true);
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
