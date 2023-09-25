using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerEngine
{
    public class OldParticle
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float LifeTime { get; set; }
        public Texture2D Texture { get; set; }
        public Model Model { get; set; }

        public OldParticle(Model model, Vector3 position, Vector3 velocity)
        {
            Model = model;
            Position = position;
            Velocity = velocity;
            LifeTime = 1.0f;  // 1 second
        }

        public void Update(float deltaTime)
        {
            // Opdater position
            Position += Velocity * deltaTime;
            // Reducer levetid
            LifeTime -= deltaTime;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            // Tegn 3D model
            foreach(ModelMesh mesh in Model.Meshes)
            {
                foreach(BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateTranslation(Position);
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }
    }
}
