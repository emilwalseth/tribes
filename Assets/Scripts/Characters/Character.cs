using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Managers;
using Tiles;
using UnityEngine;
using UnityEngine.Events;
using World;

namespace Characters
{
    public class Character : MonoBehaviour, IInteractable
    {
        
        
        [SerializeField] private CharacterData _characterData;
        [SerializeField] private CharacterTool _tool;
        
        
        private Animator _animator;

        public Unit CurrentUnit { get; set; }
        public CharacterData CharacterData => _characterData;
        
        
        // Animation values
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int Hit = Animator.StringToHash("Hit");


        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetIsMoving(bool newIsMoving)
        {
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
            AnimationManager.Instance.DoBounceAnim(gameObject, 0.25f);
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

        public void SetTool(ToolType toolType)
        {
            _tool.SetTool(toolType);
        }
        
        public void PlayHitAnim(ToolType toolType)
        {
            _tool.SetTool(toolType);
            _animator.Play(Hit, 0,0f);
        }


    }
}
