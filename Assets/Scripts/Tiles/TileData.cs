using System;
using UnityEngine;
using World;

namespace Tiles
{
    [CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/TileData", order = 1)]
    public class TileData : ScriptableObject
    {

        public TileTypes _tileType;

    }
}
