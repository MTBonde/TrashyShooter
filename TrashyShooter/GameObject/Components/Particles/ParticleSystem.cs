using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace MultiplayerEngine
{
    public class ParticleSystem : Component
    {

        public Queue<Particle> particles;
        public Vector3 minVelocety, maxVelocety;
        public bool worldSpace = false;
        public float lifeTime = 1;
        public float spawnRate = 10;
        public SpawnInfo spawnInfo = new PointSpawn();

        List<Particle> activeParticles = new List<Particle>();
        Model particelModel;
        readonly int maxParticles = 500;
        float spawnTimer;
        bool active;

        public void SetModel(string model)
        {
            particelModel = GameWorld.Instance.Content.Load<Model>(model);
        }

        public void Awake()
        {
            SetModel("Particle");

            particles = new Queue<Particle>(maxParticles);
            for (int i = 0; i < maxParticles; i++)
            {
                Particle particle = new Particle();
                particle.pos = Vector3.Zero;
                particle.velocety = Vector3.Zero;
                particle.livedTime = 0;
                
                particles.Enqueue(particle);
            }
        }

        public void StartSystem()
        {
            active = true;
        }

        public void StopSystem()
        {
            active = false;
        }

        void Update()
        {
            if (active)
            {
                spawnTimer += Globals.DeltaTime;
                if (spawnTimer > 1/spawnRate)
                {
                    SpawnParticles(1);
                    spawnTimer -= 1 / spawnRate;
                }
            }
            for (int i = 0; i < activeParticles.Count; i++)
            {
                activeParticles[i].Update();
                if (activeParticles[i].livedTime >= lifeTime)
                {
                    DeSpawnParticle(activeParticles[i]);
                    i--;
                }
            }
        }

        public void SpawnParticles(int amount)
        {
            if (particles.Count == 0)
                return;

            Particle particle = particles.Dequeue();
            particle.velocety = new Vector3
                (
                Globals.Rnd.NextFloat(minVelocety.X, maxVelocety.X),
                Globals.Rnd.NextFloat(minVelocety.Y, maxVelocety.Y),
                Globals.Rnd.NextFloat(minVelocety.Z, maxVelocety.Z)
                );
            if(worldSpace)
                particle.pos = transform.Position3D;
            switch (spawnInfo.spawnType)
            {
                case SpawnType.Line:
                    particle.pos += (((LineSpawn)spawnInfo).direction * Globals.Rnd.NextFloat(0, ((LineSpawn)spawnInfo).length));
                    particle.velocety = new Vector3(
                        Globals.Rnd.NextFloat(0,1),
                        Globals.Rnd.NextFloat(0, 1),
                        Globals.Rnd.NextFloat(0, 1));
                    break;
                case SpawnType.Point:
                    particle.pos += (((PointSpawn)spawnInfo).offset);
                    particle.velocety = new Vector3(
                        Globals.Rnd.NextFloat(0, 1),
                        Globals.Rnd.NextFloat(0, 1),
                        Globals.Rnd.NextFloat(0, 1));
                    break;
            }

            activeParticles.Add(particle);
        }

        void DeSpawnParticle(Particle particleToDespawn)
        {
            activeParticles.Remove(particleToDespawn);

            particleToDespawn.velocety = Vector3.Zero;
            particleToDespawn.pos = Vector3.Zero;
            particleToDespawn.livedTime = 0;

            particles.Enqueue(particleToDespawn);
        }

        void Draw3D()
        {
            foreach (Particle particle in activeParticles)
            {
                foreach (ModelMesh mesh in particelModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        CameraManager.ApplyWorldShading(effect);

                        effect.View = SceneManager.active_scene.viewMatrix;
                        if(worldSpace)
                            effect.World = SceneManager.active_scene.worldMatrix *
                                Matrix.CreateScale(lifeTime - particle.livedTime) *
                                Matrix.CreateRotationX(MathHelper.ToRadians(transform.Rotation.X)) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(transform.Rotation.Y)) *
                                Matrix.CreateRotationZ(MathHelper.ToRadians(transform.Rotation.Z)) *
                                Matrix.CreateTranslation(particle.pos);
                        else
                            effect.World = SceneManager.active_scene.worldMatrix *
                                Matrix.CreateScale(lifeTime - particle.livedTime) *
                                Matrix.CreateRotationX(MathHelper.ToRadians(transform.Rotation.X)) *
                                Matrix.CreateRotationY(MathHelper.ToRadians(transform.Rotation.Y)) *
                                Matrix.CreateRotationZ(MathHelper.ToRadians(transform.Rotation.Z)) *
                                Matrix.CreateTranslation(transform.Position3D + particle.pos);
                        effect.Projection = SceneManager.active_scene.projectionMatrix;
                        mesh.Draw();
                        //Debug.WriteLine(transform.Position3D);
                    }
                }
            }
        }

        public class Particle
        {
            public Vector3 pos, velocety;
            public float livedTime;

            public void Update()
            {
                pos += velocety * Globals.DeltaTime;
                livedTime += Globals.DeltaTime;
            }
        }

        public enum SpawnType
        {
            Line,
            Point
        }

        public abstract class SpawnInfo
        {
            public abstract SpawnType spawnType { get; }
        }

        public class LineSpawn : SpawnInfo
        {
            public override SpawnType spawnType => SpawnType.Line;
            public Vector3 direction = Vector3.Zero;
            public float length = 0;
        }

        public class PointSpawn : SpawnInfo
        {
            public override SpawnType spawnType => SpawnType.Point;
            public Vector3 offset = Vector3.Zero;
        }
    }
}
