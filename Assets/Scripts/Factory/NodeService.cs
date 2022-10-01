using System;
using UnityEngine;

namespace Factory
{
    public class NodeService : MonoBehaviour
    {
        public Action SimulationStarted = () => {};

        private bool _simulationStarted;

        public void StartSimulation()
        {
            if(!_simulationStarted)
            {
                _simulationStarted = true;
                SimulationStarted.Invoke();
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