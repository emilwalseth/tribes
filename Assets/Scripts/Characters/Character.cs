using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Managers;
using Tiles;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using World;

namespace Characters
{
    
    public enum CharacterState
    {
        Idle,
        Moving,
        Harvesting,
        Attacking,
    }
    
    public class Character : MonoBehaviour, IInteractable, IContextInterface
    {
        
        
        [SerializeField] private CharacterData _characterData;
        [SerializeField] private CharacterTool _tool;
        [SerializeField] private MeshFilter _characterMeshFilter;
        [SerializeField] private MeshRenderer _characterMeshRenderer;
        
        [SerializeField] private List<RectTransform> _enemyMenuOptions = new();
        [SerializeField] private List<RectTransform> _teamMenuOptions = new();

        
        
        private Animator _animator;
        public Unit CurrentUnit { get; set; }
        public CharacterState State { get; private set; } = CharacterState.Idle;
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
        private float _currentHealth;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _currentHealth = _characterData.Health;
        }

        public void SetHidden(bool newHidden)
        {
            if (_characterMeshRenderer.enabled == !newHidden) return;
            
            _characterMeshRenderer.enabled = !newHidden;
            _tool.SetHidden(newHidden);
        }

        public bool GetHidden()
        {
            return !_characterMeshRenderer.enabled;
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
                StopAttacking();
            }
        }

        private void StopAttacking()
        {
            CancelInvoke(nameof(TryAttacking));
            _targetUnit = null;
            SetTool(ToolType.None);
            State = CharacterState.Idle;
        }
        
        private void TryAttacking()
        {
            if (!_targetUnit)
                StopAttacking();
            
            State = CharacterState.Attacking;
            
            float radius = (_characterData.AttackRadius + 0.05f) * MapManager.Instance.TileSize;
            
            Vector3 targetPosition = _targetUnit.transform.position;
            Vector3 currentPosition = CurrentUnit.transform.position;
            
            if (Vector3.Distance(targetPosition, currentPosition) > radius) return;
            
            if (_timeLastAttack + _characterData.AttackSpeed >= Time.realtimeSinceStartup) return;
            _timeLastAttack = Time.realtimeSinceStartup;
            
            // TODO: Choose character
            Character targetCharacter = _targetUnit.CharactersInUnit[0];
            
            Quaternion targetRot = Quaternion.LookRotation(targetPosition - currentPosition);
            AnimationManager.Instance.DoRotateToAnimation(gameObject, targetRot, 0.5f, true);
            SetTool(ToolType.Sword);
            PlayAttackAnim();
            targetCharacter.Damage(CurrentUnit, _characterData.Damage);
            
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
        

        public void SetState(CharacterState newState)
        {

            State = newState;
            SetIsMoving(newState == CharacterState.Moving);
            
            switch (newState)
            {
                case CharacterState.Idle:
                    break;
                case CharacterState.Moving:
                    break;
                case CharacterState.Harvesting:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
            
        }

        public void OnClicked()
        {
            SelectionManager.Instance.Select(gameObject);
            if (SelectionManager.Instance.IsSelected(CurrentUnit.gameObject))
            {
                CurrentUnit.OnClicked();
            }
            else
            {
                AnimationManager.Instance.DoBounceAnim(gameObject);
            }
            
        }
        
        public TileScript GetCurrentTile()
        {
            return CurrentUnit ? CurrentUnit.GetCurrentTile() : null;
        }
        public void OnSelected()
        {
            CurrentUnit.OnSelected();
        }
        
        public void SetUnit(Unit unit)
        {
            CurrentUnit = unit;
            transform.SetParent(unit.transform);
        }

        public void OnDeselected()
        {
            CurrentUnit.OnDeselected();
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
            _animator.Play(Attack, 1,0f);
            _animator.Play(Attack, 2,0f);
        }

        public void Damage(Unit fromUnit,  float amount)
        {
            Sprite sprite = Resources.Load<Sprite>("Textures/AlertMarker");
            print(sprite);

            _currentHealth -= amount;
            if (_currentHealth <= 0)
            {
                // Split to get this character on its own
                GameManager.Instance.SpawnGravestone(transform.position);
                CurrentUnit.SplitFromUnit(this);
                TeamManager.Instance.GetTeam(CurrentUnit.TeamIndex).RemoveUnit(CurrentUnit);
                
                // Then destroy the unit
                Destroy(CurrentUnit.gameObject);
                
                
            }

            if (!GetHidden())
                InteractionManager.Instance.SpawnIndicator(transform.position + new Vector3(0,0,0), sprite);

            PlayHitAnim();
            
            // Attack back
            SetAttackTarget(fromUnit);
            
        }
        
        public void PlayHitAnim()
        {
            _animator.Play(Hit, 1,0f);
        }

        public string GetLabel()
        {
            return _characterData.CharacterName;
        }

        public Sprite GetIcon()
        {
            return _characterData.CharacterIcon;

        }

        public List<ContextButtonData> GetContextButtons()
        {
            if (CurrentUnit.TeamIndex != TeamManager.Instance.GetMyTeamIndex())
                return new List<ContextButtonData>();
            
            
            List<ContextButtonData> contextButtons = new();

            TileScript tile = GetCurrentTile();

            // If the tile we are on is a resource tile, show harvest options
            ResourceTileScript resourceTile = tile.GetResource();
            if (resourceTile)
            {
                if (tile.Occupant && tile.Occupant == CurrentUnit)
                {
                    contextButtons.Add(UIManager.instance.MakeHarvestButton(this, resourceTile.Resources[0].Resource));
                }
            }

            return contextButtons;
        }
    }
}
