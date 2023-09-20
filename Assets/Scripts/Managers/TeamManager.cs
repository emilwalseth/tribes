using System.Collections.Generic;
using Characters;
using Player;
using UnityEngine;

namespace Managers
{
    public class TeamManager : MonoBehaviour
    {
        public static TeamManager Instance { get; private set; }
        private void Awake() => Instance = this;
        
        [SerializeField] private List<TeamState> _teams = new ();

        public bool IsValidTeam(int teamIndex)
        {
            return _teams.Count >= teamIndex;
        }
    
        public TeamState GetTeam(int teamIndex)
        {
            return _teams[teamIndex];
        }
        public TeamState AddTeam(int teamIndex)
        {
            _teams.Add(new TeamState(teamIndex));
            return _teams[teamIndex];
        }

        public void DestroyTeam(int teamIndex)
        {
            _teams.RemoveAt(teamIndex);
        }
        public int TeamCount => _teams.Count;
        
        
    }
}
