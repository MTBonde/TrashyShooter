using System.Numerics;

using SharedData;

namespace GameServer
{
    /// <summary>
    /// Kontrollerer spillets logik, inklusive styring af spillets verden, spillere og andre managers.
    /// </summary>
    public class GameLogicController
    {
        private GameWorldManager gameWorldManager;
        private SnapshotManager snapshotManager;
        private LagCompensationManager lagCompensationManager;
        public PlayerManager playerManager;

        /// <summary>
        /// Initialiserer en ny instans af GameLogicController klassen.
        /// </summary>
        /// <param name="snapshotManager">Reference til SnapshotManager.</param>
        /// <param name="lagCompensationManager">Reference til LagCompensationManager.</param>
        /// <param name="playerManager">Reference til PlayerManager.</param>
        public GameLogicController(SnapshotManager snapshotManager,
                              LagCompensationManager lagCompensationManager,
                              PlayerManager playerManager)
        {
            this.snapshotManager = snapshotManager;
            this.lagCompensationManager = lagCompensationManager;
            this.playerManager = playerManager;

            // Initialize GameWorldManager and subscribe to its events
            gameWorldManager = new GameWorldManager();
            SubscribeToGameWorldEvents();
        }

        /// <summary>
        /// Abonnerer på relevante begivenheder fra GameWorldManager.
        /// </summary>
        private void SubscribeToGameWorldEvents()
        {
            // Subscribe to events
            gameWorldManager.GameRoundStarted += OnGameRoundStarted;
            gameWorldManager.GameRoundEnded += OnGameRoundEnded;
            gameWorldManager.CountdownTick += OnCountdownTick;
            gameWorldManager.CountdownStarted += OnCountdownStarted;
        }

        /// <summary>
        /// Udføres når en spilrunde slutter. Rydder op og forbereder til næste runde.
        /// </summary>
        private void OnGameRoundStarted()
        {
            // Initialize Random class
            Random random = new Random();

            // Generate random coordinates within the range x = [-10, 10] and y = [-10, 10]
            int randomX = random.Next(-10, 11);
            int randomY = random.Next(-10, 11);

            // Convert integers to Vector3
            Vector3 respawnPosition = new Vector3(randomX, randomY, 0);

            // 
            for(int i = 0; i < playerManager.players.Count; i++)
            {
                byte playerId = playerManager.players[(byte)i].id;

                // Use the generated coordinates to respawn the player
                playerManager.ResetPlayer(playerId);
                playerManager.RespawnPlayer(playerId, respawnPosition);
            }
        }



        private void OnGameRoundEnded()
        {

        }

        private void OnCountdownTick(int remainingTime)
        {

        }

        private void OnCountdownStarted(int countdown)
        {

        }

        /// <summary>
        /// Håndterer en spillers tilslutning til spillet.
        /// </summary>
        /// <param name="playerID">Spillerens unikke ID.</param>
        /// <param name="playerName">Spillerens navn.</param>
        public void HandleJoin(byte playerID, string playerName)
        {
            playerManager.AddPlayer(playerID, playerName);
            playerManager.players[playerID].OnDeath += HandlePlayerDeath;
            playerManager.RESTPost(playerID, playerName);
        

            if(playerManager.players.Count >= 1 && !gameWorldManager.GameRoundStartet)
            {
                gameWorldManager.StartGameStartCountdown();              

            }
        }

        /// <summary>
        /// Asynkront opdaterer en spillers tilstand baseret på indkommende opdateringer.
        /// </summary>
        /// <param name="update">Den modtagne spilleropdatering.</param>
        /// <param name="playerID">Spillerens unikke ID.</param>
        public async Task HandlePlayerUpdate(PlayerUpdate update, byte playerID)
        {
            playerManager.UpdatePlayerState(playerID, update);
            playerManager.UpdatePlayerInfo(playerID);

            if(update.shoot && playerManager.players[playerID].ammoInMagazine > 0)
            {
                //TODO: send ping from client and use in sytract for delay
                // Perform lag-compensated shooting logic
                lagCompensationManager.PerformLagCompensatedShoot(DateTime.UtcNow,
                                                                  playerManager.players[playerID],
                                                                  update,
                                                                  playerID,
                                                                  playerManager.players);
                playerManager.players[playerID].Shoot();
            }
            if(update.reload)
            {
                playerManager.players[playerID].Reload();
            }

            // Take a snapshot after handling the player update
            snapshotManager.TakeSnapshot(playerManager.players);
        }

        /// <summary>
        /// Håndterer en spillers frakobling fra spillet.
        /// </summary>
        /// <param name="playerID">Spillerens unikke ID.</param>
        public void HandlePlayerLeft(byte playerID)
        {
            playerManager.RemovePlayer(playerID);

        }

        /// <summary>
        /// Håndterer en spillers død.
        /// </summary>
        /// <param name="deadPlayerID">Den døde spillers unikke ID.</param>
        private void HandlePlayerDeath(byte deadPlayerID)
        {
            // TODO: denne spiller er død.
        }
    }
}

