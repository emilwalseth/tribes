using System.Collections.Generic;
using Characters;
using Interfaces;
using Managers;
using UnityEngine;

namespace Tiles
{
    public class TileScript : NavigationNode, IInteractable
    {
    
        [SerializeField] private TileData _tileData;
        [SerializeField] private GameObject _secondLayer;
        public TileData TileData => _tileData;


        public void OnClicked()
        {
            PlayClickedAnimation();

            Character selectedCharacter = SelectionManager.Instance.SelectedCharacter;
            if (selectedCharacter)
            {
                TileScript currentTile = selectedCharacter.GetCurrenTile();
                if (currentTile != this)
                {
                    if (SelectionManager.Instance.IsTileSelected(this))
                    {
                        selectedCharacter.CurrentUnit.NavigateToTile(this);
                        UIManager.instance.CloseMenu();
                        return;
                    }
                }
            }
            
            SelectionManager.Instance.SelectTilesInRadius(_tileData.SelectedRadius, this);
            OnTileClicked();

        }

        void IInteractable.OnSelected()
        {
            OnSelected();
        }

        public void OnDeselected()
        {
            
        }

        protected virtual void OnTileClicked()
        {
            // For override
        }

        private void OnSelected()
        {
            
        }

        private void PlayInteractAnimation()
        {
            AnimationManager.Instance.DoBounceAnim(_secondLayer, 0.25f);
        }

        public void Harvest()
        {
            if (_tileData.Resources.Count == 0) return;
            
            PlayInteractAnimation();
            ScoreManager.Instance.AddResources(_tileData.Resources);
            InteractionManager.Instance.SpawnIndicator(transform.position, _tileData.Resources[0]);
        }
        
        private void PlayClickedAnimation()
        {
            AnimationManager.Instance.DoBounceAnim(gameObject, 0.25f);
        }
    }
}
