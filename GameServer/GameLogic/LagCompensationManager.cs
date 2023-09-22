using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using SharedData;

namespace GameServer
{
    // LagCompensationManager 
    public class LagCompensationManager
    {
        private readonly SnapshotManager snapshotManager;
        private readonly object lockObject = new object();

        // Constructor-baseret Dependency Injection af SnapshotManager
        public LagCompensationManager(SnapshotManager snapshotManager)
        {
            this.snapshotManager = snapshotManager;
        }

        // Metode til for lag-kompenseret skud
        public async Task PerformLagCompensatedShoot(DateTime timeOfShot, PlayerInfo playersCurrentInfo, PlayerUpdate update, byte id, ConcurrentDictionary<byte, PlayerInfo> players)
        {
            // Trin 1: Find nærmeste snapshots ved hjælp af SnapshotManager
            (PlayerSnapShot[] prevSnap, PlayerSnapShot[] afterSnap, DateTime prev, DateTime after) = await snapshotManager.GetSnapshot(timeOfShot, id);

            // Trin 2: Interpolér før og efter snap for at finde midtpunkt
            PlayerSnapShot[] interpolatedState = EntityInterpolate(prevSnap, afterSnap, prev, after, timeOfShot);

            // Trin 3: Brug interpoleret midtpunkt for kollisionstjek
            Vector3 lDirection = CalculateDirection(update.rotZ);

            // Trin 4: Udfør skuddet
            Vector3 interpolatedPos = new Vector3(playersCurrentInfo.pos.X, playersCurrentInfo.pos.Y, playersCurrentInfo.pos.Z);

            // Opret Collider til den interpolerede tilstand
            List<Collider> pastColliders = CollisionManager.colliders.Select(c => (Collider)c.Clone()).ToList();

            lock(lockObject)
            {
                // Opdaterer pastColliders og interpolatedState
                for(int i = 0; i < pastColliders.Count; i++)
                {
                    pastColliders[i].position = new Vector3(interpolatedState[i].positionX, interpolatedState[i].positionY, interpolatedState[i].positionZ);
                }
            }

            // Udfør raycast og få CollisionInfo tilbage
            int playerColNumber = CollisionManager.colliders.IndexOf(playersCurrentInfo.playerCol);
            CollisionInfo colInfo = CollisionManager.RayCast(interpolatedPos, interpolatedPos + (lDirection * 10), pastColliders[playerColNumber], pastColliders);

            LaserShot laserShot = new LaserShot();
            laserShot.posX = playersCurrentInfo.pos.X;
            laserShot.posY = playersCurrentInfo.pos.Y;
            laserShot.posZ = playersCurrentInfo.pos.Z;
            laserShot.rotX = 0;
            laserShot.rotZ = update.rotZ;
            laserShot.rotY = update.rotY;
            laserShot.length = Vector3.Distance(playersCurrentInfo.pos, colInfo.collisionPoint);
            // TODO MESSAGESENDER await Server.SendDataToClientsExceptOne(laserShot, playersCurrentInfo.id);

            // Guard clause: Hvis vi ikke ramte noget, opdater ammo og send "missed" besked
            if(colInfo.playerID == null)
            {
                SendShootConfirm(update.SnapSeqId, false, colInfo.playerID);
                return;
            }
            // Hvis vi er her, betyder det, at vi har ramt noget
            byte hitPlayerID = colInfo.playerID.Value; // Hent den ramte spillers ID
            players[hitPlayerID].TakeDamage(10, id); // Reducer liv            

            // Send shoot besked, og bekræft ramt
            SendShootConfirm(update.SnapSeqId, true, colInfo.playerID);

            // Reduce ammo for the shooting player in all cases
            players[id].ammoInMagazine -= 1;
        }

        // Andre metoder og hjælpefunktioner for lag kompensation

        // Metode til at sende shootConfirm besked
        public void SendShootConfirm(int snapSeqId, bool hit, byte? playerID)
        {
            // Kører den ene eller den anden WriteLine metode baseret på betingelsen 'hit'
            if(hit)
            {
                Console.WriteLine("!!You Shoot Player " + playerID + "!!");
            }
            else
            {
                Console.WriteLine("!!You missed .. !!");
            }

        }

        public PlayerSnapShot[] EntityInterpolate(PlayerSnapShot[] beforeSnap, PlayerSnapShot[] afterSnap, DateTime prev, DateTime after, DateTime timeOfShot)
        {
            // Trin 1: Beregn interpolationsfaktor
            double totalTicks = (after - prev).Ticks;
            double targetTicks = (timeOfShot - prev).Ticks;
            // Check if totalTicks is zero before performing the division. if 0 use 1
            float factor = (totalTicks == 0) ? 1 : (float)(targetTicks / totalTicks);


            // Trin 2: Interpolér hver egenskab på entiteten fra snapshot, for at have det fulde billede af tilstanden
            PlayerSnapShot[] interpolatedState = new PlayerSnapShot[beforeSnap.Length];

            //Trin 3: loop igennem array og interpolate alle players
            for(int i = 0; i < beforeSnap.Length; i++)
            {
                //makes interpolatedState[i] not null
                interpolatedState[i] = new PlayerSnapShot();

                // For position
                interpolatedState[i].positionX = beforeSnap[i].positionX + (afterSnap[i].positionX - beforeSnap[i].positionX) * factor;
                interpolatedState[i].positionY = beforeSnap[i].positionY + (afterSnap[i].positionY - beforeSnap[i].positionY) * factor;
                interpolatedState[i].positionZ = beforeSnap[i].positionZ + (afterSnap[i].positionZ - beforeSnap[i].positionZ) * factor;

                // For rotationZ 
                interpolatedState[i].rotZ = beforeSnap[i].rotZ + (afterSnap[i].rotZ - beforeSnap[i].rotZ) * factor;

                // For Health
                interpolatedState[i].health = (int)(beforeSnap[i].health + (afterSnap[i].health - beforeSnap[i].health) * factor);

                // For Ammo
                interpolatedState[i].ammo = (int)(beforeSnap[i].ammo + (afterSnap[i].ammo - beforeSnap[i].ammo) * factor);
            }

            return interpolatedState;
        }

        // Metode til at beregne retning baseret på rotationsvinkel
        public Vector3 CalculateDirection(float rotZ)
        {
            // Tilføj 90 grader til at justere for spil-specifik orientering
            float angle = rotZ + 90;

            // Konverter vinklen til en retning i koordinater
            Vector3 lDirection = new Vector3(
                -MathF.Sin((float)(Math.PI / 180) * angle),
                MathF.Cos((float)(Math.PI / 180) * angle),
                0f
            );

            // Normaliser vektoren for at sikre, at dens længde er 1
            return Vector3.Normalize(lDirection);
        }
    }
}
