using Microsoft.Xna.Framework;

namespace MultiplayerEngine
{
    /// <summary>
    /// set up and keeps the camera updated to the owning gameobjects transform
    /// Niels
    /// </summary>
    public class Camera : Component
    {
        #region Methods

        //setup for the camera
        public void Awake()
        {
            SceneManager.active_scene.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90f), 
                GameWorld.Instance.Graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);//sets up the projection matrix to a field of view on 45 degrees
            SceneManager.active_scene.viewMatrix = Matrix.CreateLookAt(
                transform.Position3D + Vector3.Backward * 10, transform.Position3D + Vector3.Down, 
                new Vector3(0f, 0f, 1f));//Sets Z as the upwards axis for the view
        }
        //updates the cameras position and rotation every frame
        public void Update()
        {
            //creates a direction vector
            Vector3 dir = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(transform.Rotation.Y)) * Matrix.CreateRotationZ(MathHelper.ToRadians(transform.Rotation.Z)));
            //makes the camere look at the direction vectors end cordinate from the cameras transform position
            SceneManager.active_scene.viewMatrix = Matrix.CreateLookAt(transform.Position3D, transform.Position3D + dir, new Vector3(0, 0, 1));
        }
        #endregion
    }
}