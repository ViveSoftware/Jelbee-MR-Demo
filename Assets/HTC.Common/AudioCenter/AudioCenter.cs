using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Common
{
    public class AudioCenter : Singleton<AudioCenter>
    {
        [SerializeField]
        private List<AudioClipSetting> clipDefs;

        private Dictionary<string, AudioSource> audioSrcMap;
        private Dictionary<string, AudioSource> AudioSrcMap
        {
            get
            {
                if (audioSrcMap == null) audioSrcMap = new Dictionary<string, AudioSource>();
                return audioSrcMap;
            }
        }

        private Dictionary<string, AudioClip> clipMap;
        private Dictionary<string, AudioClip> ClipMap
        {
            get
            {
                if (clipMap == null)
                {
                    clipMap = new Dictionary<string, AudioClip>();
                    foreach (AudioClipSetting setting in clipDefs)
                        if (setting.Clip != null) clipMap[setting.Tag] = setting.Clip;
                }
                return clipMap;
            }
        }

        public void Register(string tag, AudioSource audioSrc)
        {
            AudioSrcMap[tag] = audioSrc;
        }

        public void Unregister(string tag, AudioSource audioSrc)
        {
            if(AudioSrcMap.ContainsKey(tag) && AudioSrcMap[tag] == audioSrc)
            {               
                AudioSrcMap.Remove(tag);
            }
        }

        public AudioSource GetPlayer(string playerTag)
        {
            AudioSource player = null;
            if (!AudioSrcMap.TryGetValue(playerTag, out player))
            {
                Debug.Log($"AudioSource with tag {playerTag} is unexisted!");
                return null;
            }
            return player;
        }

        public AudioClip GetClip(string clipTag)
        {
            AudioClip clip = null;
            if (!ClipMap.TryGetValue(clipTag, out clip))
            {
                Debug.Log($"AudioClip with tag {clipTag} is unexisted!");
                return null;
            }
            return clip;
        }
        string oldTag;
        public void PlayOneShot(string playerTag, string clipTag)
        {
            AudioSource player = GetPlayer(playerTag);
            AudioClip clip = GetClip(clipTag);
            if (player != null && clip != null) {
                if (oldTag != null) Stop(oldTag);
                player.PlayOneShot(clip);                
            }
            oldTag = playerTag;
        }

        public void Play(string playerTag, string clipTag, bool isLoop = false)
        {
            AudioSource player = GetPlayer(playerTag);
            AudioClip clip = GetClip(clipTag);
            if (player != null && clip != null)
            {
                player.clip = clip;
                player.loop = isLoop;
                player.Play();
            }
        }

        public void SetVolumeScale(string playerTag, float volume)
        {
            AudioSource player = GetPlayer(playerTag);
            if(player != null) player.volume = volume;
        }

        public void Stop(string playerTag)
        {
            AudioSource player = GetPlayer(playerTag);
            if (player != null) player.Stop();
        }

        public bool isPlaying(string playerTag)
        {
            return GetPlayer(playerTag).isPlaying;
        }
    }
}

