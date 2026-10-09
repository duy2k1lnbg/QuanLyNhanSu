using System;

namespace Bu.Services.AI_Services.Core
{
    public interface IClockProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }

    public class SystemClockProvider : IClockProvider
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public class FakeClockProvider : IClockProvider
    {
        private DateTime _customTime;

        public FakeClockProvider(DateTime customTime)
        {
            _customTime = customTime;
        }

        public DateTime Now => _customTime;
        public DateTime UtcNow => _customTime.ToUniversalTime();

        public void SetTime(DateTime newTime)
        {
            _customTime = newTime;
        }

        public void Advance(TimeSpan duration)
        {
            _customTime = _customTime.Add(duration);
        }
    }
}
