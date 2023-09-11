using System.Collections.Generic;
using System.Linq;
using Data.GeneralTiles;
using Managers;
using Tiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using World;

namespace Characters
{
    public enum UnitState
    {
        Idle,
        Moving,
        Working,
        
    }
    
    public class Unit : MonoBehaviour
    {


        private Vector3 _currentTilePosition;
        
        public UnitState State { get; set; }
        public PathRenderer CurrentPathRenderer { get; set; }
        public List<Vector3> CurrentNavigationPath { get; private set; }  = new ();
        
        private readonly List<Character> _charactersInUnit = new();
        public List<Character> CharactersInUnit => _charactersInUnit;

        public Unit RecentSplitUnit { get; set; }
        
        private const float MovementSpeed = 1f;

        
        private void SetNewNavigationPath(List<NavigationNode> newNavigationPath)
        {
            if (newNavigationPath == null) return;
            State = UnitState.Moving;
            UpdateCharactersMoving();
            CurrentNavigationPath = newNavigationPath.Select(t => t.transform.position).ToList();
            NavigationManager.Instance.MakePathRenderer(this);
        }

        public void SplitFromUnit(Character character)
        {
            if (_charactersInUnit.Count <= 1) return;
            if (!_charactersInUnit.Contains(character)) return;
            
            CharactersInUnit.Remove(character);
            
            Unit newUnit = UnitManager.Instance.SpawnUnit(GetCurrentTile(), this);
            character.SetUnit(newUnit);
            newUnit.AddCharacter(character);
            newUnit.RepositionCharacters();
            RepositionCharacters();
            DestroyIfEmpty();
        }
 
        private void DestroyIfEmpty()
        {
            if (CharactersInUnit.Count != 0) return;
            
            if (CurrentPathRenderer)
                Destroy(CurrentPathRenderer.gameObject);
            
            Destroy(gameObject);
        }

        public void CombineWithUnit(Unit unit)
        {
            if (unit == RecentSplitUnit) return;
            
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
                character.SetUnit(this);
                CharactersInUnit.Add(character);
            }
            SelectionManager.Instance.Deselect();
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
                character.transform.SetLocalPositionAndRotation(position, Quaternion.identity);
            }
            UpdateCharactersMoving();
        }

        public int GetUnitWalkRadius()
        {
            return CharactersInUnit.Aggregate(0, (current, character) => Mathf.Max(current, character.CharacterData.WalkRadius));
        }

        public void SetCurrentTile(Vector3 newTilePos)
        {
            TileScript oldTile = GetCurrentTile();
            TileScript newTile = MapManager.Instance.GetTileAtPosition(newTilePos);

            // Clear the old occupant, if it is not the split unit
            if (oldTile.Occupant)
            {
                if (oldTile.Occupant != RecentSplitUnit)
                {
                    oldTile.ClearOccupant();
                }
            }

            // Set the occupant of new tile, if it is not the split unit
            if (newTile.Occupant)
            {
                if (newTile.Occupant != RecentSplitUnit)
                {
                    newTile.Occupant.CombineWithUnit(this);
                }
            }
            // If there is no occupant at the new tile, set this as the occupant
            else
            {
                newTile.SetOccupant(this);
            }
            
            
            _currentTilePosition = newTilePos;
            
            
   

        }
        
        public TileScript GetCurrentTile()
        {
            return MapManager.Instance.GetTileAtPosition(_currentTilePosition);
        }
        
        public void AddCharacter(Character character)
        {
            _charactersInUnit.Add(character);
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
        
        public void NavigateToTile(TileScript targetTile)
        {
            TileScript currentTile = GetCurrentTile();

            if (currentTile == targetTile) return;
            List<NavigationNode> path = NavigationManager.FindPath(currentTile, targetTile);
            SetNewNavigationPath(path);
        }

        private void UpdateCharactersMoving()
        {
            _charactersInUnit.ForEach(character => character.SetIsMoving(State == UnitState.Moving));
        }

        private void OnArrivedTarget()
        {
            State = UnitState.Idle;
            UpdateCharactersMoving();
        }

        private void MoveAlongPath()
        {
            if (CurrentNavigationPath.Count == 0) return;

            Vector3 currentPos = transform.position;
            Vector3 currentTarget = CurrentNavigationPath[0];

            float distance = Vector3.Distance(currentPos, currentTarget);

            // Move towards current target
            transform.position = Vector3.MoveTowards(currentPos, currentTarget, MovementSpeed * Time.deltaTime);
            RotateTowardsPosition(currentTarget);

            // We are close enough to the point to go to next point.
            if (distance < 0.1f)
            {
                SetCurrentTile(CurrentNavigationPath[0]);
                CurrentNavigationPath.RemoveAt(0);
                
                if (RecentSplitUnit)
                {
                    RecentSplitUnit.RecentSplitUnit = null;
                    RecentSplitUnit = null;
                }
                
                if (CurrentNavigationPath.Count == 0)
                {
                    OnArrivedTarget();
                };
                
                CurrentPathRenderer.UpdatePath();
                SelectionManager.Instance.UpdateCharacterSelection();
            }
        }
    }
}
