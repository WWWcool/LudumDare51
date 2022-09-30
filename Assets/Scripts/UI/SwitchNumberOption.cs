using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class SwitchNumberOption : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI value;
        [SerializeField, Range(0.5f, 2.5f)] private float multiplier = 1f;
        [SerializeField] private float multiplierMax = 2.5f;
        [SerializeField] private float multiplierMin = 0.5f;
        [SerializeField] private float step = 0.1f;
        
        public float Value
        {
            get => _value;
            set
            {
                _value = value;
                this.value.text = $"{_value:N1}";
            }
        }

        public event Action<float> ValueChanged = v => { };

        private float _value;

        public void OnLeftClicked(){
            Value = Mathf.Clamp(Value - step, multiplierMin, multiplierMax);
            ValueChanged.Invoke(Value);
        }
        public void OnRightClicked(){
            Value = Mathf.Clamp(Value + step, multiplierMin, multiplierMax);
            ValueChanged.Invoke(Value);
        }
    }
}