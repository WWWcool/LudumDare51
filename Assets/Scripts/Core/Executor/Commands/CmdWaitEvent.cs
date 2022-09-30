using System;
using Utils;

namespace Core.Executor.Commands {
	public class CmdWaitEvent<T> : ICommand {
		private Action<ICommand> onFinishAction;

		public void Start( Action<ICommand> onFinish ) {
			// App.The.Events.Subscribe<T>( OnEvent );
			onFinishAction = onFinish;
		}

		protected virtual void OnEvent( T ev ) {
			// App.The.Events.Unsubscribe<T>( OnEvent );
			onFinishAction( this );
		}

		public override string ToString() {
			return ObjectInfo.Report( this, typeof( T ).Name );
		}
	}
}