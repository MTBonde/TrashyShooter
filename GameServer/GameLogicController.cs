using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedData;

namespace GameServer
{
    public class GameController
    {
        private GameWorldManager world;
        private SnapshotManager snapshotManager;
        private LagCompensationManager lagCompensationManager;
        private PlayerManager playerManager;

        public GameController(SnapshotManager snapshotManager,
                              LagCompensationManager lagCompensationManager,
                              PlayerManager playerManager)
        {
            this.snapshotManager = snapshotManager;
            this.lagCompensationManager = lagCompensationManager;
            this.playerManager = playerManager;
            world = new GameWorldManager();
        }

        public void HandleJoin(byte playerID, string playerName)
        {
            playerManager.AddPlayer(playerID, playerName);
            playerManager.players[playerID].OnDeath += HandlePlayerDeath;

        }

        public async Task HandlePlayerUpdate(PlayerUpdate update, byte playerID)
        {
            playerManager.UpdatePlayerState(playerID, update);
            playerManager.UpdatePlayerInfo(playerID);

            if(update.shoot)
            {
                //TODO: send ping from client and use in sytract for delay
                // Perform lag-compensated shooting logic
                await lagCompensationManager.PerformLagCompensatedShoot(DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(0)),
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

