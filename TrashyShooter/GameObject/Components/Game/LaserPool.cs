using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace MultiplayerEngine
{
    public class LaserPool : Component
    {
        // Kø til at holde tilgængelige kugle GameObjects
        private Queue<GameObject> availableLasers;

        // Størrelse af objektpoolen
        private int poolSize;

        // Konstruktør til at initialisere objektpoolen
        public LaserPool()
        {
            // Angiv poolens startstørrelse
            poolSize = 1000;

            // Initialiser køen med poolstørrelsen
            availableLasers = new Queue<GameObject>(poolSize);

            // Initialiser puljen med deaktiverede kugle GameObjects
            for(int i = 0; i < poolSize; i++)
            {
                // Opret et nyt kugle GameObject
                GameObject laser = new GameObject();

                // Tilføj en BulletComponent til GameObject
                LaserComponent laserComp = laser.AddComponent<LaserComponent>();
                laserComp.Setup(this);
                laserComp.linePS = laser.AddComponent<ParticleSystem>();
                laserComp.linePS.Awake();
                laserComp.linePS.worldSpace = true;
                laserComp.linePS.lifeTime = 0.5f;
                laserComp.linePS.spawnRate = 250;
                laserComp.linePS.spawnInfo = new ParticleSystem.LineSpawn();

                laserComp.startPS = laser.AddComponent<ParticleSystem>();
                laserComp.startPS.Awake();
                laserComp.startPS.worldSpace = true;
                laserComp.startPS.lifeTime = 0.5f;
                laserComp.startPS.spawnRate = 250;
                laserComp.stopPS = laser.AddComponent<ParticleSystem>();
                laserComp.stopPS.Awake();
                laserComp.stopPS.worldSpace = true;
                laserComp.stopPS.lifeTime = 0.5f;
                laserComp.stopPS.spawnRate = 250;

                // Deaktiver/stopper GameObjects components
                laserComp.enabled = false;
                laserComp.linePS.StopSystem();
                laserComp.startPS.StopSystem();
                laserComp.stopPS.StopSystem();

                // Tilføj GameObject til puljen
                availableLasers.Enqueue(laser);
            }
        }

        public void OnLaserShotReceived(NetworkMessage message)
        {
            LaserShot laserShotData = (LaserShot)message;
            GameObject laser = GetLaser();
            LaserComponent laserComp = laser.GetComponent<LaserComponent>();
            laser.transform.Position3D = new Vector3(laserShotData.posX, laserShotData.posY, laserShotData.posZ);
            laser.transform.Rotation = new Vector3(laserShotData.rotX, laserShotData.rotY, laserShotData.rotZ);
            laserComp.FireLaser(laser.transform.Rotation, laserShotData.length);
            laserComp.linePS.StartSystem();
            //laser.GetComponent<ParticleSystem>().Awake();
        }

        // Metode til at hente et tilgængeligt kugle GameObject fra puljen
        public GameObject GetLaser()
        {
            // TODO: måske udvide puljens størrelse her
            if(availableLasers.Count == 0)
            {

            }

            // Hent et GameObject fra puljen
            GameObject laser = availableLasers.Dequeue();

            // Aktiver GameObject
            laser.GetComponent<LaserComponent>().enabled = true;
            laser.GetComponent<LaserComponent>().linePS.StartSystem();
            laser.GetComponent<LaserComponent>().startPS.StartSystem();
            laser.GetComponent<LaserComponent>().stopPS.StartSystem();

            // Returner det aktiverede GameObject
            return laser;
        }

        // Metode til at returnere et brugt kugle GameObject tilbage til puljen og rense det
        public void ReturnLaser(GameObject laser)
        {
            // Hent og nulstil PositionComponent
            laser.transform.Position3D = Vector3.Zero;

            // Deaktiver kuglen
            laser.GetComponent<LaserComponent>().enabled = false;
            laser.GetComponent<LaserComponent>().linePS.StopSystem();
            laser.GetComponent<LaserComponent>().startPS.StopSystem();
            laser.GetComponent<LaserComponent>().stopPS.StopSystem();

            // Returner kuglen til puljen
            availableLasers.Enqueue(laser);
        }
    }
}
