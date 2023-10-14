using Data.Resources;
using UnityEngine;

namespace Managers
{
    public class CheatManager : MonoBehaviour
    {
        public static CheatManager Instance { get; private set; }

        private void Awake() => Instance = this;


        public static void AddWood(int amount)
        {
            TeamManager.Instance.GetTeam(0).AddResource(ResourceType.Wood, amount);
        }
    
    }
}
