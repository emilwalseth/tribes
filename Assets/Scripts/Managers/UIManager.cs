using System.Collections.Generic;
using Interfaces;
using Tiles;
using UnityEngine;
using ContextMenu = UI.ContextMenu;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
    
        public static UIManager instance;
        private void Awake() => instance = this;

        private RectTransform _openMenu;
        
        
        [SerializeField] private ContextMenu _contextMenu;
        
        
        public void OpenContextMenu(TileScript tile)
        {
            CloseMenu();
            
            
            _contextMenu.OnOpened();

            List<RectTransform> options = tile.TileData.MenuOptions;
            _contextMenu.MakeContextMenu(options);
            _openMenu = _contextMenu.gameObject.GetComponent<RectTransform>();
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
