using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("Sound Effects")]
    public AudioClip strikerHit;
    public AudioClip coinHit;
    public AudioClip coinPocket;
    public AudioClip queenPocket;
    public AudioClip wallHit;
    public AudioClip win;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayStrikerHit()
    {
        sfxSource.PlayOneShot(strikerHit);
    }

    public void PlayCoinHit()
    {
        sfxSource.PlayOneShot(coinHit);
    }

    public void PlayCoinPocket()
    {
        sfxSource.PlayOneShot(coinPocket);
    }

    public void PlayQueenPocket()
    {
        sfxSource.PlayOneShot(queenPocket);
    }

    public void PlayWallHit()
    {
        sfxSource.PlayOneShot(wallHit);
    }

    public void PlayWin()
    {
        sfxSource.PlayOneShot(win);
    }
}