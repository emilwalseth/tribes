using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ContextMenu : MonoBehaviour, IMenuInterface
    {
        
        private readonly List<RectTransform> _currentMenuOptions = new();
    
        
        
        public void MakeContextMenu(List<RectTransform> menuOptions)
        {
            ClearMenu();
            foreach (RectTransform option in menuOptions)
            {
                RectTransform newOption = Instantiate(option, transform, true);
                _currentMenuOptions.Add(newOption);
            }
        }
    
        private void ClearMenu()
        {
            foreach (RectTransform option in _currentMenuOptions.Where(option => option))
            {
                Destroy(option.gameObject);
            }

            _currentMenuOptions.Clear();
        }

        public void OnOpened()
        {
            gameObject.SetActive(true);
        }

        public void OnClosed()
        {
            ClearMenu();
            gameObject.SetActive(false);
        }
    }
}
