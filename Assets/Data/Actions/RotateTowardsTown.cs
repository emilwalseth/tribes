using System.Collections.Generic;
using Managers;
using Tiles;
using UnityEngine;


namespace Data.Actions
{
    [CreateAssetMenu(fileName = "RotateTowardsTown", menuName = "ScriptableActions/RotateTowardsTown", order = 1)]
    public class RotateTowardsTown : ScriptableAction
    {
        public override void Execute(GameObject executor)
        {
            if (!executor.TryGetComponent(out TileScript tileScript)) return;

            if (tileScript.TownTile)
            {
                tileScript.transform.rotation = Quaternion.LookRotation(tileScript.TownTile.transform.position - tileScript.transform.position, Vector3.up);
            }

        }
    }
}
