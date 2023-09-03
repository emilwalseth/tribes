using System;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class ScoreManager : MonoBehaviour
    {
        
        public static ScoreManager Instance { get; private set; }
        private void Awake() => Instance = this;

        private int _woodCount = 0;
        public UnityAction<int> onWoodChanged;

    
        public void AddResources(List<Resource> resources)
        {
            foreach (Resource resource in resources)
            {
                int count = resource.Amount;

                switch (resource.ResourceType)
                {
                    case ResourceType.Wood:
                        _woodCount += count;
                        onWoodChanged?.Invoke(_woodCount);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
