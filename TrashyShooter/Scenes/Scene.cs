using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MultiplayerEngine
{
    /// <summary>
    /// the basic infomation needed to set up and run a scene
    /// Niels
    /// </summary>
    public abstract class Scene
    {
        #region Properties & Fields
        /// <summary>
        /// a list of all the gameobjects currently instatiated in the scene
        /// </summary>
        public List<GameObject> gameObjects = new List<GameObject>();


        /// <summary>
        /// the point the camera is looking at
        /// </summary>
        public Vector3 camTarget;
        /// <summary>
        /// the position of the camera
        /// </summary>
        public Vector3 camPosition;
        /// <summary>
        /// the cameras projection matrix used for determining fov
        /// </summary>
        public Matrix projectionMatrix;
        /// <summary>
        /// primaryly used to keep track of what direction is up for the camera
        /// </summary>
        public Matrix viewMatrix;
        /// <summary>
        /// primarily used to determent the worlds up, forward and side directions and where the center of the world is located
        /// </summary>
        public Matrix worldMatrix;
        #endregion


        #region methods
        /// <summary>
        /// put all your scene setup and instantiation code in this method
        /// </summary>
        public abstract void SetupScene();
        #endregion
    }
}
