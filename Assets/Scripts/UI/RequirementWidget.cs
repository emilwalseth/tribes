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
        [SerializeField] private TMP_Text _amountText;

        [SerializeField] private ResourceData _resourceData;
        [SerializeField] private int _requiredAmount;
    

        private void OnEnable()
        {
            TeamManager.Instance.GetTeam(0).onStatsChanged += SetRequirement;
        }
    
        private void OnDisable()
        {
            TeamManager.Instance.GetTeam(0).onStatsChanged -= SetRequirement;
        }

        public void InitializeWidget(ResourceData resourceData, int requiredAmount)
        {
            _resourceData = resourceData;
            _requiredAmount = requiredAmount;
            SetRequirement();
        }
    
        private void SetRequirement()
        {
            _icon.sprite = _resourceData.ResourceIcon;
            _amountText.text = _requiredAmount.ToString();

            bool isEnough = TeamManager.Instance.GetTeam(0).HasResource(_resourceData.ResourceType, _requiredAmount);
            
            _amountText.color = isEnough ? Color.white : Color.red;
        }
    
    
    }
}
