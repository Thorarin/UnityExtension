using System;

namespace System.Diagnostics
{
    public class Stopwatch
    {
        private DateTime? _start;
        private DateTime? _stop;

        public TimeSpan Elapsed
        {
            get
            {
                if (!_start.HasValue)
                {
                    throw new InvalidOperationException("Stopwatch has not been started.");
                }

                if (_stop.HasValue)
                {
                    return _stop.Value - _start.Value;
                }

                return DateTime.UtcNow - _start.Value;
            }
        }

        public void Start()
        {
            _start = DateTime.UtcNow;
            _stop = null;
        }

        public void Stop()
        {
            _stop = DateTime.UtcNow;
        }

        public void Restart()
        {
            Start();
        }
    }
}