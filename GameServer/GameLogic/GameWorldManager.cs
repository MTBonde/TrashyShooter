using System.Timers;

using SharedData;

namespace GameServer
{
    /// <summary>
    /// Klasse til at styrer spil verden og dens opsætning
    /// </summary>
    public class GameWorldManager
    {
        // Timer til at styre spilrunden
        private System.Timers.Timer gameRoundTimer;
        // Timer for 5sek countdown
        private System.Timers.Timer countdownTimer;

        // Tid i sekunder
        private int elapsedTimeInSeconds = 0;
        private int countdownValue = 5;

        // Definer events
        public event Action GameRoundStarted;
        public event Action GameRoundEnded;
        public event Action<int> CountdownTick;
        public event Action<int> CountdownStarted;

        public bool GameRoundStartet = false;

        public GameWorldManager()
        {
            InitializeGameWorld();
        }

        /// <summary>
        /// Initialiser spilverdenen
        /// </summary>
        private void InitializeGameWorld()
        {
            // 3 minutter i millisekunder
            gameRoundTimer = new System.Timers.Timer(1000);
            // Event der udløses ved runde slut
            //gameRoundTimer.Elapsed += OnGameRoundEnd;
            // Kør kun eventet én gang
            gameRoundTimer.AutoReset = true;

            // 1 second in milliseconds
            countdownTimer = new System.Timers.Timer(1000);
            countdownTimer.AutoReset = true;
        }

        /// <summary>
        /// Start 5-second countdown
        /// </summary>
        public void StartGameStartCountdown()
        {
            ResetCountdown();
            countdownTimer.Elapsed += OnGameStartCountdownTick;
            countdownTimer.Start();
            CountdownStarted?.Invoke(countdownValue);
        }

        /// <summary>
        /// Nulstil Countdown
        /// </summary>
        private void ResetCountdown()
        {
            countdownValue = 5;
        }

        /// <summary>
        /// Start 5-second countdown for spil slut
        /// </summary>
        public void StartGameEndWarning()
        {
            ResetCountdown();
            countdownTimer.Elapsed += OnGameEndWarningTick;
            StartCountdown();
            CountdownStarted?.Invoke(countdownValue);
        }

        /// <summary>
        /// Generic Start Countdown
        /// </summary>
        private void StartCountdown()
        {
            countdownTimer.Start();
            CountdownStarted?.Invoke(5);
        }

        /// <summary>
        /// Countdown tick event til game start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameStartCountdownTick(object sender, ElapsedEventArgs e)
        {
            HandleCountdownTick();
        }

        /// <summary>
        /// Countdown tick event for spilslut
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameEndWarningTick(object sender, ElapsedEventArgs e)
        {
            HandleCountdownTick();
            countdownTimer.Elapsed -= OnGameEndWarningTick;
        }

        /// <summary>
        /// Countdown Tick Håndtering
        /// </summary>
        private void HandleCountdownTick()
        {
            // Opdaterer serverinformationen
            ServerInfoMessage serverInfo = new ServerInfoMessage()
            {
                ServerInformation = $"T:"
            };

            // Sender serverinformationen til klienterne.
            MessageSender.SendDataToClients(serverInfo, MessageType.ServerInfoMessage, MessagePriority.Low);

            CountdownTick?.Invoke(countdownValue);

             serverInfo = new ServerInfoMessage()
            {
                ServerInformation = countdownValue >= 0 ? $"M:Game Round start in {countdownValue}" : "M:"
            };

            MessageSender.SendDataToClients(serverInfo, MessageType.ServerInfoMessage, MessagePriority.Low);

            if(countdownValue <= 0)
            {
                countdownTimer.Stop();
                countdownTimer.Elapsed -= OnGameStartCountdownTick;  // Detach event

                // Clear the countdown display on clients by sending an empty message
                serverInfo.ServerInformation = "M:";
                MessageSender.SendDataToClients(serverInfo, MessageType.ServerInfoMessage, MessagePriority.Low);

                // Start the new game round
                StartNewGameRound();
            }
            else
            {
                countdownValue--;  // Decrement countdown value
            }
        }


        /// <summary>
        /// Start en ny spilrunde
        /// </summary>
        public void StartNewGameRound()
        {
            // Set the initial time to 180 seconds (3 minutes)
            elapsedTimeInSeconds = 180;

            // Indicate that a game round has started
            GameRoundStartet = true;

            // Detach any existing event handlers to avoid multiple triggers
            gameRoundTimer.Elapsed -= OnTimedEvent;

            // Attach the event handler
            gameRoundTimer.Elapsed += OnTimedEvent;

            // Start the game round timer
            gameRoundTimer.AutoReset = true;  // Make sure the timer keeps ticking
            gameRoundTimer.Start();

            // Trigger the game round started event
            GameRoundStarted?.Invoke();
        }


        /// <summary>
        /// Eventhandler for timerens Elapsed-event
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            // nedtælling
            elapsedTimeInSeconds--;            

            // Opdaterer serverinformationen
            ServerInfoMessage serverInfo = new ServerInfoMessage()
            {
                ServerInformation = $"T:Game Time left {elapsedTimeInSeconds} seconds"
            };

            // Sender serverinformationen til klienterne.
            MessageSender.SendDataToClients(serverInfo, MessageType.ServerInfoMessage, MessagePriority.Low);

            //// Trigger 5-second warning
            //if(elapsedTimeInSeconds <= 5)
            //{
            //    StartGameEndWarning();
            //}

            // Stop the timer when time reaches zero
            if(elapsedTimeInSeconds <= 0)
            {
                gameRoundTimer.Stop();  // Stop the timer
                GameRoundEnded?.Invoke();  // Trigger the game round ended event
            }
        }

        /// <summary>
        /// Afslut spilrunden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameRoundEnd(object sender, ElapsedEventArgs e)
        {
            // Stopper timeren
            gameRoundTimer.Stop();
            // Udløser GameRoundEnded event
            GameRoundEnded?.Invoke();
            Console.WriteLine("Spilrunden er slut!");
        }
    }
}
