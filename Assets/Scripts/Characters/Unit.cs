using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Data.GeneralTiles;
using Managers;
using Tiles;
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
    
    public class Unit : MonoBehaviour
    {
    
        [SerializeField] private CharacterMarker _unitMarker;

        private Vector3 _currentTilePosition;
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
            
            if ((nextPosition - targetPosition).magnitude < 0.1f)
            {
                TileScript targetTile = GetCurrentTile();
                SetCurrentTile(targetPosition);
                NavigateToTile(targetTile);
            }
            else
            {
                CurrentNavigationPath.Remove(targetPosition);
            }
            
            NavigationManager.Instance.MakePathRenderer(this);
        }
        
        private bool SetNewNavigationPath(List<NavigationNode> newNavigationPath)
        {
            if (newNavigationPath == null) return false;

            List<Vector3> newPath = new List<Vector3>();

            List<TileScript> radius = SelectionManager.Instance.GetRadius(Stats.WalkRadius, GetCurrentTile());
            
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
                NavigationManager.Instance.MakePathRenderer(this);
            return true;
        }
        
        
        private void OnArrivedTarget()
        {
            print("Arrived!");
            GetCurrentTile().onOccupantEntered -= OnOccupantUpdated;
            UpdateCharactersMoving(false);
            SetNewOccupant(GetCurrentTile());
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
            
            bool wasSelected = SelectionManager.Instance.SelectedUnit == unit || SelectionManager.Instance.SelectedUnit == this;
            
            foreach (Character character in unit.CharactersInUnit)
            {
                if (CharactersInUnit.Count >= 7)
                {
                    List<TileScript> neighbors = MapManager.Instance.GetTileNeighbors(GetCurrentTile().gameObject)
                        .Where(tile => tile.TileData.IsWalkable).ToList();
                    Vector3 fallbackTilePos = neighbors[Random.Range(0, neighbors.Count)].transform.position;
                    unit.transform.position = fallbackTilePos;
                    unit.SetCurrentTile(fallbackTilePos);
                    break;
                }

                if (SelectionManager.Instance.SelectedCharacter == character)
                    wasSelected = true;
                
                character.SetUnit(this);
                CharactersInUnit.Add(character);
            }
            
            SelectionManager.Instance.DeselectAll();
            
            if (wasSelected)
                SelectionManager.Instance.SelectUnit(this);
            
            unit.CharactersInUnit.Clear();
            unit.DestroyIfEmpty();
            RepositionCharacters();
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
            UpdateCharactersMoving(_charactersInUnit[0].State == CharacterState.Moving);
            UpdateStats();
        }
        public void SetCurrentTile(Vector3 newTilePos)
        {
            _currentTilePosition = newTilePos;
            TileScript tile = GetCurrentTile();
            
            gameObject.SetActive(tile.IsExplored);
            ExploreTiles();
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
                

                SelectionManager.Instance.UpdateCharacterSelection();
            }

            // Move towards current target
            transform.position = Vector3.MoveTowards(currentPos, currentTarget, moveAmount);

            
        }
    }
}
