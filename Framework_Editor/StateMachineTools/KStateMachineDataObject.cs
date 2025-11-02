using System;
using System.Collections.Generic;
using UnityEngine;

namespace KToolkit
{
    [CreateAssetMenu(fileName = "KStateMachineData", menuName = "KToolkit/KTool StateMachine Data")]
    public class KStateMachineData : ScriptableObject
    {
        [Serializable]
        public class StateNode
        {
            public string stateName;
            public string ownerTypeName = "MonoBehaviour"; // 默认宿主
            public string generateLocation;
            public Vector2 position;
        }

        public List<StateNode> states = new List<StateNode>();
    }
}