using System;
using System.Collections.Generic;
using Data.Resources;
using Managers;
using Tiles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RequirementWidget : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _numberText;
        
        private bool _requirementMet = false;


        private Func<bool> _requirementFunction;
        
        public void InitRequirement(Sprite icon, int number, Func<bool> requirementFunction)
        {
            _icon.sprite = icon;
            _numberText.text = number.ToString();
            _requirementFunction = requirementFunction;
        }

        private void Start()
        {
            CheckRequirement();
        }

        private void Update()
        {
            CheckRequirement();
        }

        private void CheckRequirement()
        {
            bool newRequirementMet = DoesMeetRequirement();
            _numberText.color = newRequirementMet ? Color.white : Color.red;
            if (_requirementMet != newRequirementMet)
            {
                _requirementMet = newRequirementMet;
                AnimationManager.Instance.DoBounceAnim(_icon.gameObject);
            }
        }

        public bool DoesMeetRequirement()
        {
            return _requirementFunction == null || _requirementFunction.Invoke();
        }
    }
}
