using System;
using System.Collections.Generic;
using AI;
using Characters;
using Data.Actions;
using Data.Buildings;
using Data.GeneralTiles;
using Interfaces;
using Managers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Tiles
{
    public class TileScript : NavigationNode, IInteractable, IContextInterface
    {
        [SerializeField] private TileData _tileData;
        [SerializeField] private GameObject _fogTile;
        [SerializeField] private GameObject _mainTile;
        [SerializeField] private MeshFilter _groundMeshFilter;
        [SerializeField] private GameObject _secondLayer;
        [SerializeField] private TMP_Text _debugText;
        [SerializeField] private bool _debug = false;
  

        private GameObject _currentTile;
        private TileScript _townTile;
        private int _claimedByTeam = -1;

        public TileData TileData => _tileData;
        public GameObject CurrentTile => _currentTile;
        public TileScript TownTile => _townTile;
        public int ClaimedByTeam => _claimedByTeam;
        public bool IsExplored { get; set; } = true;

        public Unit Occupant { get; private set; }

        public UnityAction<Unit> onOccupantEntered;


        private void Start()
        {
            SetDebugText(0.ToString());
        }

        public void OnClicked()
        {
            PlayClickedAnimation();
            SelectionManager.Instance.Select(gameObject);

        }

        public void SetDebugText(string text)
        {
            if (!_debug)
            {
                _debugText.gameObject.SetActive(false);
                return;
            }
            
            if(!_debugText) return;
            _debugText.text = text;
        }
        
        public void Interact(Character character)
        {
            if (!_currentTile || !_currentTile.TryGetComponent(out ITileInterface tileInterface)) return;
            
            tileInterface.OnInteract(character);
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
            TileManager.Instance.SetTileGround(this);
        }
        
        public void SetTileData(TileData tileData, int teamIndex = -1)
        {
            _tileData = tileData;
            if (teamIndex != -1)
                _claimedByTeam = teamIndex;
            
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

        public void SetExplored(bool newExplored, bool force = false)
        {
            if (newExplored == IsExplored && !force) return;
            IsExplored = newExplored;
            _fogTile.SetActive(!IsExplored);
            _mainTile.SetActive(IsExplored);
            if (Occupant)
            {
                Occupant.SetTeamHidden(!newExplored);
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
                if (_currentTile && _currentTile.TryGetComponent(out ITileInterface tileInterface))
                    tileInterface.SetOwningTile(this);
                if (Occupant)
                    Occupant.SetState(CharacterState.Idle);
            }
            
            IsWalkable = _tileData.IsWalkable;
            NodeWeight = _tileData.MovementCost;
            TileManager.Instance.SetTileGround(this);
        }

        public void SetGround(Mesh groundType)
        {
            _groundMeshFilter.mesh = groundType;
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
            if (occupant)
            {
                onOccupantEntered?.Invoke(occupant);
            }
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

        public string GetLabel()
        {
            return _tileData.TileName;
        }

        public Sprite GetIcon()
        {
            return _tileData.TileIcon;
        }
        
        public bool IsResource()
        {
            return _currentTile && _currentTile.TryGetComponent(out ResourceTileScript resource);
        }
        
        public bool IsBuilding()
        {
            return _currentTile && _currentTile.TryGetComponent(out BuildingTileScript building);
        }
        
        public ResourceTileScript GetResource()
        {
            return _currentTile && _currentTile.TryGetComponent(out ResourceTileScript resource) ? resource : null;
        }
        
        public BuildingTileScript GetBuilding()
        {
            return _currentTile && _currentTile.TryGetComponent(out BuildingTileScript building) ? building : null;
        }

        public List<ContextButtonData> GetContextButtons()
        {

            List<ContextButtonData> contextButtons = new();
            BuildingTileScript currentBuilding = GetBuilding();

            if (TownTile && this != TownTile)
            {
                int teamIndex = TeamManager.Instance.GetMyTeamIndex();
                
                if (!currentBuilding)
                {
                    BuildingTileData building = TileManager.Instance.GetTownData().House;
                    contextButtons.Add(UIManager.instance.MakeBuildButton(this, teamIndex, building));
                }
                else
                {
                    List<BuildingTileData> upgrades = currentBuilding.BuildingTileData.UpgradeOptions;
                    foreach (BuildingTileData building in upgrades)
                    {
                        contextButtons.Add(UIManager.instance.MakeBuildButton(this, teamIndex, building));
                    }
                }
            }
            
            return contextButtons;
        }
    }
}
