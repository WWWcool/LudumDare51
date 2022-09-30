using System;
using System.Collections.Generic;
using Core.Anchors;
using Core.SaveLoad;
using Core.Windows;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Zenject;

namespace Core.Conditions
{
    public class ConditionBuilderContext
    {
        public AnchorService anchorService;
        public WindowManager windowManager;
        public SaveService saveService;
    }

    public class ConditionBuilder : MonoBehaviour
    {
        [SerializeField] private float updateTime = 0.1f;

        public DelegateWrap<Action<float>> Updated = new DelegateWrap<Action<float>>();
        private ConditionBuilderContext _context = new ConditionBuilderContext();

        [Inject]
        public void Construct(
            AnchorService anchorService,
            WindowManager windowManager,
            SaveService saveService
        )
        {
            _context.anchorService = anchorService;
            _context.windowManager = windowManager;
            _context.saveService = saveService;
        }

        private void Update()
        {
            Updated.Delegate?.Invoke(Time.deltaTime);
        }

        public ConditionBase CreateCondition(ExpressionDesc desc, UnityAction<bool> callback = null)
        {
            return CreateCondition(desc.expression, desc.conditions, callback);
        }

        public ConditionBase CreateCondition(string expression, ConditionDesc[] conditions, UnityAction<bool> callback)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return null;
            }

            var stack = new Stack<ConditionBase>();
            var tokens = ExpressionParsingUtils.InfixToReversePolishNotation(expression).Split(' ');
            foreach (var token in tokens)
            {
                switch (token)
                {
                    case "&":
                    {
                        var operand1 = stack.Pop();
                        var operand2 = stack.Pop();
                        stack.Push(new ConditionContainer(new[] {operand1, operand2}, EConditionOperation.And));
                    }
                        break;
                    case "|":
                    {
                        var operand1 = stack.Pop();
                        var operand2 = stack.Pop();
                        stack.Push(new ConditionContainer(new[] {operand1, operand2}, EConditionOperation.Or));
                    }
                        break;
                    case "!":
                        var operand = stack.Pop();
                        stack.Push(new ConditionContainer(new[] {operand}, EConditionOperation.Not));
                        break;
                    default:
                        var token1 = token;
                        var desc = Array.Find(conditions, x => x.conditionTag == token1);
                        if (desc != null)
                        {
                            stack.Push(CreateCondition(desc));
                        }
                        else
                        {
                            Debug.LogWarningFormat(
                                "[ConditionBuilder][CreateExpression] Description for tag {0} not found", token);
                        }

                        break;
                }
            }

            if (stack.Count > 0)
            {
                var result = stack.Pop();
                result.Init(callback);
                return result;
            }

            return null;
        }

        private ConditionBase CreateCondition(ConditionDesc desc)
        {
            ConditionBase res = new ConditionTrue();
            switch (desc.type)
            {
                case EConditionType.False:
                    res = new ConditionFalse();
                    break;
                case EConditionType.True:
                    res = new ConditionTrue();
                    break;
                case EConditionType.AllWindowsClosed:
                    res = new ConditionAllWindowsClosed(_context.windowManager);
                    break;
                case EConditionType.WindowOpened:
                    res = new ConditionWindowOpened(desc.windowId, _context.windowManager);
                    break;
                case EConditionType.AnchorVisible:
                    if (_context.anchorService.TryGetAnchor(desc.anchorType, out var anchor0, desc.anchorId))
                    {
                        res = new ConditionAnchorVisible(anchor0);
                    }

                    break;
                case EConditionType.AnchorTap:
                    if (_context.anchorService.TryGetAnchor(desc.anchorType, out var anchor1, desc.anchorId))
                    {
                        res = new ConditionAnchorTap(anchor1);
                    }

                    break;
                case EConditionType.EventTrigger:
                    switch (desc.eventId)
                    {
                        case EEventTriggerId.ScreenTap:
                            res = new ConditionEventTriggered<EvScreenTap>();
                            break;
                        case EEventTriggerId.None:
                            Debug.LogWarning("[ConditionBuilder][CreateCondition] Event id couldn't be None");
                            break;
                        default:
                            Debug.LogWarningFormat("[ConditionBuilder][CreateCondition] Event id not found {0}",
                                desc.eventId);
                            break;
                    }

                    res = new ConditionTrue();
                    break;
                case EConditionType.Extension:
                    res = desc.extension.CreateCondition(_context);
                    break;
                default:
                    Debug.LogWarningFormat("[ConditionBuilder][CreateCondition] Unknown condition type {0}", desc.type);
                    break;
            }

            if (res.Updatable)
            {
                res.InitUpdate(Updated);
            }

            return res;
        }
    }
}