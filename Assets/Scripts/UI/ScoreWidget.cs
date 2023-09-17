using System;
using System.Linq;
using Data.Resources;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScoreWidget : MonoBehaviour
    {

        [SerializeField] private ResourceData _resourceData;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _scoreText;

        private void Start()
        {
            _icon.sprite = _resourceData.ResourceIcon;
            EventManager.Instance.onHeroSpawned += character => TeamManager.Instance.GetTeam(0).onStatsChanged += SetScore;
            
        }
        
        
        private void OnDisable()
        {
            if (!TeamManager.Instance.IsValidTeam(0)) return;
            TeamManager.Instance.GetTeam(0).onStatsChanged -= SetScore;
        }

        private void SetScore()
        {
            int count = 0;
            if (TeamManager.Instance.GetTeam(0).Resources.TryGetValue(_resourceData.ResourceType, out int amount))
                count = amount;

            
            string score = count.ToString();
            
            if (score == _scoreText.text) return;
            
            _scoreText.text = score;
            AnimationManager.Instance.DoBounceAnim(gameObject, 0.25f);

        }
    }
}
