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

        public bool CanProduce(IReadOnlyList<Resource> sources)
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

            var testSources = new List<Resource>(sources);
            
            foreach (var resource in resources)
            {
                if (!testSources.Contains(resource))
                {
                    return false;
                }

                testSources.Remove(resource);
            }
            
            return true;
        }
    }
}