using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Common
{
    public class ClipSetting : ScriptableObject
    {
        public List<AudioClipSetting> Definitions;

        private Dictionary<string, AudioClip> clipMap;
        private Dictionary<string, AudioClip> ClipMap
        {
            get
            {
                if(clipMap == null)
                {
                    clipMap = new Dictionary<string, AudioClip>();
                    foreach (AudioClipSetting setting in Definitions) 
                        if(setting.Clip != null) clipMap[setting.Tag] = setting.Clip;
                }
                return clipMap;
            }
        }

        public AudioClip GetClip(string tag)
        {
            return ClipMap[tag];
        }
    }

    [Serializable]
    public class AudioClipSetting
    {
        public string Tag;
        public AudioClip Clip;
    }
}
