using System;

namespace Factory
{
    [Serializable]
    public enum ResourceType
    {
        Test1,
        Test2,
        Test3,
        Trash
    }
    
    [Serializable]
    public class Resource : IEquatable<Resource>
    {
        public ResourceType type;

        public Resource(){}
        public Resource(ResourceType type)
        {
            this.type = type;
        }
        
        public bool Equals(Resource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return type == other.type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Resource) obj);
        }

        public override int GetHashCode()
        {
            return (int) type;
        }
    }
}