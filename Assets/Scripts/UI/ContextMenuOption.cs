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
            TileManager.Instance.CreateTown(tile);
        }

        public void StartHarvesting()
        {
            InteractionManager.Instance.StartHarvesting();
        }

        public void Attack()
        {

            GameObject enemy = SelectionManager.Instance.MarkedEnemy;
            Unit selectedUnit = SelectionManager.Instance.GetSelectedUnit();
            if (!selectedUnit) return;

            if (!enemy.TryGetComponent(out Character character)) return;
            selectedUnit.Attack(character.CurrentUnit);
            SelectionManager.Instance.DeselectEnemy();
            UIManager.instance.CloseMenu();

        }

    }
}
