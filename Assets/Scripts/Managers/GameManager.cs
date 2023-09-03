using System.Collections;
using UnityEngine;


namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        
        // Public Getter
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Link delegates
            EventManager.Instance.onMapGenerated += () => StartCoroutine(SpawnHero());
        }
        
        private IEnumerator SpawnHero()
        {
            yield return null;
            // Spawn the hero at a random tile
            UnitManager.Instance.SpawnHero();
        }


    }
}
