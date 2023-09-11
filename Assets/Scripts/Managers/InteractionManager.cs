using System.Collections;
using Characters;
using Data.Resources;
using Interfaces;
using Tiles;
using UI;
using UnityEngine;

namespace Managers
{
    public class InteractionManager : MonoBehaviour
    {
        public static InteractionManager Instance { get; private set; }
        private void Awake() => Instance = this;


        [SerializeField] private Indicator _indicatorPrefab;


        public void SpawnIndicator(Vector3 position, ResourceData resource)
        {
            Vector3 indicatorPos = position + new Vector3(0, 1.5f, 0);
            Indicator indicator = Instantiate(_indicatorPrefab, indicatorPos, Quaternion.identity);
            indicator.SetImage(resource.ResourceIcon);
        }
        
        public void StartChoppingWood()
        {
            Character character = SelectionManager.Instance.SelectedCharacter;
            float harvestTime = character.CharacterData.WoodChopSpeed;
            StartHarvesting(character, harvestTime, 1f);
        }
        
        private void StartHarvesting(Character character, float harvestTime, float harvestEfficiency)
        {
            if (!character) return;
            character.SetState(UnitState.Working);
            UIManager.instance.CloseMenu();
            StartCoroutine(Harvesting(character, harvestTime, harvestEfficiency));
        }
        
        private static IEnumerator Harvesting(Character character, float harvestTime, float harvestEfficiency)
        {
            
            TileScript tile = character.GetCurrenTile();
            while (true)
            {
                // If the character is no longer working or the currentTile is no longer the one we are in, quit chopping
                if (character.GetState() != UnitState.Working)
                {
                    character.SetTool(ToolType.None);
                    yield break;
                }

                tile.Interact();
                
                character.PlayHitAnim(ToolType.Axe);
                
                yield return new WaitForSeconds(harvestTime);
                
            }
        }
    }
}
