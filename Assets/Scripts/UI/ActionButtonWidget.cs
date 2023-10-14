using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    
    public struct RequirementData
    {
        public readonly Sprite icon;
        public readonly int number;
        public readonly Func<bool> requirementFunction;

        public RequirementData(Sprite icon, int number, Func<bool> requirementFunction)
        {
            this.icon = icon;
            this.number = number;
            this.requirementFunction = requirementFunction;
        }
    }
    
    public class ActionButtonWidget : MonoBehaviour
    {

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _icon;
        [SerializeField] private UnityAction _buttonAction;
        [SerializeField] private RequirementWidget _requirementWidgetPrefab;
        [SerializeField] private VerticalLayoutGroup _requirements;
        
        private readonly List<RequirementWidget> _requirementWidgets = new ();

        public void InitButton(Sprite icon, string label, List<RequirementData> requirements, UnityAction buttonAction)
        {
            _icon.sprite = icon;
            _label.text = label;
            _buttonAction = buttonAction;

            ClearRequirements();
            
            foreach (RequirementData requirement in requirements)
            {
                RequirementWidget requirementWidget = Instantiate(_requirementWidgetPrefab, _requirements.transform);
                requirementWidget.InitRequirement(requirement.icon, requirement.number, requirement.requirementFunction);
                requirementWidget.gameObject.SetActive(requirement.icon);
                _requirementWidgets.Add(requirementWidget);
            }
        }
        
        private void ClearRequirements()
        {
            foreach (Transform widget in _requirements.transform)
            {
                Destroy(widget.gameObject);
            }
            _requirementWidgets.Clear();
        }
    
        private void Start()
        {
            _button.interactable = MeetsAllRequirements();
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            AnimationManager.Instance.DoBounceAnim(gameObject);

            if (MeetsAllRequirements())
            {
                _buttonAction?.Invoke();
            }
        }

        private void Update()
        {
            _button.interactable = MeetsAllRequirements();
        }

        private bool MeetsAllRequirements()
        {
            return _requirementWidgets.All(requirement => requirement.DoesMeetRequirement());
        }
    }
}
