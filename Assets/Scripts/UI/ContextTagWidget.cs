using System;
using System.Linq;
using Data.Resources;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ContextTagWidget : MonoBehaviour
    {
        
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        
        public void InitWidget(Sprite icon, string label)
        {
            _icon.sprite = icon;
            _label.text = label;
        }
    }
}
