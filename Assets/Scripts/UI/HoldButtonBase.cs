using System;
using UI.Interfaces;
using UnityEngine;

namespace Game.UI
{
    public abstract class HoldButtonBase : MonoBehaviour, IHoldButton
    {
        public event Action<float> HoldPerformed = _ => { };
        public event Action<bool> ToggleHold = _ => { };

        [SerializeField] private AnimationCurve curve;

        private bool _isHolding = false;
        private float _holdingDuration = 0f;

        public void Release()
        {
            _isHolding = false;
        }

        protected void ToggleHolding(bool hold)
        {
            _holdingDuration = 0f;
            _isHolding = hold;

            ToggleHold.Invoke(hold);
        }

        private void Update()
        {
            if (_isHolding)
            {
                _holdingDuration += Time.deltaTime;

                var progress = curve.Evaluate(_holdingDuration);
                HoldPerformed.Invoke(progress);
            }
        }
    }
}
