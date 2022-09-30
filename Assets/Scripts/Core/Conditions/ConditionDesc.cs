using System;
using Core.Anchors;
using Core.Windows;

namespace Core.Conditions {
	[Serializable]
	public class ConditionDesc {
        public string conditionTag = "A";
		public EConditionType type;
		public EPopupType windowId;
        public EAnchorType anchorType;
        public string anchorId;
		public EEventTriggerId eventId;
        public int num;
        // For Nani
        public string scriptName;
        public ConditionDescExtension extension;
    }
}