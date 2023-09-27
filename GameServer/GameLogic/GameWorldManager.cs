using System;
using System.Timers;

using SharedData;

namespace GameServer
{
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

        // Initialiser spilverdenen
        private void InitializeGameWorld()
        {
            // 3 minutter i millisekunder
            gameRoundTimer = new System.Timers.Timer(180000);
            // Event der udløses ved runde slut
            gameRoundTimer.Elapsed += OnGameRoundEnd;
            // Kør kun eventet én gang
            gameRoundTimer.AutoReset = true;

            // 1 second in milliseconds
            countdownTimer = new System.Timers.Timer(1000); 
            countdownTimer.AutoReset = true;
        }

        // Start 5-second countdown for game start
        public void StartGameStartCountdown()
        {
            ResetCountdown();
            countdownTimer.Elapsed += OnGameStartCountdownTick;
            countdownTimer.Start();
            CountdownStarted?.Invoke(countdownValue);
        }

        private void ResetCountdown()
        {
            countdownValue = 5;
        }

        // Start 5-second countdown for game end warning
        public void StartGameEndWarning()
        {
            countdownTimer.Elapsed += OnGameEndWarningTick;
            StartCountdown();
            CountdownStarted?.Invoke(5);
        }

        // Generic Start Countdown
        private void StartCountdown()
        {
            countdownTimer.Start();
            CountdownStarted?.Invoke(5);
        }

        // Countdown tick event for game start
        private void OnGameStartCountdownTick(object sender, ElapsedEventArgs e)
        {
            HandleCountdownTick();
        }

        // Countdown tick event for game end warning
        private void OnGameEndWarningTick(object sender, ElapsedEventArgs e)
        {
            HandleCountdownTick();
            countdownTimer.Elapsed -= OnGameEndWarningTick;
        }

        // Shared countdown tick logic
        private void HandleCountdownTick()
        {
            CountdownTick?.Invoke(countdownValue);

            ServerInfoMessage serverInfo = new ServerInfoMessage()
            {
                ServerInformation = countdownValue >= 0 ? $"M:Game Round start in {countdownValue}" : ""
            };

            MessageSender.SendDataToClients(serverInfo, MessageType.ServerInfoMessage, MessagePriority.Low);

            if(countdownValue <= 0)
            {
                countdownTimer.Stop();
                countdownTimer.Elapsed -= OnGameStartCountdownTick;  // Detach event

                // Clear the countdown display on clients by sending an empty message
                serverInfo.ServerInformation = "";
                MessageSender.SendDataToClients(serverInfo, MessageType.ServerInfoMessage, MessagePriority.Low);

                // Start the new game round
                StartNewGameRound();
            }
            else
            {
                countdownValue--;  // Decrement countdown value
            }
        }


        // Start en ny spilrunde
        public void StartNewGameRound()
        {
            // Reset elapsed time
            elapsedTimeInSeconds = 0;

            // Indicate that a game round has started
            GameRoundStartet = true;

            // Detach any existing event handlers to avoid multiple triggers
            gameRoundTimer.Elapsed -= OnTimedEvent;

            // Attach the event handler
            gameRoundTimer.Elapsed += OnTimedEvent;

            // Start the game round timer
            gameRoundTimer.Start();

            // Trigger the game round started event
            GameRoundStarted?.Invoke();
        }


        // Eventhandler for timerens Elapsed-event
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            // Opdaterer forløbet tid
            elapsedTimeInSeconds++;

            // Opdaterer serverinformationen
            ServerInfoMessage serverInfo = new ServerInfoMessage()
            {
                ServerInformation = $"T:Game Time elapsed {elapsedTimeInSeconds} seconds"
            };

            // Sender serverinformationen til klienterne.
            MessageSender.SendDataToClients(serverInfo, MessageType.ServerInfoMessage, MessagePriority.Low);
        }

        // Afslut spilrunden
        private void OnGameRoundEnd(object sender, ElapsedEventArgs e)
        {
            // Stopper timeren
            gameRoundTimer.Stop();
            // Udløser GameRoundEnded event
            GameRoundEnded?.Invoke();
            Console.WriteLine("Spilrunden er slut!");
        }

        // TODO: yderligere metoder til at håndtere spilverdenens tilstand
    }
}
