using System.Collections.Generic;
using Interfaces;
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

        private RectTransform _openMenu;
        
        
        
        [SerializeField] private BuildMenu _buildMenu;
        [SerializeField] private ContextMenu _contextMenu;
        
        
        public void OpenBuildMenu(TileScript tile)
        {
            CloseMenu();

            _openMenu = _buildMenu.gameObject.GetComponent<RectTransform>();
            
            OpenMenu();
        }
        
        public void OpenContextMenu(List<RectTransform> menuOptions)
        {
            CloseMenu();
            
            _contextMenu.MakeContextMenu(menuOptions);
            _openMenu = _contextMenu.gameObject.GetComponent<RectTransform>();
            
            OpenMenu();
        }

        private void OpenMenu()
        {
            if (_openMenu && _openMenu.TryGetComponent(out IMenuInterface menu))
            {
                menu.OnOpened();
            }
        }

        public void CloseMenu()
        {
            if (_openMenu && _openMenu.TryGetComponent(out IMenuInterface menu))
            {
                menu.OnClosed();
            }
            
        }
    }
}
