using System.Collections.Generic;
using Core.Executor.Commands;

namespace Core.Executor {
	public class Executor : Sequence {
		public static readonly List<Executor> DEBUG_EXECUTORS_LIST = new List<Executor>();
		public readonly string DebugName;

		public Executor( string debugName ) {
			DebugName = debugName;
			DEBUG_EXECUTORS_LIST.Add( this );
		}

		public void Dispose() {
			DEBUG_EXECUTORS_LIST.Remove( this );
		}

		public override string ToString() {
			return DebugName + ":" + DebugReport( Commands.Utils.Delimiter + Commands.Utils.Space );
		}
	}
}