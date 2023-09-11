using System.Collections.Generic;
using Managers;
using Tiles;
using UnityEngine;


namespace Data.Actions
{
    [CreateAssetMenu(fileName = "ClaimLand", menuName = "ScriptableActions/ClaimLand", order = 1)]
    public class ClaimLand : ScriptableAction
    {
        public override void Execute(GameObject executor)
        {
            if (!executor.TryGetComponent(out TileScript tileScript)) return;
            
            List<TileScript> tiles = SelectionManager.Instance.GetRadius(tileScript.TileData.SelectionRadius, tileScript);
            
            foreach (TileScript tile in tiles)
            {
                tile.SetTown(tileScript);
            }
        }
    }
}
