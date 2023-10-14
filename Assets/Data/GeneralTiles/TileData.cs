using System;
using System.Collections.Generic;
using Data.Actions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Data.GeneralTiles
{
    public enum TileType
    {
        Generic,
        Resource,
        Building
    }
    public enum GroundType
    {
        Grass,
        Town,
        Water,
        Stone,
    }
    
    [CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/TileData", order = 1)]
    public class TileData : ScriptableObject
    {

        [Header("General")]
        [SerializeField] private Sprite _tileIcon;
        [SerializeField] private string _tileName;
        [SerializeField] private TileType _tileType;
        [SerializeField] private GroundType _groundType;
        [SerializeField] private int _selectionRadius;
        
        [Header("Navigation")]
        [SerializeField] private int _movementCost = 0;
        [SerializeField] private bool _isWalkable = true;
        
        [Header("Visual")]
        [SerializeField] private GameObject _tile;

        [Header("Menu")]
        [SerializeField] private List<RectTransform> _menuOptions;

        [Header("Actions")] 
        [SerializeField] private List<ScriptableAction> _onTilePlaced;
        
        public Sprite TileIcon => _tileIcon;
        public string TileName => _tileName;
        public TileType TileType => _tileType;
        public GroundType GroundType => _groundType;
        public int SelectionRadius => _selectionRadius;
        public int MovementCost => _movementCost;
        public bool IsWalkable => _isWalkable;
        public GameObject Tile => _tile;
        public List<RectTransform> MenuOptions => _menuOptions;
        
        public List<ScriptableAction> OnTilePlaced => _onTilePlaced;
    }
}