using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController instance;
    public AudioClip[] audioClips;

    AudioSource audioSource;
    int index = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!GetComponent<AudioSource>().isPlaying)
        {
            // ������һ������Ƭ��
            audioSource.clip = audioClips[index];
            index = (index + 1) % audioClips.Length;
            audioSource.Play();
        }
    }
}
