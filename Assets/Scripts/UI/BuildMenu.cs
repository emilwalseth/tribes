using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BuildMenu : MonoBehaviour, IMenuInterface
    {
        
        public void OnOpened()
        {
            gameObject.SetActive(true);
        }

        public void OnClosed()
        {
            gameObject.SetActive(false);
        }
    }
}
