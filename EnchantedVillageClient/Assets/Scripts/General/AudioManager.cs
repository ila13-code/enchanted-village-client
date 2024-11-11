using UnityEngine;


namespace Unical.Demacs.EnchantedVillage
{

    public class AudioManager : MonoBehaviour
    {
        private AudioSource musicSource;
        private AudioSource sfxSource;

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f; 

            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlayBackgroundMusic(AudioClip music)
        {
            if (musicSource.clip != music)
            {
                musicSource.clip = music;
                musicSource.Play();
            }
        }

        public void PlaySFX(AudioClip sfx)
        {
            sfxSource.PlayOneShot(sfx);
        }


        public void CollectResource()
        {
            sfxSource.PlayOneShot(ServicesManager.Instance.collectResources);
        }


        public void PlaceBuilding()
        {
            sfxSource.PlayOneShot(ServicesManager.Instance.placeBuilding);
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void PauseMusic()
        {
            musicSource.Pause();
        }

        public void ResumeMusic()
        {
            musicSource.UnPause();
        }
    }
}