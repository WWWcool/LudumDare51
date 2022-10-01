using System;

namespace Factory
{
    [Serializable]
    public class NodeDestinationState
    {
        public int count;
    }
    
    [Serializable]
    public class NodeDestination
    {
        public int count;
        public NodeView node;
    }
}