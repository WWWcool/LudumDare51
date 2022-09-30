using System;
using Core.Executor.Commands;

namespace Core.Conditions.Commands {
	public class CmdWaitCondition : ICommand {
        private readonly string _expression;
        private readonly ConditionDesc[] _conditionDescs;
        private ConditionBase _conditionOrNull;
        private Action<ICommand> _onFinish;
        private readonly ExpressionDesc _desc;
        private ConditionBuilder _conditionBuilder;
        
        public CmdWaitCondition( ExpressionDesc desc, ConditionBuilder conditionBuilder ) {
            _desc = desc;
            _conditionBuilder = conditionBuilder;
        }

        public void Start( Action<ICommand> onFinish ) {
            _onFinish = onFinish;
            _conditionOrNull = _conditionBuilder.CreateCondition( _desc, OnConditionChanged );
            if ( _conditionOrNull != null ) {
                OnConditionChanged( _conditionOrNull.IsTrue );
            }
        }

        private void OnConditionChanged( bool completed ) {
            if ( completed ) {
                Dispose();
                _onFinish?.Invoke( this );
            }
        }

        private void Dispose() {
            if ( _conditionOrNull != null ) {
                _conditionOrNull.Dispose();
                _conditionOrNull = null;
            }
        }
	}
}