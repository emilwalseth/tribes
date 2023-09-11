using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Data.Actions
{
    
    public class ScriptableAction : ScriptableObject
    {
        public virtual void Execute(GameObject executor){}
    }
}
