using System.Collections.Generic;
using System.Linq;
using Characters;
using Data.Buildings;
using Data.Resources;
using Managers;
using Tiles;
using UnityEngine;

namespace AI
{
    public class AIController : MonoBehaviour
    {

        [SerializeField] private float _updateRate = 2f;
        [SerializeField] private int _teamIndex;
        
        private void Start()
        {
            StartAI();
        }
        
        public void SetTeamIndex(int teamIndex)
        {
            _teamIndex = teamIndex;
        }

        private void StartAI()
        {
            InvokeRepeating(nameof(Decision), 2f, _updateRate);
        }
        
        private void StopAI()
        {
            CancelInvoke(nameof(Decision));
        }
        
        private void Decision()
        {
            if (!TeamManager.Instance.IsValidTeam(_teamIndex)) return;
            
            Build();
        }


        private void Harvest(ResourceType resourceType)
        {

            Character character = GetAvailableCharacter();
            if (!character) return;

            TileScript currentTile = character.GetCurrentTile();

            // If we are already at a resource, harvest this
            if (currentTile.CurrentTile && currentTile.CurrentTile.TryGetComponent(out ResourceTileScript resourceTileScript))
            {
                if (resourceTileScript.Resources[0].Resource.ResourceType == resourceType)
                {
                    StartHarvesting(character, character.GetCurrentTile(), resourceType);
                    return;
                }
            }
            
            TileScript resourceTile = GetClosestResource(character, resourceType);
            
            if (!resourceTile)
            {
                Explore();
                return;
            }
            
            
            if (MoveTo(character, resourceTile))
            {
                character.CurrentUnit.onReachedDestination += () =>
                {
                    StartHarvesting(character, resourceTile, resourceType);
                };
            }
        }

        private void StartHarvesting(Character character, TileScript tile, ResourceType resourceType)
        {
            if (character.GetCurrentTile() == tile && tile.CurrentTile && tile.CurrentTile.TryGetComponent(out ResourceTileScript resourceTileScript))
            {
                InteractionManager.Instance.StartHarvesting(character);
            }
            else
            {
                Harvest(resourceType);
            }
        }

        private bool MoveTo(Character character, TileScript tile)
        {
            character.CurrentUnit.SplitFromUnit(character);
            return character.CurrentUnit.NavigateToTile(tile);
        }

        private void Build()
        {
            BuildingTileData house = TileManager.Instance.GetTownData().House;
            int cost = house.BuildingData.BuildRequirements.Resources[0].Amount;
            bool hasEnough = TeamManager.Instance.GetTeam(_teamIndex).HasResource(ResourceType.Wood, cost);

            if (!hasEnough)
                Harvest(ResourceType.Wood);
            else
            {
                TileScript tile = GetAvailableTownTile();
                if (!tile)
                {
                    Harvest(ResourceType.Stone);
                    return;
                }
                TeamManager.Instance.GetTeam(_teamIndex).RemoveResource(ResourceType.Wood, cost);
                TileManager.Instance.PlaceBuilding(_teamIndex, tile, TileManager.Instance.GetTownData().House);
            }
        }

        private TileScript GetAvailableTownTile()
        {
            List<TileScript> towns = TeamManager.Instance.GetTeam(_teamIndex).Towns;
            if (towns.Count == 0) return null;


            foreach (TileScript town in towns)
            {
                List<TileScript> claimed = SelectionManager.Instance.GetRadius(town.TileData.SelectionRadius, town);

                foreach (TileScript tile in claimed)
                {
                    if (!tile.CurrentTile)
                        return tile;

                    if (!tile.CurrentTile.TryGetComponent(out BuildingTileScript buildingTile))
                        return tile;
                }
                
            }

            return null;
        }
        

        private void Explore()
        {
            Character character = GetAvailableCharacter();
            if (!character) return;

            List<TileScript> tiles =
                SelectionManager.Instance.GetRadius(character.CharacterData.WalkRadius, character.CurrentUnit.GetCurrentTile());
            MoveTo(character, tiles[Random.Range(1, tiles.Count)]);
        }

        private Character GetAvailableCharacter()
        {
            List<Unit> units = TeamManager.Instance.GetTeam(_teamIndex).Units;
            foreach (Unit unit in units)
            {
                foreach (Character character in unit.CharactersInUnit)
                {
                    if (character.State == CharacterState.Idle)
                    {
                        return character;
                    }
                }
       
            }
            return null;
        }
        private TileScript GetClosestResource(Character character, ResourceType resourceType)
        {
            List<ResourceTileScript> seenResources = TeamManager.Instance.GetTeam(_teamIndex).SeenResourceTiles;
            if (seenResources.Count == 0) return null;

            List<TileScript> unitRadius = SelectionManager.Instance.GetRadius(character.CharacterData.WalkRadius, character.GetCurrentTile());
            
            float closestDistance = 1000;
            TileScript closest = null;
            foreach (ResourceTileScript resource in seenResources.Where(tile => tile && unitRadius.Contains(tile.GetOwningTile())))
            {
                if (resource.Resources[0].Resource.ResourceType == resourceType)
                {
                    if (!resource)
                    {
                        TeamManager.Instance.GetTeam(_teamIndex).SeenResourceTiles.Remove(resource);
                        continue;
                    }
                    
                    float distance = Vector3.Distance(resource.gameObject.transform.position, character.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = resource.GetOwningTile();
                    }
                }
            }
            
            return closest;
        }
    }
}
