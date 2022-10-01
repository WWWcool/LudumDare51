using System.Collections.Generic;
using UnityEngine;
using Utils;
using Zenject;

namespace Factory
{
    public class NodeView : MonoBehaviour
    {
        [SerializeField] private List<NodeSource> sources = new();
        [SerializeField] private List<NodeDestination> destinations = new();
        [SerializeField] private bool hasOutputPool;
        [SerializeField] private bool alwaysActive = true;
        [SerializeField] private bool isSource;
        [SerializeField] private RecipeSO recipe;
        [SerializeField] private NodeBehaviourSO behaviour;
        [SerializeField] private bool isLast;
        [SerializeField] private string nodeName;

        private bool _simulationStarted;
        private bool _activated;
        private Timer _timer = new();
        private List<Resource> _inputPool = new();
        private List<Resource> _outputPool = new();
        private bool _produceStarted;
        private RecipeSO _currentRecipe;
        private NodeDestination _currentDestination;
        private Dictionary<NodeDestination, NodeDestinationState> _destinationStates = new();

        private NodeService _nodeService;

        [Inject]
        public void Construct(NodeService nodeService)
        {
            _nodeService = nodeService;
            _nodeService.SimulationStarted += OnSimulationStarted;
            _timer.TimerFinished += OnTimerFinished;
            if (isSource)
            {
                _currentRecipe = recipe;
                _timer.Init(_currentRecipe.ProduceTime);
                _produceStarted = true;
            }

            foreach (var destination in destinations)
            {
                _destinationStates.Add(destination, new NodeDestinationState());
            }
        }

        private void OnDestroy()
        {
            _nodeService.SimulationStarted -= OnSimulationStarted;
            _timer.TimerFinished -= OnTimerFinished;
        }

        private void Update()
        {
            if (!_simulationStarted) return;

            if (_activated || alwaysActive)
            {
                if (_produceStarted)
                {
                    _timer.Update(Time.deltaTime);
                }
            }
        }

        public void PushResource(Resource resource)
        {
            Debug.Log($"[NodeView][PushResource][{nodeName}] You got new resource {resource.type}!");
            _inputPool.Add(resource);
            
            if (_produceStarted)
            {
                _produceStarted = false;
                _timer.Reset();
            }

            if (behaviour.TryGetRecipe(_inputPool, out var recipe))
            {
                _currentRecipe = recipe;
                _produceStarted = true;
                _timer.Init(_currentRecipe.ProduceTime);
            }
        }

        private void OnTimerFinished()
        {
            if (_currentRecipe == null) return;

            if (_currentRecipe.ResultResource.type == ResourceType.Trash)
            {
                Debug.Log($"[NodeView][OnTimerFinished][{nodeName}] You crafted trash!");
                return;
            }
            
            if (isLast)
            {
                Debug.Log($"[NodeView][OnTimerFinished][{nodeName}] You crafted what you need! Congratulations!");
            }

            if (hasOutputPool)
            {
                _outputPool.Add(_currentRecipe.ResultResource);
                // TODO: add some self updated event
            }
            else
            {
                // choose to which dest push
                if (_currentDestination == null)
                {
                    _currentDestination = destinations[0];
                    _destinationStates[_currentDestination].count = 0;
                }

                if (_destinationStates[_currentDestination].count >= _currentDestination.count)
                {
                    var index = destinations.IndexOf(_currentDestination);
                    _currentDestination = destinations[(index + 1) % destinations.Count];
                    _destinationStates[_currentDestination].count = 0;
                }

                _currentDestination.node.PushResource(_currentRecipe.ResultResource);
                _destinationStates[_currentDestination].count++;
            }
        }

        private void OnSimulationStarted()
        {
            _simulationStarted = true;
        }
    }
}