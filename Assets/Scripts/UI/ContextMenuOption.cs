using System.Collections;
using Characters;
using Managers;
using Tiles;
using UnityEngine;

namespace UI
{
    public class ContextMenuOption : MonoBehaviour
    {

        public void MakeCampsite()
        {
            TileScript tile = SelectionManager.Instance.GetSelectedTile();
            TileManager.Instance.CreateTown(tile, 0);
        }
        

        public void Attack()
        {

            //GameObject enemy = SelectionManager.Instance.;
            //Unit selectedUnit = SelectionManager.Instance.GetSelectedUnit();
            //if (!selectedUnit) return;
            //
            //Unit enemyUnit = enemy.GetComponent<Unit>();
            //if (!enemyUnit)
            //{
            //    if (!enemy.TryGetComponent(out Character character)) return;
            //    enemyUnit = character.CurrentUnit;
            //}
            //
            //selectedUnit.Attack(enemyUnit);

        }

    }
}
