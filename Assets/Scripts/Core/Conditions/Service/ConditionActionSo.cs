using System;
using Core.Windows;
using UnityEngine;
using Utils.Attributes;

namespace Core.Conditions.Service
{
    [Serializable]
    [CreateAssetMenu(fileName = "ConditionActionSo", menuName = "Conditions/ConditionActions")]
    public class ConditionActionSo : ScriptableObject
    {
        [SerializeField] private EConditionActionType type;

        [EnumConditionalHide(nameof(type), EConditionActionType.OpenPopup, true)]
        [SerializeField] private EPopupType windowId;

        [SerializeField] private bool oneTime;
        
        [SerializeField] private ExpressionDesc expressionDesc;
        
        public ExpressionDesc ExpressionDesc => expressionDesc;
        public EConditionActionType Type => type;
        public EPopupType WindowId => windowId;

        public bool OneTime => oneTime;
    }
}