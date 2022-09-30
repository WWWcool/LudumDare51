namespace Core.Conditions {
	public struct EvScreenTap { }

	public class ConditionEventTriggered<T> : ConditionBase {
		private bool completed;

		public ConditionEventTriggered() {
			// App.The.Events.Subscribe<T>( OnEvent );
		}

		public override void Dispose() {
			// App.The.Events.Unsubscribe<T>( OnEvent );
			base.Dispose();
		}

		public override bool IsTrue => completed;

		private void OnEvent( T ev ) {
			completed = true;
			MarkChanged();
		}
	}
}