﻿using System.Collections.Concurrent;
using System.Numerics;
using System.Text;
using System.Text.Json;
using REST_API;
using SharedData;

namespace GameServer
{
    /// <summary>
    /// Administrerer spillere
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
        public async void AddPlayer(byte id, string name)
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

        public async Task RESTPost(byte id, string name)
        {
            ScoreboardModel score = new ScoreboardModel
            {
                ID = id,
                playerName = name,
                score = 0
            };
            string json = JsonSerializer.Serialize(score);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await RestManager.GetRestClient().PostAsync("PostScore", content);
                Console.WriteLine("added data to rest: " + response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("error on sending data to rest api\n" + ex.ToString());
                return;
            }
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

        /// <summary>
        /// Generel metode til at opdatere en spillers stats
        /// </summary>
        /// <param name="player"></param>
        /// <param name="id"></param>
        private void UpdatePlayerStat(PlayerInfo player, byte id)
        {
            // Opret opdateringsmeddelelse og udløs event
            PlayerInfoUpdate updateMessage = player.CreatePlayerInfoUpdate();
            OnPlayerStatChanged?.Invoke(updateMessage, id);
        }

        /// <summary>
        /// Håndterer en spillers død
        /// </summary>
        /// <param name="id"></param>
        private void HandlePlayerDeath(byte id)
        {
            RespawnPlayer(id);
        }

        /// <summary>
        /// Håndterer, når en spiller dræber en anden spiller
        /// </summary>
        /// <param name="id"></param>
        private void HandlePlayerKill(byte id)
        {
            if(players.TryGetValue(id, out PlayerInfo specificPlayer))
            {
                specificPlayer.OnKill();
            }
        }

        public void RespawnPlayer(byte id)
        {
            RespawnPlayer(id, spawnPoint);
        }
        /// <summary>
        /// Respawner en spiller ved spawn-punktet.
        /// </summary>
        /// <param name="id">Spillerens unikke ID.</param>
        public void RespawnPlayer(byte id, Vector3 pos)
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

        /// <summary>
        /// Nultil en spiller
        /// </summary>
        /// <param name="id"></param>
        public void ResetPlayer(byte id)
        {
            PlayerInfo player = players[id];
            player.pos = spawnPoint;
            player.health = 100;
            player.ammoInMagazine = player.maxAmmo;
            player.isReloading = false;
            player.playerCol.position = player.pos;
            player.InvokeOnHealthChanged();
            player.InvokeOnAmmoChanged();
        }
    }
}
