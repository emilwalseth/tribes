using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Data.GeneralTiles;
using Interfaces;
using Managers;
using Tiles;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using World;
using Random = UnityEngine.Random;

namespace Characters
{

    public struct UnitStats
    {
        public int WalkRadius { get; set; }
        public float WalkSpeed { get; set; }
    }
    
    public class Unit : MonoBehaviour, IContextInterface, IInteractable
    {
    
        [SerializeField] private CharacterMarker _unitMarker;

        private Vector3 _currentTilePosition;
        private Vector3 _lastTilePosition;
        private UnitStats _unitStats = new UnitStats();
        
        public UnitStats Stats => _unitStats;
        public int TeamIndex { get; set; } = 0;
        public PathRenderer CurrentPathRenderer { get; set; }
        public List<Vector3> CurrentNavigationPath { get; private set; }  = new ();
        
        private readonly List<Character> _charactersInUnit = new();
        public List<Character> CharactersInUnit => _charactersInUnit;

        public Unit RecentSplitUnit { get; set; }
        public AIBrain AI { get; set; }

        public UnityAction onReachedDestination;

        private void Start()
        {
            AI = GetComponent<AIBrain>();
        }

        public void Attack(Unit enemyUnit)
        {
            enemyUnit.SetMarker(MarkerType.Attack);
            AI.SetTarget(enemyUnit.gameObject);
            AI.SetState(AIState.Attack);
        }
        
        public void SetAttackTarget(Unit targetUnit)
        {
            foreach (Character character in CharactersInUnit)
            {
                character.SetAttackTarget(targetUnit);
            }
        }

        private void UpdateStats()
        {

            foreach (Character character in CharactersInUnit)
            {
                _unitStats.WalkSpeed = Mathf.Max(Stats.WalkSpeed, character.CharacterData.WalkSpeed);
                _unitStats.WalkRadius = Mathf.Max(Stats.WalkRadius, character.CharacterData.WalkRadius);

            }
            
        }
        
        public bool NavigateToTile(TileScript targetTile)
        {
            
            onReachedDestination = null;
            
            if (!targetTile) 
                return false;
            
            
            TileScript currentTile = GetCurrentTile();

            bool enemyOccupant = targetTile.Occupant && targetTile.Occupant.TeamIndex != TeamIndex;

            if (currentTile == targetTile)
            {
                if (CurrentNavigationPath.Count == 0) return false;
                
                Vector3 nextPosition = CurrentNavigationPath[0];
                SetCurrentTile(nextPosition);
                NavigateToTile(currentTile);
                return true;

            }
            
            List<NavigationNode> path = NavigationManager.FindPath(currentTile, targetTile);
            
            if (path == null)
                return false;
            if (enemyOccupant && path.Count <= 1)
                return true;
            if (!SetNewNavigationPath(path))
                return false;

            
            
            if (enemyOccupant)
            {
                Fallback();
            }
            else
            {
                targetTile.onOccupantEntered -= OnOccupantUpdated;
                targetTile.onOccupantEntered += OnOccupantUpdated;
            }
            
            ClearOccupant(currentTile);
            return true;
        }

        private void OnOccupantUpdated(Unit newOccupant)
        {
            if (newOccupant.TeamIndex == TeamIndex) return;
            
            Fallback();
            
        }
        
        
        public void SetMarker(MarkerType markerType)
        {
            if(!_unitMarker) return;
            _unitMarker.SetMarker(markerType);
        }

        private void Fallback()
        {
            print("Falling back");

            Vector3 nextPosition = CurrentNavigationPath[0];
            Vector3 targetPosition = CurrentNavigationPath[^1];

            TileScript fallbackTile;
            
            if ((nextPosition - targetPosition).magnitude < 0.1f)
            {
                TileScript targetTile = GetCurrentTile();
                SetCurrentTile(targetPosition);
                fallbackTile = targetTile;
            }
            else
            {
                Vector3 fallbackPos = CurrentNavigationPath[^2];
                fallbackTile = MapManager.Instance.GetTileAtPosition(fallbackPos);
            }
            
            if (fallbackTile.Occupant && fallbackTile.Occupant.TeamIndex == TeamIndex)
            {
                List<TileScript> tiles =  MapManager.Instance.GetTileNeighbors(GetCurrentTile().gameObject);
                foreach (TileScript tile in tiles.Where(tile => !tile.Occupant || tile.Occupant.TeamIndex == TeamIndex))
                {
                    fallbackTile = tile;
                    break;
                }
            }
            
            
            NavigateToTile(fallbackTile);
            
            MakePathRenderer();

        }

        private void MakePathRenderer()
        {
            NavigationManager.Instance.MakePathRenderer(this);
            bool isSelected = SelectionManager.Instance.GetSelectedUnit() == this;
            SetRenderPathVisibility(isSelected);
        }
        
        private bool SetNewNavigationPath(List<NavigationNode> newNavigationPath)
        {
            if (newNavigationPath == null) return false;

            List<Vector3> newPath = new();

            List<TileScript> radius = SelectionManager.Instance.GetRadius(Stats.WalkRadius + 1, GetCurrentTile());
            
            foreach (NavigationNode node in newNavigationPath)
            {
                if (node.TryGetComponent(out TileScript tileScript))
                    if (!radius.Contains(tileScript))
                    {
                        newPath.Clear();
                        break;
                    }
                newPath.Add(node.transform.position);
            }

            if (newPath.Count == 0) return false;
            
            CurrentNavigationPath = newPath;
            UpdateCharactersMoving(true);
            if (TeamIndex == 0)
                MakePathRenderer();
            return true;
        }
        
        
        private void OnArrivedTarget()
        {
            GetCurrentTile().onOccupantEntered -= OnOccupantUpdated;
            UpdateCharactersMoving(false);
            SetNewOccupant(GetCurrentTile());
            
            if (_charactersInUnit.Count == 1 && TeamIndex == TeamManager.Instance.GetMyTeamIndex())
            {
                Character character = CharactersInUnit[0];
                if (SelectionManager.Instance.IsSelected(character.gameObject))
                {
                    UIManager.instance.OpenContextMenu(character.gameObject);
                }
            }
            
            onReachedDestination?.Invoke();
        }


        public bool SplitFromUnit(Character character)
        {
            if (_charactersInUnit.Count <= 1) return false;
            if (!_charactersInUnit.Contains(character)) return false;
            
            CharactersInUnit.Remove(character);
            
            Unit newUnit = UnitManager.Instance.SplitUnit(this);
            character.SetUnit(newUnit);
            newUnit.AddCharacter(character);
            newUnit.RepositionCharacters();
            RepositionCharacters();
            DestroyIfEmpty();

            return true;
        }
 
        private void DestroyIfEmpty()
        {
            if (CharactersInUnit.Count != 0) return;
            
            if (CurrentPathRenderer)
                Destroy(CurrentPathRenderer.gameObject);

            TeamManager.Instance.GetTeam(TeamIndex).Units.Remove(this);
            Destroy(gameObject);
            
        }

        public void CombineWithUnit(Unit unit)
        {
            if (unit == RecentSplitUnit) return;
            


            Unit mainUnit = unit.CharactersInUnit.Count > CharactersInUnit.Count ? unit : this;
            Unit oldUnit = mainUnit == unit ? this : unit;
            
            
            Unit selectedUnit = SelectionManager.Instance.GetSelectedUnit();
            bool wasSelected = selectedUnit == this || selectedUnit == unit;
            bool selectOld = false;
            
            
            for (int i = oldUnit.CharactersInUnit.Count - 1; i >= 0; i--)
            {
                Character character = oldUnit.CharactersInUnit[i];
                if (mainUnit.CharactersInUnit.Count >= 7)
                {
                    List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(oldUnit.GetCurrentTile().gameObject)
                        .Where(tile => tile.TileData.IsWalkable).ToList();
                    
                    TileScript fallbackTile = oldUnit.GetLastTile();

                    if (fallbackTile == oldUnit.GetCurrentTile())
                    {
                        fallbackTile = neighbors[Random.Range(0, neighbors.Count)];
                    }
                    
                    oldUnit.RecentSplitUnit = mainUnit;
                    oldUnit.UpdateStats();
                    oldUnit.RepositionCharacters();
                    oldUnit.NavigateToTile(fallbackTile);
                    
                    if (selectedUnit == oldUnit)
                        selectOld = true;
                    
                    break;
                }

                oldUnit.CharactersInUnit.Remove(character);
                character.SetUnit(mainUnit);
                mainUnit.CharactersInUnit.Add(character);
            }

            if (wasSelected)
            {
                SelectionManager.Instance.DeselectAll();
                
                Unit selectUnit = selectOld ? oldUnit : mainUnit;
                SelectionManager.Instance.Select(selectUnit.CharactersInUnit[0].gameObject);
            }
            
            mainUnit.GetCurrentTile().SetOccupant(mainUnit);
            mainUnit.UpdateStats();
            mainUnit.RepositionCharacters();
            oldUnit.DestroyIfEmpty();
        }

        private readonly Vector2[][] _characterConfigurations = new Vector2[][]
        {
            // For 1 Character
            new Vector2[]
            {
                new (0, 0)
            },
            
            // For 2 Characters
            new Vector2[]
            {
                new (0.22f, 0), 
                new (-0.22f, 0)
            },
            
            // For 3 Characters
            new Vector2[]
            {
                new (-0.19f, 0.15f), 
                new (0.19f, 0.15f),
                new (0, -0.15f),
            },
            
            // For 4 Characters
            new Vector2[]
            {
                new (-0.2f, 0.2f), 
                new (0.2f, 0.2f), 
                new (-0.2f, -0.2f), 
                new (0.2f, -0.2f), 
            },
            
            // For 5 Characters
            new Vector2[]
            {
                new (-0.22f, 0.22f), 
                new (0.22f, 0.22f), 
                new (0, 0), 
                new (-0.22f, -0.22f), 
                new (0.22f, -0.22f), 
            },
            
            // For 6 characters
            new Vector2[]
            {
                new (-0.11f, 0.21f),
                new (0.11f, 0.21f),
                new (-0.24f, 0),
                new (0.24f, 0),
                new (-0.11f, -0.21f),
                new (0.11f, -0.21f),

            },
            
            // For 7 characters
            new Vector2[]
            {
                new (-0.12f, 0.23f),
                new (0.12f, 0.23f),
                new (-0.27f, 0),
                new (0, 0),
                new (0.27f, 0),
                new (-0.12f, -0.23f),
                new (0.12f, -0.23f),
            }

        };

        private void RepositionCharacters()
        {
            if (CharactersInUnit.Count == 0) return;
            
            int configIndex = CharactersInUnit.Count - 1;
            for (int i = 0; i < CharactersInUnit.Count; i++)
            {
                Character character = _charactersInUnit[i];
                if (!character) continue;
                
                Vector2 newPos = _characterConfigurations[configIndex][i];
                Vector3 position = new Vector3(newPos.x, 0, newPos.y) * MapManager.Instance.TileSize;
                AnimationManager.Instance.DoMoveToAnimation(character.gameObject, position, 0.4f, false);
            }
            SetTeamHidden(!GetCurrentTile().IsExplored);
            UpdateCharactersMoving(_charactersInUnit[0].State == CharacterState.Moving);
            UpdateStats();
        }
        
        public void SetLastTile(Vector3 newTilePos)
        {
            _lastTilePosition = newTilePos;
        }
        
        public void SetCurrentTile(Vector3 newTilePos)
        {
            SetLastTile(_currentTilePosition);
            _currentTilePosition = newTilePos;
            TileScript tile = GetCurrentTile();
            
            SetTeamHidden(!tile.IsExplored);
            ExploreTiles();
        }

        public void SetTeamHidden(bool newHidden)
        {
            foreach (Character character in _charactersInUnit)
            {
                character.SetHidden(newHidden);
            }
            
        }
        
        private void ExploreTiles()
        {
            List<TileScript> tiles = SelectionManager.Instance.GetRadius(Stats.WalkRadius, GetCurrentTile());
            
            if (TeamIndex == 0)
            {
                tiles.ForEach(tile => tile.SetExplored(true));
            }
   
            foreach (TileScript tile in tiles)
            {
                if (!tile || !tile.CurrentTile) continue;
                TeamManager.Instance.GetTeam(TeamIndex).AddSeenTile(tile);
            }
        }
        
        
        public TileScript GetCurrentTile()
        {
            return MapManager.Instance.GetTileAtPosition(_currentTilePosition);
        }
        
        public TileScript GetLastTile()
        {
            return MapManager.Instance.GetTileAtPosition(_lastTilePosition);
        }
        
        public void AddCharacter(Character character)
        {
            _charactersInUnit.Add(character);
            UpdateStats();
        }

        private void Update()
        {
            MoveAlongPath();
        }
        
        private void RotateTowardsPosition(Vector3 target)
        {

            foreach (Character character in CharactersInUnit)
            {
                Transform playerTransform = character.transform;
                Vector3 direction = (target - transform.position).normalized;
                character.transform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime * 10f);
            }
            
        }


        private void UpdateCharactersMoving(bool newMoving)
        {
            if (CharactersInUnit.Count == 0) return;
            
            CharactersInUnit.ForEach(character =>
            {
                character.SetState(newMoving ? CharacterState.Moving : CharacterState.Idle);
            });

        }

        private void ClearOccupant(TileScript tile)
        {
            if (!tile) return;
            
            // Clear the old occupant, if it is not the split unit
            if (tile.Occupant)
            {
                if (tile.Occupant != RecentSplitUnit)
                {
                    tile.ClearOccupant();
                }
            }
        }

        public void SetNewOccupant(TileScript tile)
        {
            // Set the occupant of new tile, if it is not the split unit
            if (tile.Occupant)
            {
                if (tile.Occupant != RecentSplitUnit)
                {
                    tile.Occupant.CombineWithUnit(this);
                }
            }
            else
            {
                tile.SetOccupant(this);
            }
        }

        public void SetState(CharacterState newState)
        {
            foreach (Character character in _charactersInUnit)
            {
                character.SetState(newState);
            }
        }

        private void MoveAlongPath()
        {
            if (CurrentNavigationPath.Count == 0) return;

            Vector3 currentPos = transform.position;
            Vector3 currentTarget = CurrentNavigationPath[0];

            float distance = Vector3.Distance(currentPos, currentTarget);
            float moveAmount = Stats.WalkSpeed * Time.deltaTime;
            RotateTowardsPosition(currentTarget);

            if (distance < moveAmount)
            {
                if (RecentSplitUnit)
                {
                    RecentSplitUnit.RecentSplitUnit = null;
                    RecentSplitUnit = null;
                }
                
                SetCurrentTile(CurrentNavigationPath[0]);
                CurrentNavigationPath.RemoveAt(0);
                
                if (CurrentNavigationPath.Count == 0)
                    OnArrivedTarget();

                if (CurrentPathRenderer)
                    CurrentPathRenderer.UpdatePath();

                if (TeamIndex == 0)
                    SelectionManager.Instance.UpdateCharacterSelection();
            }

            // Move towards current target
            transform.position = Vector3.MoveTowards(currentPos, currentTarget, moveAmount);

            
        }

        public string GetLabel()
        {
            return "Viking Army";
        }

        public Sprite GetIcon()
        {
            Sprite sprite = Resources.Load<Sprite>("Textures/UI/Icons/Unit_Icon");
            return sprite;
        }

        public List<ContextButtonData> GetContextButtons()
        {
            return new List<ContextButtonData>();
        }

        public void OnClicked()
        {
            AnimationManager.Instance.DoBounceAnim(gameObject);
        }

        public void OnSelected()
        {
            SetRenderPathVisibility(true);
        }
        
        public void OnDeselected()
        {
            SetRenderPathVisibility(false);
        }
        
        private void SetRenderPathVisibility(bool newVisibility)
        {
            if (CurrentPathRenderer)
            {
                CurrentPathRenderer.gameObject.transform.localScale = newVisibility ? Vector3.one : Vector3.zero;
            }
        }


    }
}
