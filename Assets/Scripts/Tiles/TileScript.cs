using System;
using System.Collections.Generic;
using Characters;
using Data.Actions;
using Data.GeneralTiles;
using Interfaces;
using Managers;
using UnityEngine;

namespace Tiles
{
    public class TileScript : NavigationNode, IInteractable
    {
        [SerializeField] private TileData _tileData;
        [SerializeField] private MeshFilter _groundLayer;
        [SerializeField] private GameObject _secondLayer;
  

        private GameObject _currentTile;
        private TileScript _townTile;

        public TileData TileData => _tileData;
        public GameObject CurrentTile => _currentTile;
        public TileScript TownTile => _townTile;
        public Unit Occupant { get; private set; }

        public void OnClicked()
        {
            PlayClickedAnimation();

            Character selectedCharacter = SelectionManager.Instance.SelectedCharacter;
            Unit selectedUnit = SelectionManager.Instance.SelectedUnit;
            
            Unit chosenUnit = selectedUnit;
            
            if (!chosenUnit && selectedCharacter)
            {
                if (selectedCharacter.GetCurrenTile() != this)
                {
                    selectedCharacter.CurrentUnit.SplitFromUnit(selectedCharacter);
                    chosenUnit = selectedCharacter.CurrentUnit;
                }
            }
            if (chosenUnit)
            {
                TileScript currentTile = chosenUnit.GetCurrentTile();
                if (currentTile != this)
                {
                    if (SelectionManager.Instance.IsTileSelected(this))
                    {
                        chosenUnit.NavigateToTile(this);
                        UIManager.instance.CloseMenu();
                        return;
                    }
                }
            }
            
            SelectionManager.Instance.SelectTilesInRadius(GetSelectionRadius(), this);

            if (TownTile)
            {
                UIManager.instance.OpenBuildMenu(this);
            }
            else
            {
                UIManager.instance.CloseMenu();
            }
        }
        
        public void Interact()
        {
            if (!_currentTile || !_currentTile.TryGetComponent(out ITileInterface tileInterface)) return;
            
            tileInterface.OnInteract();
            PlayInteractAnimation();
        }

        public void OnSelected()
        {
            
        }

        public void OnDeselected()
        {
            
        }

        public void SetTown(TileScript townTile)
        {
            _townTile = townTile;
        }
        
        public void SetTileData(TileData tileData)
        {
            _tileData = tileData;
            UpdateTile();
            RunActions(TileData.OnTilePlaced);
        }


        private void RunActions(List<ScriptableAction> actions)
        {
            foreach (ScriptableAction action in actions)
            {
                action.Execute(gameObject);
            }
        }

        private void UpdateTile()
        {
            if (!_tileData) return;
            
            ClearTile();
            
            if (_tileData.Tile)
            {
                _currentTile = Instantiate(_tileData.Tile, transform);
                _currentTile.transform.parent = _secondLayer.transform;
            }

            IsWalkable = _tileData.IsWalkable;
            NodeWeight = _tileData.MovementCost;
            TileManager.Instance.SetTileGround(this);
        }

        public void SetGround(Mesh groundType)
        {
            _groundLayer.mesh = groundType;
        }

        public void ClearTile()
        {
            foreach (Transform child in _secondLayer.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        public void SetOccupant(Unit occupant)
        {
            Occupant = occupant;
        }
        public void ClearOccupant()
        {
            Occupant = null;
        }

        public int GetSelectionRadius()
        {
            if (_tileData)
            {
                return _tileData.SelectionRadius;
            }

            return 0;
        }

        public void PlayInteractAnimation()
        {
            AnimationManager.Instance.DoBounceAnim(_secondLayer, 0.25f);
        }
        
        private void PlayClickedAnimation()
        {
            AnimationManager.Instance.DoBounceAnim(gameObject, 0.25f);
        }
    }
}
