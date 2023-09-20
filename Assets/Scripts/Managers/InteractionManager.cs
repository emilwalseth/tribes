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


        public void SpawnIndicator(Vector3 position, Sprite sprite)
        {
            Vector3 indicatorPos = position + new Vector3(0, 1.5f, 0);
            Indicator indicator = Instantiate(_indicatorPrefab, indicatorPos, Quaternion.identity);
            indicator.SetImage(sprite);
        }
        
        public void StartHarvesting(Character character)
        {
            float harvestTime = character.CharacterData.HarvestSpeed;
            StartHarvesting(character, harvestTime, 1f);
        }
        
        private void StartHarvesting(Character character, float harvestTime, float harvestEfficiency)
        {
            if (!character) return;
            character.SetState(CharacterState.Working);
            StartCoroutine(Harvesting(character, harvestTime, harvestEfficiency));
        }
        
        private static IEnumerator Harvesting(Character character, float harvestTime, float harvestEfficiency)
        {
            
            while (true)
            {
                if (!character)
                    yield break;
                
                TileScript tile = character.GetCurrenTile();

                if (tile.TryGetComponent(out ResourceTileScript resource))
                    character.SetTool(resource.ToolType);
                
                // If the character is no longer working or the currentTile is no longer the one we are in, quit chopping
                if (character.State != CharacterState.Working || !tile)
                {
                    character.SetTool(ToolType.None);
                    yield break;
                }

                tile.Interact(character);
                character.PlayAttackAnim();
                
                yield return new WaitForSeconds(harvestTime);
                
            }
        }
    }
}
