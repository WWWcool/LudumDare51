using Core.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Windows.AnalyticsSignals
{
    public class PopupOpenedSignal : AnalyticsSignal, IMyTrackerSignal
    {
        public readonly string PopupName;

        public PopupOpenedSignal(string popupName)
        {
            PopupName = popupName;
        }
    }
}
