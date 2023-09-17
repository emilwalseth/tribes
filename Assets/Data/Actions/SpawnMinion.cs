using System.Collections.Generic;
using Managers;
using Tiles;
using UnityEngine;


namespace Data.Actions
{
    [CreateAssetMenu(fileName = "SpawnMinion", menuName = "ScriptableActions/SpawnMinion", order = 1)]
    public class SpawnMinion : ScriptableAction
    {
        public override void Execute(GameObject executor)
        {
            if (!executor.TryGetComponent(out TileScript tileScript)) return;
            UnitManager.Instance.SpawnMinion(tileScript, tileScript.ClaimedByTeam);
        }
    }
}
