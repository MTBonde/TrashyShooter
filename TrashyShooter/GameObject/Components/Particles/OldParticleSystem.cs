using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

namespace MultiplayerEngine
{
    public class OldParticleSystem
    {
        public List<OldParticle> Particles { get; private set; } = new List<OldParticle>();
        public Model ParticleModel { get; set; }

        public void Update(float deltaTime)
        {
            // Opdater partikler
            for(int i = Particles.Count - 1; i >= 0; i--)
            {
                Particles[i].Update(deltaTime);
                if(Particles[i].LifeTime <= 0)
                {
                    Particles.RemoveAt(i);
                }
            }
        }

        public void Draw(Matrix view, Matrix projection)
        {
            foreach(var particle in Particles)
            {
                particle.Draw(view, projection);
            }
        }

        

        public void GenerateParticles(Vector3 position, Vector3 velocity, int count)
        {
            // Generer partikler
            for(int i = 0; i < count; i++)
            {
                Particles.Add(new OldParticle(ParticleModel, position, velocity));
            }
        }

        public void GenerateParticles(Vector3 position, Vector3 baseVelocity, int count, float maxAngle)
        {
            for(int i = 0; i < count; i++)
            {
                // Generer en tilfældig vinkel
                float angle = (float)(Globals.Rnd.NextDouble() * 2.0 - 1.0) * maxAngle;
                // Roter basis hastighedsvektor
                Vector3 velocity = Vector3.Transform(baseVelocity, Matrix.CreateRotationZ(MathHelper.ToRadians(angle)));
                // Tilføj partikel
                Particles.Add(new OldParticle(ParticleModel, position, velocity));
            }
        }
    }
}
