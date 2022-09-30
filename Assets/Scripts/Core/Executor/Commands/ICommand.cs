using System;

namespace Core.Executor.Commands {
	public interface ICommand {
		void Start( Action<ICommand> onFinish );
	}

	public interface ICommandDebugReport {
		string DebugReport( string indent );
	}
}