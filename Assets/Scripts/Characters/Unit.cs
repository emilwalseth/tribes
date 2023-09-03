using System.Collections.Generic;
using System.Linq;
using Managers;
using Tiles;
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

        private const float MovementSpeed = 1f;

        
        private void SetNewNavigationPath(List<NavigationNode> newNavigationPath)
        {
            if (newNavigationPath == null) return;
            State = UnitState.Moving;
            SetCharactersIsMoving(true);
            CurrentNavigationPath = newNavigationPath.Select(t => t.transform.position).ToList();
            NavigationManager.Instance.MakePathRenderer(this);
        }

        public void SetCurrentTile(Vector3 newTilePos)
        {
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
            Transform playerTransform = transform;
            Vector3 direction = (target - playerTransform.position).normalized;
            transform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime * 10f);
        }
        
        public void NavigateToTile(TileScript targetTile)
        {
            TileScript currentTile = GetCurrentTile();

            if (currentTile == targetTile) return;
            List<NavigationNode> path = NavigationManager.FindPath(currentTile, targetTile);
            SetNewNavigationPath(path);
        }

        private void SetCharactersIsMoving(bool newMoving)
        {
            _charactersInUnit.ForEach(character => character.SetIsMoving(newMoving));
        }

        private void OnArrivedTarget()
        {
            State = UnitState.Idle;
            SetCharactersIsMoving(false);
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
