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
            BuildingManager.Instance.MakeCampsite();
        }

        public void StartWoodChopping()
        {
            InteractionManager.Instance.StartChoppingWood();
        }

    }
}
