using Microsoft.Xna.Framework;

namespace MultiplayerEngine
{
    /// <summary>
    /// a transform used by gameobjects to keep track of location an rotation
    /// Niels
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// the position of the transform
        /// </summary>
        private Vector3 _position = new Vector3(0, 0, 0);
        /// <summary>
        /// the rotation of the transform
        /// </summary>
        private Vector3 _rotation = new Vector3(0, 0, 0);
        /// <summary>
        /// the transforms position in 2D
        /// </summary>
        public Vector2 Position { get { return new Vector2(_position.X, _position.Y); } set { _position.X = value.X; _position.Y = value.Y; } }
        /// <summary>
        /// the transforms position in 3D
        /// </summary>
        public Vector3 Position3D { get { return _position; } set { _position = value; } }
        /// <summary>
        /// the transforms rotation in 3D
        /// </summary>
        public Vector3 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = new Vector3(value.X, value.Y, value.Z);
            }
        }
    }
}
