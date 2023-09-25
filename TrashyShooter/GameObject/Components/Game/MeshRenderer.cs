
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerEngine
{
    /// <summary>
    /// renders a model/mesh in 3D space
    /// Niels
    /// </summary>
    public class MeshRenderer : Component
    {
        #region Fields & Properties
        /// <summary>
        /// the model that this meshrenderer is displaying
        /// </summary>
        private Model _model;
        #endregion

        #region Methods
        /// <summary>
        /// sets the model of the MeshRenderer to a model from the content with the given model_name
        /// </summary>
        /// <param name="model_name">name of the model</param>
        public void SetModel(string model_name)
        {
            _model = GameWorld.Instance.Content.Load<Model>(model_name);
        }

        /// <summary>
        /// draws the model at the Gameobjects current location from the pivot of the mesh/model
        /// </summary>
        public void Draw3D()
        {
            //checks if it has a model attach and returns if it dosent
            if (_model == null)
            {
                return;
            }
            //renders the model
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    CameraManager.ApplyWorldShading(effect);

                    effect.View = SceneManager.active_scene.viewMatrix;
                    effect.World = SceneManager.active_scene.worldMatrix * 
                        Matrix.CreateRotationX(MathHelper.ToRadians(transform.Rotation.X)) * 
                        Matrix.CreateRotationY(MathHelper.ToRadians(transform.Rotation.Y)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(transform.Rotation.Z)) * 
                        Matrix.CreateTranslation(transform.Position3D);
                    effect.Projection = SceneManager.active_scene.projectionMatrix;
                    mesh.Draw();
                    //Debug.WriteLine(transform.Position3D);
                }
            }
        }
        #endregion
    }
}
