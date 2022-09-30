using Core.Signals;

namespace Core.Windows.AnalyticsSignals
{
    public class PopupClosedSignal : AnalyticsSignal, IMyTrackerSignal
    {
        public readonly string PopupName;

        public PopupClosedSignal(string popupName)
        {
            PopupName = popupName;
        }
    }
}
