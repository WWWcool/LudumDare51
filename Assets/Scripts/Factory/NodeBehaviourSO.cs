using System.Collections.Generic;
using UnityEngine;

namespace Factory
{
    [CreateAssetMenu(fileName = "NodeBehaviourSO", menuName = "Nodes")]
    public class NodeBehaviourSO : ScriptableObject
    {
        [SerializeField] private List<RecipeSO> recipes = new();

        [SerializeField] private bool canPull;

        public bool TryGetRecipe(List<Resource> sources, out RecipeSO recipe)
        {
            foreach (var recipeSo in recipes)
            {
                if (recipeSo.CanProduce(sources))
                {
                    recipe = recipeSo;
                    return true;
                }
            }

            recipe = null;
            return false;
        }  
    }
}