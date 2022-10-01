using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Factory
{
    

    public class NodeService : MonoBehaviour
    {
        [SerializeField] private World testWorldPrefab;
        [SerializeField] private Transform worldRoot;
        
        public Action SimulationStarted = () => { };

        private bool _simulationStarted;
        private World _currentWorld;

        private void Start()
        {
            _currentWorld = Instantiate(testWorldPrefab, worldRoot);
        }

        public void StartSimulation()
        {
            if (!_simulationStarted)
            {
                _simulationStarted = true;
                SimulationStarted.Invoke();
                _currentWorld.StartSimulation();
            }
        }

        public void Reload()
        {
            // TODO
            // save all time settings
            // reload scene
        }
    }
}
