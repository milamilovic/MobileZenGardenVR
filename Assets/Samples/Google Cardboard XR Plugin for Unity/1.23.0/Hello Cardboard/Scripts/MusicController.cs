using UnityEngine;

public class MusicController : MonoBehaviour
{ 
    [Header("Music Mode")]
    [SerializeField] private bool useAdaptiveMusic = true;

    [Header("Adaptive Music Clips")]
    [SerializeField] private AudioClip[] musicLoops = new AudioClip[8];
    [SerializeField] private AudioClip[] musicTails = new AudioClip[8];

    [Header("Simple Looping Music")]
    [SerializeField] private AudioClip simpleLoopingSong;

    [Header("Settings")]
    [SerializeField] private float musicVolumeAdaptive = 0.7f;
    [SerializeField] private float musicVolumeSimple = 0.3f;
    [SerializeField] private int loopsBeforeIncrease = 3;

    private void Start()
    {
        if (useAdaptiveMusic)
        {
            // Use adaptive music system
            AudioManager.Instance.SetupAdaptiveMusic(musicLoops, musicTails);
            AudioManager.Instance.SetLoopsBeforeIncrease(loopsBeforeIncrease);
            AudioManager.Instance.StartAdaptiveMusic(musicVolumeAdaptive, true);
        }
        else
        {
            // Use simple looping song
            AudioManager.Instance.PlayMusic(simpleLoopingSong, musicVolumeSimple);
        }
    }
}