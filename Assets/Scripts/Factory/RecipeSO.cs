using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Factory
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "Recipes", order = 0)]
    public class RecipeSO : ScriptableObject
    {
        [SerializeField] private float produceTime = 1f;
        [SerializeField] private List<Resource> resources = new();
        [SerializeField] private Resource resultResource;
        [SerializeField] private bool strictOrder;

        public float ProduceTime => produceTime;
        public Resource ResultResource => resultResource;

        public bool CanProduce(List<Resource> sources)
        {
            if (resources.Count == 0) return true;
            if (resources.Count != sources.Count) return false;
            
            if (strictOrder)
            {
                var res = true;
                for (var i = 0; i < resources.Count; i++)
                {
                    if (resources[i] != sources[i])
                    {
                        res = false;
                        break;
                    }
                }

                return res;
            }
            
            foreach (var resource in resources)
            {
                if (!sources.Contains(resource))
                {
                    return false;
                }

                sources.Remove(resource);
            }
            
            return true;
        }
    }
}