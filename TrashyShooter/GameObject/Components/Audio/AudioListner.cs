using Microsoft.Xna.Framework;

namespace MultiplayerEngine
{
    /// <summary>
    /// used to update the audioManagers listner position
    /// Thor
    /// </summary>
    public class AudioListner : Component
    {
        //sets the audio listners up to be z posetive
        public void Start()
        {
            AudioManager.audioListener.Up = new Vector3(0, 0, 1);
        }

        // updates the audio managers listner position
        public void Update()
        {
            AudioManager.audioListener.Position = gameObject.transform.Position3D;
            AudioManager.audioListener.Forward = Vector3.Transform(
                Vector3.Up,
                Matrix.CreateRotationZ(MathHelper.ToRadians(transform.Rotation.Z + 90)));
        }
    }
}
