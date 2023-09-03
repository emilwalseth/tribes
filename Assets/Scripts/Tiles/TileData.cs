using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using World;

namespace Tiles
{
    [CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/TileData", order = 1)]
    public class TileData : ScriptableObject
    {

        [SerializeField] private TileTypes _tileType = TileTypes.Grass;
        [SerializeField] private int _selectedRadius= 0;
        [SerializeField] private List<RectTransform>  _menuOptions = new();
        [SerializeField] private List<Resource> _resources = new();
        
        
        public TileTypes TileType => _tileType;
        public int SelectedRadius => _selectedRadius;
        public List<RectTransform> MenuOptions => _menuOptions;
        public List<Resource> Resources => _resources;
    }
}
