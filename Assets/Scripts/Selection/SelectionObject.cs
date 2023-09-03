using System;
using System.Collections.Generic;
using UnityEngine;

namespace Selection
{
    public class SelectionObject : MonoBehaviour
    {

        [SerializeField] private List<GameObject> _sections;
        [SerializeField] [Range(0, 63)] private int _testIndex = 0;
    
        public int SelectionIndex { get; private set; }

        private void OnValidate()
        {
            SetSelectionIndex(_testIndex);
        }

        public void SetSelectionIndex(int newIndex)
        {

            SelectionIndex = newIndex;

            if (_sections.Count < 6) return;

            _sections[0].SetActive((SelectionIndex & 0x01) != 0);
            _sections[1].SetActive((SelectionIndex & 0x02) != 0);
            _sections[2].SetActive((SelectionIndex & 0x04) != 0);
            _sections[3].SetActive((SelectionIndex & 0x08) != 0);
            _sections[4].SetActive((SelectionIndex & 0x10) != 0);
            _sections[5].SetActive((SelectionIndex & 0x20) != 0);
            
        }
    }
}
