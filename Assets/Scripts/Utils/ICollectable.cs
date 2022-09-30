using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public interface ICollectable<T> where T : ScriptableObject
    {
        void SetData(List<T> data);
        string GetRootFolder();
    }
}