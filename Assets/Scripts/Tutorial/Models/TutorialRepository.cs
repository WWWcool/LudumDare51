using System.Collections.Generic;
using UnityEngine;

namespace Tutorial.Models
{
    [CreateAssetMenu(fileName = "TutorialRepository", menuName = "Repositories/Tutorial")]
    public class TutorialRepository : ScriptableObject
    {
        public string tutorialVersion;
        public bool tutorialEnabled;
        public List<TutorialDesc> tutorials = new List<TutorialDesc>();
    }
}
