using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Managers;
using Tiles;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using World;

namespace Characters
{
    public class Character : MonoBehaviour, IInteractable
    {
        
        
        [SerializeField] private CharacterData _characterData;
        [SerializeField] private CharacterTool _tool;
        [SerializeField] private MeshFilter _characterMeshFilter;
        
        [SerializeField] private List<RectTransform> _enemyMenuOptions = new();
        [SerializeField] private List<RectTransform> _teamMenuOptions = new();

        
        
        private Animator _animator;
        public Unit CurrentUnit { get; set; }
        public CharacterData CharacterData => _characterData;
        public List<RectTransform> EnemyMenuOptions => _enemyMenuOptions;
        public List<RectTransform> TeamMenuOptions => _teamMenuOptions;
        
        // Animation values
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Attack = Animator.StringToHash("Attack");
        
        private float _timeLastAttack;
        private Unit _targetUnit;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        public void SetAttackTarget(Unit targetUnit)
        {
            
            if (targetUnit)
            {
                InvokeRepeating(nameof(TryAttacking), 0, 0.1f);
                _targetUnit = targetUnit;
            }
            else
            {
                CancelInvoke(nameof(TryAttacking));
                _targetUnit = null;
                SetTool(ToolType.None);
            }
        }
        
        private void TryAttacking()
        {
            if (!_targetUnit) return;

            float radius = (_characterData.AttackRadius + 0.05f) * MapManager.Instance.TileSize;
            
            if (Vector3.Distance(_targetUnit.transform.position, transform.position) > radius) return;
            
            if (_timeLastAttack + _characterData.AttackSpeed >= Time.realtimeSinceStartup) return;
            _timeLastAttack = Time.realtimeSinceStartup;
            
            // TODO: Choose character
            Character targetCharacter = _targetUnit.CharactersInUnit[0];
            
            PlayAttackAnim();
            targetCharacter.Damage(0);
            
        }

        public void SetIsMoving(bool newIsMoving)
        {
            _animator.Update(0);
            
            if (newIsMoving)
            {
                _animator.CrossFade(Walking, 0.1f, 0);
   
            }
            else
            {
                _animator.CrossFade(Idle, 0.1f, 0);

            }
            
        }

        public void OnClicked()
        {
            SelectionManager.Instance.SelectCharacter(this);
            AnimationManager.Instance.DoBounceAnim(gameObject);
        }
        
        public TileScript GetCurrenTile()
        {
            return CurrentUnit ? CurrentUnit.GetCurrentTile() : null;
        }

        public UnitState GetState()
        {
            return CurrentUnit ? CurrentUnit.State : UnitState.Idle;
        }

        public void SetState(UnitState state)
        {
            if (!CurrentUnit) return;
            CurrentUnit.State = state;
        }
        
        public void OnSelected()
        {
            SetRenderPathVisibility(true);
        }
        
        public void SetUnit(Unit unit)
        {
            CurrentUnit = unit;
            transform.SetParent(unit.transform);
        }

        public void OnDeselected()
        {
            SetRenderPathVisibility(false);
        }

        private void SetRenderPathVisibility(bool newVisibility)
        {
            if (CurrentUnit && CurrentUnit.CurrentPathRenderer)
            {
                CurrentUnit.CurrentPathRenderer.gameObject.transform.localScale = newVisibility ? Vector3.one : Vector3.zero;
            }
        }
        
        public void SetCharacterData(CharacterData characterData)
        {
            _characterData = characterData;
            UpdateCharacter();
        }
        
        private void UpdateCharacter()
        {
            SetCharacterMesh();
        }

        private void SetCharacterMesh()
        {
            _characterMeshFilter.mesh = _characterData.CharacterMesh;
            Bounds bounds = _characterMeshFilter.mesh.bounds;
            GetComponent<BoxCollider>().size = bounds.size;
            GetComponent<BoxCollider>().center = bounds.center;

        }

        public void SetTool(ToolType toolType)
        {
            _tool.SetTool(toolType);
        }
        public void PlayAttackAnim()
        { 
            SetTool(ToolType.Sword);
            _animator.Play(Attack, 1,0f);
            _animator.Play(Attack, 2,0f);
        }

        public void Damage(int amount)
        {
            Sprite sprite = Resources.Load<Sprite>("Textures/AlertMarker");
            print(sprite);
            InteractionManager.Instance.SpawnIndicator(transform.position + new Vector3(0,0,0), sprite);
            PlayHitAnim();
        }
        
        public void PlayHitAnim()
        {
            _animator.Play(Hit, 1,0f);
        }


    }
}
