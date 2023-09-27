using System;
using System.Timers;

namespace GameServer
{
    public class GameWorldManager
    {
        // Timer til at styre spilrunden
        private System.Timers.Timer gameRoundTimer;
        // Timer for 5sek countdown
        private System.Timers.Timer countdownTimer;

        // Definer events
        public event Action GameRoundStarted;
        public event Action GameRoundEnded;
        public event Action<int> CountdownTick;
        public event Action<int> CountdownStarted;

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

            if(remainingTime <= 0)
            {
                countdownTimer.Stop();
                StartNewGameRound();
            }
        }

        // Start en ny spilrunde
        public void StartNewGameRound()
        {
            // Starter timeren
            gameRoundTimer.Start();
            // Udløser GameRoundStarted event
            GameRoundStarted?.Invoke();
            Console.WriteLine("Spilrunden er startet!");
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
