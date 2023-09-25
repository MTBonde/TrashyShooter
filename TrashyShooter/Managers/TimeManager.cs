using System.Diagnostics;

namespace MultiplayerEngine
{
    /// <summary>
    /// Time Manager Class by Thor
    /// </summary>
    public class TimeManager
    {
        // Stopwatch is build into diagnostics
        private Stopwatch _stopwatch;

        /// <summary>
        /// Initializes a new instance of the TimeManager class.
        /// </summary>
        public TimeManager()
        {
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Start the timer.
        /// </summary>
        public void StartTimer()
        {
            _stopwatch.Start();
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void StopTimer()
        {
            _stopwatch.Stop();
        }

        /// <summary>
        /// Reset the timer to zero.
        /// </summary>
        public void ResetTimer()
        {
            _stopwatch.Reset();
        }

        /// <summary>
        /// Gets the current elapsed time and return it as a TimeSpan.
        /// </summary>
        /// <returns>TimeSpan is the current elapsed time of the timer.</returns>
        public TimeSpan GetElapsedTime()
        {
            return _stopwatch.Elapsed;
        }
    }
}
