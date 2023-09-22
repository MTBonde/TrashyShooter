using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;



using SharedData;

namespace GameServer
{
    // En struct til at holde snapshotinformation
    public struct SnapshotInfo
    {
        public PlayerSnapShot[] PrevSnap { get; set; }
        public PlayerSnapShot[] AfterSnap { get; set; }
        public DateTime PrevTime { get; set; }
        public DateTime AfterTime { get; set; }
    }
    public class SnapshotManager
    {
        // En variabel til at holde den seneste tid
        private DateTime latestSnapshotTime;

        private const int MaxSnapshots = 240;

        // Dictionary til at gemme snapshots baseret på tidspunkt
        private ConcurrentDictionary<DateTime, PlayerSnapShot[]> playerSnapshots = new ConcurrentDictionary<DateTime, PlayerSnapShot[]>();
        private ConcurrentQueue<DateTime> snapshotOrder = new ConcurrentQueue<DateTime>();

        // Metode til at tage et snapshot af spilverdenen
        public void TakeSnapshot(ConcurrentDictionary<byte, PlayerInfo> players)
        {
            DateTime currentTime = DateTime.UtcNow;
            latestSnapshotTime = currentTime;
            List<PlayerSnapShot> snapshots = new List<PlayerSnapShot>();

            foreach(KeyValuePair<byte, PlayerInfo> snapShot in players)
            {
                PlayerSnapShot newSnapshot = new PlayerSnapShot
                {
                    playerID = snapShot.Key,
                    positionX = snapShot.Value.pos.X,
                    positionY = snapShot.Value.pos.Y,
                    positionZ = snapShot.Value.pos.Z,
                    rotZ = snapShot.Value.rotZ + 90f,
                    health = snapShot.Value.health,
                    ammo = snapShot.Value.ammoInMagazine,
                    SnapSeqId = snapShot.Value.seqID
                };

                snapshots.Add(newSnapshot);
            }

            playerSnapshots.TryAdd(currentTime, snapshots.ToArray());
            snapshotOrder.Enqueue(currentTime);

            if(playerSnapshots.Count > MaxSnapshots)
            {
                DateTime oldestKey;
                if(snapshotOrder.TryDequeue(out oldestKey))
                {
                    PlayerSnapShot[] removedValue;
                    if(playerSnapshots.TryRemove(oldestKey, out removedValue))
                    {
                        // Successfully removed the oldest snapshot
                        // Log or print 'removedValue' for debugging.
                        // Console.WriteLine($"Removed snapshot for key {oldestKey}");
                    }
                }
            }
        }

        // Metode til at finde et par af snapshots baseret på en bestemt tid og spiller ID
        public async Task<(PlayerSnapShot[] prevSnap, PlayerSnapShot[] afterSnap, DateTime prev, DateTime after)> GetSnapshot(DateTime targetTime, byte playerID)
        {
            //DateTime beforeTime = new DateTime();
            DateTime beforeTime = latestSnapshotTime;
            //DateTime afterTime = new DateTime();
            DateTime afterTime = latestSnapshotTime;
            PlayerSnapShot[] beforeSnap = null;
            PlayerSnapShot[] afterSnap = null;

            //// await for at sikre at der er nået at tage et snap som kan bruges som after
            //await Task.Delay(100);
            // Vent, hvis nødvendigt, for at sikre at der er et tilgængeligt snapshot
            if(afterSnap == null)
            {
                await Task.Delay(100);  
            }

            foreach(KeyValuePair<DateTime, PlayerSnapShot[]> snapshot in playerSnapshots)
            {
                if(snapshot.Key >= targetTime)
                {
                    if(afterSnap == null || snapshot.Key < afterTime)
                    {
                        afterTime = snapshot.Key;
                        // Check if the player ID exists before calling First()
                        afterSnap = snapshot.Value;
                    }
                }
                if(beforeSnap == null || snapshot.Key < afterTime)
                {
                    beforeTime = snapshot.Key;
                    beforeSnap = snapshot.Value;
                }
            }
            // TODO: returnere en struct i stedet for en tuple
            return (beforeSnap, afterSnap, beforeTime, afterTime);
        }

        // Metode til at returnere det nyeste snapshot af spilverdenen
        public PlayerSnapShot[] GetLatestWorldStateSnapshot()
        {
            // Returner det seneste snapshot 
            return latestSnapshotTime != default ? playerSnapshots[latestSnapshotTime] : null;
        }
    }
}
