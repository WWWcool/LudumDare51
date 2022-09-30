using System;
using System.Collections.Generic;

namespace Tutorial.Models
{
    [Serializable]
    public class TutorialDesc
    {
        public string name;
        public bool lockSaves;
        public List<TutorialStepSo> steps;
    }
}