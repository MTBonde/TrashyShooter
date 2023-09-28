using System.Numerics;

using SharedData;

namespace GameServer
{
    public class GameLogicController
    {
        private GameWorldManager gameWorldManager;
        private SnapshotManager snapshotManager;
        private LagCompensationManager lagCompensationManager;
        public PlayerManager playerManager;

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

        private void SubscribeToGameWorldEvents()
        {
            // Subscribe to events
            gameWorldManager.GameRoundStarted += OnGameRoundStarted;
            gameWorldManager.GameRoundEnded += OnGameRoundEnded;
            gameWorldManager.CountdownTick += OnCountdownTick;
            gameWorldManager.CountdownStarted += OnCountdownStarted;
        }

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

        public void HandleJoin(byte playerID, string playerName)
        {
            playerManager.AddPlayer(playerID, playerName);
            playerManager.players[playerID].OnDeath += HandlePlayerDeath;

            if(playerManager.players.Count >= 1 && !gameWorldManager.GameRoundStartet)
            {
                gameWorldManager.StartGameStartCountdown();              

            }
        }

        public async Task HandlePlayerUpdate(PlayerUpdate update, byte playerID)
        {
            playerManager.UpdatePlayerState(playerID, update);
            playerManager.UpdatePlayerInfo(playerID);

            if(update.shoot)
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

        public void HandlePlayerLeft(byte playerID)
        {
            playerManager.RemovePlayer(playerID);

        }

        private void HandlePlayerDeath(byte deadPlayerID)
        {
            // TODO: denne spiller er død.
        }
    }
}

