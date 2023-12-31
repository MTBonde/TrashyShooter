﻿using System.Numerics;
using SharedData;
using REST_API;
using System.Text.Json;
using System.Text;

namespace GameServer
{
    /// <summary>
    /// Klasse der håndterer spillerinformation, begivenheder og adfærd i spillet.
    /// </summary>
    public class PlayerInfo
    {
        // Events 
        public event Action OnRespawn;
        public event Action OnShoot;
        public event Action OnReload;
        public event Action OnMove;
        public event Action OnHit;
        public event Action<byte> OnDeath;
        public event Action<int> OnHealthChanged;
        public event Action<int> OnAmmoChanged;
        public event Action<int> OnPointsChanged;
        public event Action<byte> OnPlayerKill;

        // Player Attributes
        public byte id;
        public string name;
        public int points = 0;
        public DateTime lastUpdate;

        // PlayerMove
        float moveScale = 1.0f;
        public Vector3 pos = new Vector3(0, 0, 1.6f);
        public float rotX, rotY, rotZ;
        public BoxCollider playerCol;

        // Health & Ammo
        public int health = 100;
        public int ammoInMagazine = 30;
        public int maxAmmo = 30;
        public int spareAmmo = 90;
        public bool isReloading = false;

        // Sekvens-ID 
        public int seqID;

        
        public PlayerInfo(byte id)
        {
            lastUpdate = DateTime.Now;
            playerCol = new BoxCollider();
            playerCol.playerID = id;
            playerCol.position = pos;
            playerCol.offset = new Vector3(0, 0, -0.6f);
            playerCol.size = new Vector3(1, 1, 2);
            this.id = id;
            points = 0;

            OnPointsChanged += UpdateScoreboard;
        }

        // Metoder til at udløse events
        public void InvokeOnRespawn() => OnRespawn?.Invoke();
        public void InvokeOnHealthChanged() => OnHealthChanged?.Invoke(health);
        public void InvokeOnAmmoChanged() => OnAmmoChanged?.Invoke(ammoInMagazine);

        /// <summary>
        /// Flytter spilleren baseret på klient update
        /// </summary>
        /// <param name="up">Bevæger op, hvis sand.</param>
        /// <param name="down">Bevæger ned, hvis sand.</param>
        /// <param name="left">Bevæger til venstre, hvis sand.</param>
        /// <param name="right">Bevæger til højre, hvis sand.</param>
        /// <param name="jump">Udfører hop, hvis sand.</param>
        /// <param name="newRotY">Ny Y-rotation.</param>
        /// <param name="newRotZ">Ny Z-rotation.</param>
        /// <param name="seqID">Sekvens-ID for bevægelsen.</param>
        public void Move(bool up, bool down, bool left, bool right, bool jump, float newRotY, float newRotZ, int seqID)
        {
            float elapsed = (float)(lastUpdate - DateTime.Now).TotalSeconds;
            lastUpdate = DateTime.Now;
            Vector3 movement = pos;
            Vector3 facing = new Vector3(
                -MathF.Sin((float)(Math.PI / 180) * (rotZ + 90)),
                MathF.Cos((float)(Math.PI / 180) * (rotZ + 90)),
                0f
            );
            Vector3 sideVector = new Vector3(
                -MathF.Sin((float)(Math.PI / 180) * (rotZ)),
                MathF.Cos((float)(Math.PI / 180) * (rotZ)),
                0f
            );
            //moves player based on keyboard inputs
            if (up)
                movement -= facing * moveScale * elapsed;
            if(down)
                movement += facing * moveScale * elapsed;
            if(right)
                movement += sideVector * moveScale * elapsed;
            if(left)
                movement -= sideVector * moveScale * elapsed;
            pos = movement;
            playerCol.position = movement;
            rotY = newRotY;
            rotZ = newRotZ;
            this.seqID = seqID;
        }

        /// <summary>
        /// Udfører et skud, hvis der er ammunition tilgængelig.
        /// </summary>
        public void Shoot()
        {
            // Tjek ammunition
            if(ammoInMagazine <= 0)
            {
                // TODO: Ingen ammunition. click click sound??
                Console.WriteLine("Click click, no ammo!");
            }
            else
            {
                float radianRotY = rotY * (MathF.PI / 180.0f);
                Vector3 shootingDirection = new Vector3(MathF.Sin(radianRotY), 0, MathF.Cos(radianRotY));
                // Reducer ammunition
                ammoInMagazine--;
                OnAmmoChanged?.Invoke(ammoInMagazine);
            }
        }

        /// <summary>
        /// Reload
        /// </summary>
        public void Reload()
        {
            // Tjek om spilleren allerede er i gang med at genoplade
            if(isReloading)
                return;

            // Tjek om der er behov for at reload
            if(ammoInMagazine == maxAmmo)
                return;

            // Tjek om der er ekstra ammunition til rådighed
            if(spareAmmo <= 0)
                return;

            isReloading = true;

            // TODO: RELOAD SOUND???
            // fake reload tid
            Task.Delay(2000).ContinueWith(_ =>
            {
                // Beregn hvor meget ammunition der er brug for
                int ammoNeeded = maxAmmo - ammoInMagazine;

                // Beregn hvor meget ammunition der faktisk er til rådighed
                int ammoToReload = Math.Min(ammoNeeded, spareAmmo);

                // Opdater ammunitionstilstand
                ammoInMagazine += ammoToReload;
                spareAmmo -= ammoToReload;

                OnAmmoChanged?.Invoke(ammoInMagazine);

                isReloading = false;
            });
        }

        /// <summary>
        /// Giver skade til spilleren og udløser død, hvis helbredet er 0 eller lavere.
        /// </summary>
        /// <param name="damage">Antal skadepoint.</param>
        /// <param name="shootersId">ID på spilleren, der skød.</param>
        public void TakeDamage(int damage, byte shootersId)
        {
            health -= damage;
            if(health <= 0)
            {
                points--;  
                OnPointsChanged?.Invoke(points);  
                OnDeath?.Invoke(id);
                // Todo: giv point til id
                OnPlayerKill?.Invoke(shootersId);
            }
            OnHealthChanged.Invoke(health);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void OnKill()
        {
            points++;  
            OnPointsChanged?.Invoke(points);  
        }

        /// <summary>
        /// Genererer en opdateringspakke med spillerens aktuelle status.
        /// </summary>
        /// <returns>En PlayerInfoUpdate instans med spillerens aktuelle status.</returns>
        public PlayerInfoUpdate CreatePlayerInfoUpdate()
        {
            PlayerInfoUpdate update = new PlayerInfoUpdate
            {
                health = this.health,
                ammo = this.ammoInMagazine,
                points = this.points
            };
            return update;
        }

        /// <summary>
        /// Asynkront opdaterer pointtavlen ved at sende data til en REST API.
        /// </summary>
        /// <param name="newScore">Den nye score for spilleren.</param>
        public async void UpdateScoreboard(int newScore)
        {
            ScoreboardModel score = new ScoreboardModel
            {
                score = newScore
            };
            string json = JsonSerializer.Serialize(score);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await RestManager.GetRestClient().PutAsync(id.ToString(), content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("error on updating data to rest api\n" + ex.ToString());
                return;
            }
        }
    }
}
