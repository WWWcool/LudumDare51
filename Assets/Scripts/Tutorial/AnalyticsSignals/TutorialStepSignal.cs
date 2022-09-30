using Core.Signals;

namespace Tutorial.AnalyticsSignals
{
    public class TutorialStepSignal : AnalyticsSignal, IMyTrackerSignal
    {
        public readonly int Index;
        public readonly string Id;

        public TutorialStepSignal(int index, string id)
        {
            Index = index;
            Id = id;
        }
    }
}
