using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MultiplayerEngine
{
    /// <summary>
    /// an audio manager used by audio componets for spacial sound and manegment
    /// Niels/Thor
    /// </summary>
    public static class AudioManager
    {

        public static AudioListener audioListener = new AudioListener();
        public static SoundEffect music, ingame;
        private static SoundEffectInstance musicSource;

        /// <summary>
        /// loads all background music and sound
        /// </summary>
        public static void LoadMusic()
        {
            //music = GameWorld.Instance.Content.Load<SoundEffect>("creepy_music");
            //ingame = GameWorld.Instance.Content.Load<SoundEffect>("SoundFX\\creepy_sound");
        }

        /// <summary>
        /// stas the background music and stops background sound
        /// </summary>
        public static void StartBackgroundMusic()
        {
            if (musicSource == null)
                musicSource = music.CreateInstance();
            else
            {
                musicSource.Stop();
                musicSource.Dispose();
                musicSource = null;
                musicSource = ingame.CreateInstance();
            }
            musicSource.Volume = 0.1f;
            musicSource.Play();
        }

        /// <summary>
        /// starts background sound and stops background music
        /// </summary>
        public static void StatBackgroundSound()
        {
            if (musicSource == null)
                musicSource = ingame.CreateInstance();
            else
            {
                musicSource.Stop();
                musicSource.Dispose();
                musicSource = null;
                musicSource = ingame.CreateInstance();
            }
            musicSource.Volume = 0.05f;
            musicSource.Play();
        }

        /// <summary>
        /// apply 3D sound to given sfx instance
        /// </summary>
        /// <param name="soundEffectInstance">sound effect instance to apply spacial on</param>
        /// <param name="emitter">the emitter thats emmiting the sound</param>
        /// <param name="maxListenDistance">the maximum distance that the sound can be heard awai from the center point</param>
        public static void ApplySpacialSound(SoundEffectInstance soundEffectInstance, AudioEmitter emitter, float maxListenDistance)
        {
            //sets volume based on max distance
            float volume = Vector3.Distance(audioListener.Position, emitter.Position) / maxListenDistance;
            //apply spacial if volume is higher than 0
            if (volume > 0 && volume <= 1)
            {
                //if (soundEffectInstance.State == SoundState.Stopped)
                //    soundEffectInstance.Play();
                soundEffectInstance.Apply3D(audioListener, emitter);
                soundEffectInstance.Volume = Math.Clamp((1 - volume) * 2,0,1);
            }
            //stops the sound while sound is equal to 0
            else if(soundEffectInstance.State == SoundState.Playing)
                soundEffectInstance.Stop();
        }

        /// <summary>
        /// stops and disposed all sound in the active scene
        /// </summary>
        public static void ResetSound()
        {
            for (int i = 0; i < SceneManager.active_scene.gameObjects.Count; i++)
            {
                SceneManager.active_scene.gameObjects[i].StopSound();
            }
        }
    }
}
