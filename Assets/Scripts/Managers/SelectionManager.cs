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


        [SerializeField] private SelectionObject _tileSelectionObjectPrefab;
        [SerializeField] private GameObject _characterSelectionObjectPrefab;
        [SerializeField] private GameObject _unitSelectionObjectPrefab;

        public List<TileScript> SelectedTiles { get; private set; } = new();
        public Character SelectedCharacter { get; private set; }
        public Unit SelectedUnit { get; private set; }


        private readonly List<SelectionObject> _currentTileSelectionObjects = new();
        private GameObject _currentCharacterSelection;
        private GameObject _currentUnitSelection;

        

        public void DeselectAll()
        {
            DeselectTiles();
            DeselectCharacter();
            DeselectUnit();
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
            if (!SelectedCharacter) return;
            SelectedCharacter.OnDeselected();
            SelectedCharacter = null;
            UIManager.instance.CloseMenu();
            if(_currentCharacterSelection)
                Destroy(_currentCharacterSelection);
        }
        
        public void DeselectUnit()
        {
            if (!SelectedUnit) return;
            SelectedUnit = null;
            UIManager.instance.CloseMenu();
            if(_currentUnitSelection)
                Destroy(_currentUnitSelection);
            
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

        private void MakeCharacterSelection(Character character)
        {
            if (_currentUnitSelection)
                Destroy(_currentUnitSelection);
            
            Vector3 pos = character.CurrentUnit.transform.position + character.transform.localPosition + new Vector3(0,0.01f, 0);;
            GameObject selectionObject = Instantiate(_characterSelectionObjectPrefab, pos, Quaternion.identity);
            selectionObject.transform.SetParent(character.transform);
            _currentCharacterSelection = selectionObject;
        }
        
        private void MakeUnitSelection(Unit unit)
        {
            if (_currentUnitSelection)
                Destroy(_currentUnitSelection);
            
            Vector3 pos = unit.transform.position + new Vector3(0,0.01f, 0);
            GameObject selectionObject = Instantiate(_unitSelectionObjectPrefab, pos, Quaternion.identity);
            selectionObject.transform.SetParent(unit.transform);
            _currentUnitSelection = selectionObject;
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

            TileScript currentTile = unit.GetCurrentTile();
            
            if (currentTile)
            {   
                SelectTilesInRadius(unit.GetUnitWalkRadius(), currentTile, true);
            }
            
            SelectedUnit = unit;
            AnimationManager.Instance.DoBounceAnim(SelectedUnit.gameObject);
            MakeUnitSelection(SelectedUnit);
        }
    
        public void SelectCharacter(Character character)
        {
            TileScript currentTile = character.GetCurrenTile();

            if (!character || character == SelectedCharacter)
            {
                DeselectAll();
                return;
            }
            
            if (!SelectedUnit && SelectedUnit != character.CurrentUnit && character.CurrentUnit.CharactersInUnit.Count > 1)
            {
                DeselectCharacter();
                SelectUnit(character.CurrentUnit);
            }
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
                TryOpenMenu(character);
                MakeCharacterSelection(character);
            }
        }
        
        
        private void TryOpenMenu(Character character)
        {
            if (!character) return;
            TileScript currentTile = character.GetCurrenTile();
            if (!currentTile) return;
            
            if (character.CurrentUnit.State == UnitState.Idle)
            {
                UIManager.instance.OpenContextMenu(currentTile);
            }
        }
        
        public void UpdateCharacterSelection()
        {
            if (SelectedCharacter)
            {
                TileScript currentTile = SelectedCharacter.GetCurrenTile();
                
                if (currentTile) SelectTilesInRadius(SelectedCharacter.CharacterData.WalkRadius, currentTile, true);
                TryOpenMenu(SelectedCharacter);
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

        public void SelectTiles(List<TileScript> tiles)
        {
            DeselectTiles();

            SelectedTiles = tiles;

            foreach (TileScript tile in tiles)
            {
            
                List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(tile.gameObject, true);

                int selectionIndex = 0;

                Vector3 offset = new Vector3(0, 0.01f, 0);
            
                // Make a selection object
                SelectionObject selectionObject = Instantiate(_tileSelectionObjectPrefab, tile.transform.position + offset, Quaternion.identity);
                selectionObject.transform.parent = tiles[0].transform;

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
