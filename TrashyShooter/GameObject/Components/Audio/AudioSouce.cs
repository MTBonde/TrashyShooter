using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;

namespace MultiplayerEngine
{
    /// <summary>
    /// used to create sound in the world
    /// Thor
    /// </summary>
    public class AudioSouce : Component
    {

        AudioEmitter _emitter = new AudioEmitter();
        SoundEffectInstance _soundEffectInstance;
        SoundEffect _soundEffect;
        List<SoundEffect> _soundEffects = new List<SoundEffect>();
        bool running;
        /// <summary>
        /// the maximum distance the sound can be heard from
        /// </summary>
        public float maxDistance = 10;
        /// <summary>
        /// detemines if the audiosouce should be spacialy affected
        /// </summary>
        public bool Spacial = false;
        /// <summary>
        /// whether or not to loop the sound
        /// </summary>
        public bool loop = false;
        /// <summary>
        /// use random sound effects from the list instead of one persistant one
        /// </summary>
        public bool randomLoopEffects;
        /// <summary>
        /// the base volume of the audio source
        /// </summary>
        public float volume = 1;
        /// <summary>
        /// if true the audio source will be removed and cleaned at the end of the frame
        /// </summary>
        bool remove = false;

        /// <summary>
        /// sets the audio souces sound effect
        /// </summary>
        /// <param name="path">the sound effect to add</param>
        public void SetSoundEffect(string path)
        {
            _soundEffect = GameWorld.Instance.Content.Load<SoundEffect>(path);
        }

        /// <summary>
        /// Adds a sound effect to the list of effects to play
        /// </summary>
        /// <param name="path">effect to add</param>
        public void AddSoundEffect(string path)
        {
            _soundEffects.Add(GameWorld.Instance.Content.Load<SoundEffect>(path));
        }

        /// <summary>
        /// Updates the audio sound based on the variables
        /// </summary>
        public void Update()
        {
            if (running && !remove)
            {
                //sets sound position
                _emitter.Position = transform.Position3D;
                //sets volume
                _soundEffectInstance.Volume = volume;
                //applys spacial sound if spacial is true
                if(Spacial)
                    AudioManager.ApplySpacialSound(_soundEffectInstance,_emitter,maxDistance);
                //sets running to false if loop is off and soundÍnstance has stopped playing
                if (!loop && _soundEffectInstance.State == SoundState.Stopped)
                    running = false;
                //loops using a random sound from soundEffects list if randomLoopEffects is turned on and the audio has stopped playing
                if (loop && randomLoopEffects && _soundEffectInstance.State == SoundState.Stopped)
                {
                    _soundEffectInstance = _soundEffects[Globals.Rnd.Next(0, _soundEffects.Count)].CreateInstance();
                    _soundEffectInstance.Play();
                }
            }
        }

        /// <summary>
        /// tells if the audio source is playing or stopped
        /// </summary>
        /// <returns>playing is true stopped is false</returns>
        public bool IsPlaying()
        {
            return running;
        }

        /// <summary>
        /// starts the audioSouce so it plays sound
        /// </summary>
        public void Play()
        {
            //checks if running
            if (!running)
            {
                running = true;
                //plays a random sound from soundeffects list if randomLoopEffects is on
                if(randomLoopEffects)
                    _soundEffectInstance = _soundEffects[Globals.Rnd.Next(0, _soundEffects.Count)].CreateInstance();
                //else plays the attached sound
                else
                    _soundEffectInstance = _soundEffect.CreateInstance();
                //sets looping
                _soundEffectInstance.IsLooped = loop;
                //starts the sound effect instance
                _soundEffectInstance.Play();
            }
        }

        /// <summary>
        /// stops the sound from playing
        /// </summary>
        public void Stop() 
        { 
            if (running)
            {
                running = false;
                _soundEffectInstance.Stop();
            }
        }

        /// <summary>
        /// used for cleaning up the sound before detroying component
        /// </summary>
        public void StopSound()
        {
            if(_soundEffectInstance != null)
            {
                _soundEffectInstance.Stop();
                _soundEffectInstance.Dispose();
                _soundEffectInstance = null;
                remove = true;
            }
        }
    }
}
