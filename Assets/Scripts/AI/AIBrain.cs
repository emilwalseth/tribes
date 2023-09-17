using System;
using System.Collections;
using Characters;
using Managers;
using UnityEngine;

namespace AI
{
    public enum AIState
    {
        None,
        Attack,
    }
 
    [RequireComponent(typeof(Unit))]
    public class AIBrain : MonoBehaviour
    {

        [SerializeField] private float _updateRate = 0.5f;
        
        
        private AIState _state = AIState.None;
        private Unit _unit;
        private GameObject _target;

        public void SetTarget(GameObject newTarget)
        {
            _target = newTarget;
        }
        
        public void SetState(AIState newState)
        {
            _state = newState;
            CancelInvoke(nameof(DoState));
            
            if (newState == AIState.None)
            {
                if (!_target) return;
                _target.TryGetComponent(out Unit targetUnit);
                if (targetUnit)
                {
                    targetUnit.SetMarker(MarkerType.None);
                    _unit.SetAttackTarget(null);
                }
                SetTarget(null);
                return;
            }
            InvokeRepeating(nameof(DoState), 0, _updateRate);
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
        }
        
        private void DoState()
        {
            switch (_state)
            {
                case AIState.Attack:
                    DoAttackState();
                    break;
                case AIState.None:
                    break;
                default:
                    break;
            }
        }
            
        private void DoAttackState()
        {
            if (_target == null) return;
            
            float distance = Vector3.Distance(transform.position, _target.transform.position) / MapManager.Instance.TileSize;

            // Walk towards target
            if (!_target.TryGetComponent(out Unit targetUnit)) return;
            
            if (distance <= _unit.Stats.WalkRadius)
            {
                // Navigate towards enemy
                _unit.NavigateToTile(targetUnit.GetCurrentTile());
                _unit.SetAttackTarget(targetUnit);
                
            }
            else
            {
                _state = AIState.None;
            }
            
        }
        
    }
}
