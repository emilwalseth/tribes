using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{

    public struct ContextButtonData
    {
        
        public Sprite icon;
        public string label;
        public List<RequirementData> requirements;
        public UnityAction buttonAction;
        
        public ContextButtonData(Sprite icon, string label, List<RequirementData> requirements, UnityAction buttonAction)
        {
            this.icon = icon;
            this.label = label;
            this.requirements = requirements;
            this.buttonAction = buttonAction;
        }
        
    }
    
    
    [RequireComponent(typeof(RectTransform))]
    public class ContextMenu : MonoBehaviour
    {

        [SerializeField] private RectTransform _contextButtons;
        [SerializeField] private ContextTagWidget _contextTag;
        [SerializeField] private RectTransform _buttonBox;
        [SerializeField] private ActionButtonWidget _actionButtonPrefab;



        private GameObject _currentContextObject;
        private Animator _animator;
        private bool _tagOpen;
        private bool _buttonsOpen;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            Close();
            PlayButtonAnim(true);
            PlayTagAnim(true);
        }

        private void AddButton(ContextButtonData buttonData)
        {
            ActionButtonWidget actionButton = Instantiate(_actionButtonPrefab, _buttonBox.transform);
            actionButton.InitButton(buttonData.icon, buttonData.label, buttonData.requirements, buttonData.buttonAction);
        }
        
        private void ClearButtonOptions()
        {
            foreach (RectTransform option in _buttonBox.transform)
            {
                Destroy(option.gameObject);
            }
        }

        public void Open(GameObject contextObject)
        {
            if (!contextObject.TryGetComponent(out IContextInterface contextInterface))
            {
                Close();
                return;
            }
            
            
            string label = contextInterface.GetLabel();
            Sprite icon = contextInterface.GetIcon();
            List<ContextButtonData> contextButtons = contextInterface.GetContextButtons();
            

            // First handle the tag
            if (_currentContextObject != contextObject)
            {
                _contextTag.InitWidget(icon, label);
                OpenTag();
            }
            _currentContextObject = contextObject;
            
            // Then handle the buttons
            if (contextButtons.Count == 0)
            {
                CloseButtons();
                return;
            }
            
            // We have context buttons, so add them and open.
            ClearButtonOptions();
            
            foreach (ContextButtonData buttonData in contextButtons)
            {
                AddButton(buttonData);
            }
            
            OpenButtons();
  
        }

        private void OpenTag()
        {
            if (!_tagOpen)
                PlayTagAnim(false);
            else
                PlayTagBounce();
            
            
            _tagOpen = true;
        }
        
        private void OpenButtons()
        {
            if (!_buttonsOpen)
                PlayButtonAnim(false);
            
            _buttonsOpen = true;
        }

        private void CloseTag()
        {
            if (_tagOpen)
            {
                PlayTagAnim(true);
            }
            _tagOpen = false;
        }
        
        public void CloseButtons()
        {
            if (_buttonsOpen)
            {
                PlayButtonAnim(true);
            }
            _buttonsOpen = false;
        }

        private void PlayButtonAnim(bool reverse)
        {
            _animator.Play( reverse ? "ContextButtons_Anim_Reversed" : "ContextButtons_Anim", 0,0);
        }
        
        private void PlayTagBounce()
        {
            _animator.Play("ContextTagBounce_Anim", 1,0);
        }
        
        private void PlayTagAnim(bool reverse)
        {
            _animator.Play( reverse ? "ContextTag_Anim_Reversed" : "ContextTag_Anim", 1,0);
        }

        public void Close()
        {
            CloseTag();
            CloseButtons();
            _currentContextObject = null;
        }
    }
}
