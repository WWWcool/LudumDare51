using System;

namespace Core.Conditions {
	[Serializable]
	public class ExpressionDesc {
		public ConditionDesc[] conditions;
		public string expression = "A";
	}
}