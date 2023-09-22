using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;



using SharedData;

namespace GameServer
{
    /// <summary>
    /// Administrerer spillere i et multiplayer-spil.
    /// </summary>
    public class PlayerManager
    {
        /// <summary>
        /// Udløses, når en spillers tilstand ændres.
        /// </summary>
        public event Action<PlayerInfoUpdate, byte> OnPlayerStatChanged;

        // Trådsikker dictionary til at holde spillere og deres ID'er.
        public ConcurrentDictionary<byte, PlayerInfo> players = new ConcurrentDictionary<byte, PlayerInfo>(); //TODO: lav private og giv GC adgang gennem metoder

        // Standard spawn-punkt for spillere.
        private Vector3 spawnPoint = new Vector3(0, 0, 1.6f);

        /// <summary>
        /// Tilføjer en ny spiller til spillet.
        /// </summary>
        /// <param name="id">Spillerens unikke ID.</param>
        /// <param name="name">Spillerens navn.</param>
        public void AddPlayer(byte id, string name)
        {
            // Opret ny spiller
            PlayerInfo newPlayer = new PlayerInfo(id) { name = name };

            // Tilmeld event-handlere
            newPlayer.OnHealthChanged += (newHealth) => UpdatePlayerStat(newPlayer, id);
            newPlayer.OnAmmoChanged += (newAmmo) => UpdatePlayerStat(newPlayer, id);
            newPlayer.OnPointsChanged += (newPoints) => UpdatePlayerStat(newPlayer, id);

            newPlayer.OnDeath += HandlePlayerDeath;
            newPlayer.OnPlayerKill += HandlePlayerKill;

            // Tilføj spiller til dictionary
            players.TryAdd(id, newPlayer);
        }

        /// <summary>
        /// Fjerner en spiller fra spillet.
        /// </summary>
        /// <param name="id">Spillerens unikke ID.</param>
        public void RemovePlayer(byte id)
        {
            // Fjern spiller, hvis den findes
            if(!players.TryRemove(id, out _))
            {
                // TODO: Log fejl
            }
        }

        /// <summary>
        /// Opdaterer en spillers tilstand baseret på input.
        /// </summary>
        /// <param name="id">Spillerens unikke ID.</param>
        /// <param name="update">Objekt med opdateringsdata.</param>
        public void UpdatePlayerState(byte id, PlayerUpdate update)
        {
            // Flyt spiller baseret på input
            players[id].Move(update.up, update.down, update.left, update.right, update.jump, update.rotY, update.rotZ, update.SnapSeqId);
        }

        /// <summary>
        /// Opdaterer info for en specifik spiller.
        /// </summary>
        /// <param name="playerId">Spillerens unikke ID.</param>
        public void UpdatePlayerInfo(byte playerId)
        {
            // Guard clause: exit hvis spilleren ikke findes
            if(!players.TryGetValue(playerId, out PlayerInfo specificPlayer))
            {
                // TODO: Log fejl
                return;
            }
            // Opdater spillerens status
            UpdatePlayerStat(specificPlayer, playerId);
        }

        // Generel metode til at opdatere en spillers stats
        private void UpdatePlayerStat(PlayerInfo player, byte id)
        {
            // Opret opdateringsmeddelelse og udløs event
            PlayerInfoUpdate updateMessage = player.CreatePlayerInfoUpdate();
            OnPlayerStatChanged?.Invoke(updateMessage, id);
        }

        // Håndterer en spillers død
        private void HandlePlayerDeath(byte id)
        {
            RespawnPlayer(id);
        }

        // Håndterer, når en spiller dræber en anden spiller
        private void HandlePlayerKill(byte id)
        {
            if(players.TryGetValue(id, out PlayerInfo specificPlayer))
            {
                specificPlayer.OnKill();
            }
        }

        /// <summary>
        /// Respawner en spiller ved spawn-punktet.
        /// </summary>
        /// <param name="id">Spillerens unikke ID.</param>
        public void RespawnPlayer(byte id)
        {
            // Find spiller og nulstil deres tilstand
            PlayerInfo player = players[id];
            player.pos = spawnPoint;
            player.health = 100;
            player.ammoInMagazine = player.maxAmmo;
            player.isReloading = false;
            player.playerCol.position = player.pos;

            // Udløs relevante events
            player.InvokeOnRespawn();
            player.InvokeOnHealthChanged();
            player.InvokeOnAmmoChanged();
        }
    }
}
