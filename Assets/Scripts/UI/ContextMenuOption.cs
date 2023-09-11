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

        public void StartWoodChopping()
        {
            InteractionManager.Instance.StartChoppingWood();
        }

    }
}
