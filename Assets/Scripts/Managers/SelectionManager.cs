using System.Collections.Generic;
using System.Linq;
using Characters;
using Selection;
using Tiles;
using UnityEngine;

namespace Managers
{
    public class SelectionManager : MonoBehaviour
    {
    
        public static SelectionManager Instance { get; private set; }

        private void Awake() => Instance = this;


        [SerializeField] private SelectionObject _selectionObjectPrefab;

        public List<TileScript> SelectedTiles { get; private set; } = new();
        public Character SelectedCharacter { get; private set; }


        private readonly List<SelectionObject> _currentSelectionObjects = new();


        public void Deselect()
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

        public void SelectTilesInRadius(int radius, TileScript centerTile)
        {
            List<TileScript> selectTiles = new() { centerTile };
            if (radius != 0)
            {
                selectTiles = GetRadius(radius, centerTile);
            }
            SelectTiles(selectTiles);
        }

        private void DestroySelectionObjects()
        {
            foreach (SelectionObject selectionObject in _currentSelectionObjects.Where(selectionObject => selectionObject))
            {
                Destroy(selectionObject.gameObject);
            }
            DeselectCharacter();

            _currentSelectionObjects.Clear();
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
    
        public void SelectCharacter(Character character)
        {
            if (!character || character == SelectedCharacter)
            {
                Deselect();
                return;
            }

            
            TileScript currentTile = character.GetCurrenTile();
            if (currentTile)
            {
                SelectTilesInRadius(character.CharacterData._selectionRadius, currentTile);
                TryOpenMenu(character);
                
            }
            
            character.OnSelected();
            DeselectCharacter();
            SelectedCharacter = character;
    
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
            if (!SelectedCharacter) return;

            Character character = SelectedCharacter;
            TileScript currentTile = character.GetCurrenTile();
            
            
            if (currentTile) SelectTilesInRadius(character.CharacterData._selectionRadius, currentTile);
            character.OnSelected();
            SelectedCharacter = character;
            TryOpenMenu(character);


        }
    
        private void DeselectCharacter()
        {
            if (!SelectedCharacter) return;
            SelectedCharacter.OnDeselected();
            SelectedCharacter = null;
            UIManager.instance.CloseMenu();
        }
    
        public bool IsTileSelected(TileScript tile)
        {
            return SelectedTiles.Contains(tile);
        }

        public void SelectTiles(List<TileScript> tiles)
        {
            Deselect();

            SelectedTiles = tiles;

            foreach (TileScript tile in tiles)
            {
            
                List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(tile.gameObject, true);

                int selectionIndex = 0;

                Vector3 offset = new Vector3(0, 0.01f, 0);
            
                // Make a selection object
                SelectionObject selectionObject = Instantiate(_selectionObjectPrefab, tile.transform.position + offset, Quaternion.identity);
                selectionObject.transform.parent = tiles[0].transform;

                _currentSelectionObjects.Add(selectionObject);
            
            
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
