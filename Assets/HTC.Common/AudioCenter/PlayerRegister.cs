using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Common
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerRegister : MonoBehaviour
    {
        [SerializeField]
        private string Tag;

        [SerializeField]
        private AudioSource audioSrc;

        private void OnValidate()
        {
            audioSrc = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            AudioCenter.Instance.Register(Tag, audioSrc);
        }

        private void OnDisable()
        {
            if(AudioCenter.IsInstanceExist) AudioCenter.Instance.Unregister(Tag, audioSrc);
        }
    }
}

