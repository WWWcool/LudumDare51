using System.Collections.Generic;

namespace Core.Signals
{
    public interface IMyTrackerSignal { }

    public abstract class AnalyticsSignal : ISignal
    {
        public virtual bool IsFlush { get; } = false;
    }
}