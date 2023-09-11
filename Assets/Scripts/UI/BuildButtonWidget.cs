using System;
using System.Collections.Generic;
using System.Linq;
using Data.Buildings;
using Data.Resources;
using Managers;
using Tiles;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BuildButtonWidget : MonoBehaviour
    {
    
        [SerializeField] private Button _buildButton;
        [SerializeField] private BuildingTileData _buildingData;
        [SerializeField] private RequirementWidget _requirementWidgetPrefab;
        [SerializeField] private VerticalLayoutGroup _requirementGroup;
        
        
        private readonly List<RequirementWidget> _requirementWidgets = new List<RequirementWidget>();
        
        
        private void UpdateEnabled()
        {
            if (!_buildingData) return;
            List<ResourceKeyValuePair> requirements = _buildingData.BuildingLevels[0].BuildRequirements.Resources;

            bool canBuild = true;
            
            foreach (ResourceKeyValuePair requirement in requirements)
            {
                if (StatsManager.Instance.HasResource(requirement.Resource, requirement.Amount)) continue;
                
                canBuild = false;
                break;
            }
            
            _buildButton.interactable = canBuild;
            
        }
        
        private void UpdateRequirements()
        {
            if (!_buildingData || _buildingData.BuildingLevels.Count == 0) return;

            Requirements requirements = _buildingData.BuildingLevels[0].BuildRequirements;

            ClearRequirementWidgets();

            foreach (ResourceKeyValuePair resource in requirements.Resources)
            {
                RequirementWidget requirementWidget = Instantiate(_requirementWidgetPrefab, _requirementGroup.transform, true);
                requirementWidget.transform.localScale = Vector3.one;
                requirementWidget.InitializeWidget(resource.Resource, resource.Amount);
                _requirementWidgets.Add(requirementWidget);
            }
            
        }

        private void ClearRequirementWidgets()
        {
            foreach (RequirementWidget requirement in _requirementWidgets.Where(requirement => requirement))
            {
                Destroy(requirement.gameObject);
            }
            _requirementWidgets.Clear();
        }
        
        
        private void OnEnable()
        {
            UpdateRequirements();
            StatsManager.Instance.onStatsChanged += UpdateEnabled;
            _buildButton.onClick.AddListener(Build);
            UpdateEnabled();
        }
        
        private void OnDisable()
        {
            StatsManager.Instance.onStatsChanged -= UpdateEnabled;
            _buildButton.onClick.RemoveAllListeners();
        }
    
        private void Build()
        {
            TileScript tile = SelectionManager.Instance.GetSelectedTile();
            if (!tile) return;
            TileManager.Instance.PlaceBuilding(tile, _buildingData);
            RemoveResources();
            UIManager.instance.CloseMenu();
        }

        private void RemoveResources()
        {
            if (!_buildingData) return;
            
            Requirements requirements = _buildingData.BuildingLevels[0].BuildRequirements;

            foreach (ResourceKeyValuePair requirement in requirements.Resources)
            {
                StatsManager.Instance.RemoveResource(requirement.Resource, requirement.Amount);
            }
        }
    }
}
