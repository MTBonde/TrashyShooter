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
            gameRoundTimer.AutoReset = false;

            // 1 second in milliseconds
            countdownTimer = new System.Timers.Timer(1000); 
            countdownTimer.AutoReset = true;
        }

        // Start 5-second countdown for game start
        public void StartGameStartCountdown()
        {
            countdownTimer.Elapsed += OnGameStartCountdownTick;
            StartCountdown();

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
            countdownTimer.Elapsed -= OnGameStartCountdownTick;
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
            // Denne linje beregner den resterende tid i sekunder for nedtællingen
            // ved at bruge modulo-operationen på den nuværende sekund i systemets tid. 
            int remainingTime = (int)(5 - (DateTime.Now.Second % 5));
            CountdownTick?.Invoke(remainingTime);

            ServerInfoMessage serverInfo = new ServerInfoMessage()
            {
                ServerInformation = $"M:Game Round start in {remainingTime}"
            };
            
            MessageSender.SendDataToClients(serverInfo, MessageType.ServerInfoMessage, MessagePriority.Low);

            if(remainingTime <= 0)
            {
                countdownTimer.Stop();
                StartNewGameRound();
            }
        }

        // Start en ny spilrunde
        public void StartNewGameRound()
        {
            // Nulstiller forløbet tid
            elapsedTimeInSeconds = 0;

            // Angiver at en spilrunde er startet.
            GameRoundStartet = true;

            // Starter nedtællingstimeren for spilrunden.
            gameRoundTimer.Start();

            // Tilmelder en eventhandler til timerens Elapsed-event
            gameRoundTimer.Elapsed += OnTimedEvent;

            // Udløser eventet for start af spilrunde.
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
