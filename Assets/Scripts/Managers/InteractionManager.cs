using System.Collections;
using Characters;
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


        public void SpawnIndicator(Vector3 position, Resource resource)
        {
            
            Vector3 indicatorPos = position + new Vector3(0, 1.5f, 0);
            Indicator indicator = Instantiate(_indicatorPrefab, indicatorPos, Quaternion.identity);
            indicator.SetImage(resource.ResourceIcon);
        }
        
        public void StartChoppingWood()
        {
            StartHarvesting(1f, 1f);
        }
        
        private void StartHarvesting(float harvestTime, float harvestEfficiency)
        {
            Character character = SelectionManager.Instance.SelectedCharacter;
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
                yield return new WaitForSeconds(harvestTime);
                
                // If the character is no longer working or the currentTile is no longer the one we are in, quit chopping
                if (character.GetState() != UnitState.Working)
                {
                    character.SetTool(ToolType.None);
                    yield break;
                }
                tile.Harvest();
                character.PlayHitAnim(ToolType.Axe);

                print("Harvesting");
            }
        }
    }
}
