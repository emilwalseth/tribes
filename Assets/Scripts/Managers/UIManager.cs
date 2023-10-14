using System.Collections.Generic;
using Characters;
using Data.Buildings;
using Data.Resources;
using Interfaces;
using Player;
using Tiles;
using UI;
using UnityEngine;
using ContextMenu = UI.ContextMenu;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
    
        public static UIManager instance;
        private void Awake() => instance = this;
        
        [SerializeField] private ContextMenu _contextMenu;
        
        
        public void OpenContextMenu(GameObject contextObject)
        {
            _contextMenu.Open(contextObject);
        }

        public void CloseContextButtons()
        {
            _contextMenu.CloseButtons();
        }
        public void CloseContextMenu()
        {
            _contextMenu.Close();
        }

        public ContextButtonData MakeHarvestButton(Character character, ResourceData resourceData)
        {

            List<RequirementData> requirements = new()
            {
                new RequirementData(
                    null,
                    0,
                    () => character.State != CharacterState.Harvesting
                )
            };

            ContextButtonData harvestButton = new ContextButtonData(
                resourceData.ResourceIcon,
                "Harvest",
                requirements,
                () =>
                {
                    InteractionManager.Instance.StartHarvesting(character);
                }
            );
            
            return harvestButton;
        }
        
        public ContextButtonData MakeBuildButton(TileScript tile, int teamIndex, BuildingTileData buildingData)
        {

            List<RequirementData> requirements = new();
            List<ResourceAmount> requirementCost = buildingData.GetResourceCostList();
            
            foreach (ResourceAmount requirement in requirementCost)
            {
                requirements.Add(MakeHarvestRequirement(requirement.ResourceData, requirement.Amount));
            }

            ContextButtonData harvestButton = new (
                buildingData.BuildingData.TileData.TileIcon,
                "Build " + buildingData.BuildingData.TileData.TileName,
                requirements,
                () =>
                {
                    TeamState team = TeamManager.Instance.GetTeam(teamIndex);
                    
                    team.RemoveResources(buildingData.GetResourceCostList());
                    TileManager.Instance.PlaceBuilding(teamIndex, tile, buildingData);
                    SelectionManager.Instance.DeselectAll();
                }
            );
            
            return harvestButton;
        }

        public RequirementData MakeHarvestRequirement(ResourceData resourceData, int requiredAmount)
        {
            return new RequirementData(
                resourceData.ResourceIcon,
                requiredAmount,
                () =>
                {
                    int teamIndex = TeamManager.Instance.GetMyTeamIndex();
                    TeamState team = TeamManager.Instance.GetTeam(teamIndex);
                    return team.HasResource(resourceData.ResourceType, requiredAmount);
                });
        }
    }
}
