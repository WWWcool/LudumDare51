using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Factory
{
    [Serializable]
    public class NodeActiveInterval
    {
        public float startDelay;
        public float duration;
    }

    [Serializable]
    public class NodeActiveData
    {
        public List<NodeActiveInterval> intervals;
        public string id;
    }

    [Serializable]
    public class NodeActiveState
    {
        public string id;
        public int intervalIndex = 0;
        public bool intervalInDelay;
    }
    public class World : MonoBehaviour
    {
        [SerializeField] private List<NodeActiveData> activeData = new();
        [SerializeField] private List<NodeView> sourceNodes = new();
        
        private Dictionary<string, NodeView> _nodes = new();
        private Dictionary<string, Timer> _nodeTimers = new();
        private Dictionary<string, NodeActiveState> _nodeStates = new();
        
        private bool _simulationStarted;

        private void Update()
        {
            if (_simulationStarted)
            {
                foreach (var data in activeData)
                {
                    if (_nodes.ContainsKey(data.id))
                    {
                        var timer = _nodeTimers[data.id];
                        timer.Update(Time.deltaTime);
                    }
                }
            }
        }
        
        public void StartSimulation()
        {
            _simulationStarted = true;
            foreach (var sourceNode in sourceNodes)
            {
                RegisterSourceNode(sourceNode);
            }
        }

        private void RegisterSourceNode(NodeView node)
        {
            if (!_nodes.ContainsKey(node.NodeName))
            {
                _nodes.Add(node.NodeName, node);
                var timer = new Timer();
                timer.Init(GetData(node.NodeName).intervals[0].startDelay);
                timer.TimerFinished += () => OnTimerFinished(node.NodeName);
                _nodeTimers.Add(node.NodeName, timer);
                _nodeStates.Add(node.NodeName, new NodeActiveState {id = node.NodeName, intervalInDelay = true});
            }
        }
        
        private void OnTimerFinished(string nodeId)
        {
            var state = _nodeStates[nodeId];
            var timer = _nodeTimers[nodeId];
            var node = _nodes[nodeId];
            var data = GetData(nodeId);

            if (state.intervalInDelay)
            {
                state.intervalInDelay = false;
                node.SetActive(true);
                timer.Init(data.intervals[state.intervalIndex].duration);
            }
            else
            {
                state.intervalIndex++;
                node.SetActive(false);
                if(state.intervalIndex < data.intervals.Count)
                {
                    state.intervalInDelay = true;
                    timer.Init(data.intervals[state.intervalIndex].startDelay);
                }
            }
        }

        private NodeActiveData GetData(string id) => activeData.Find(d => d.id == id);
    }
}