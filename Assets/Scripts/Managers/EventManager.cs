using Characters;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class EventManager : MonoBehaviour
    {
    
        public static EventManager Instance { get; private set; }

        // Events
        public UnityAction onMapGenerated;
        public UnityAction<Character> onHeroSpawned;
    

        private void Awake() => Instance = this;
        
    }
}
