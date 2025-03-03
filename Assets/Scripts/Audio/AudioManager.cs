using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public List<Song> playlist = new List<Song>();
    private AudioSource audioSource;
    private int currentSongIndex = 0;
    public bool isPlaying = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        // Rimuoviamo l'autoplay
    }

    public void StartCarMusic()
    {
        if (playlist.Count > 0 && !isPlaying)
        {
            PlaySong(0);
        }
    }

    public void PlaySong(int index)
    {
        if (index >= 0 && index < playlist.Count)
        {
            currentSongIndex = index;
            audioSource.clip = playlist[currentSongIndex].audioClip;
            audioSource.Play();
            isPlaying = true;
            UpdateMusicUI();
        }
    }

    public void PlayPause()
    {
        if (isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
        }
        else
        {
            audioSource.UnPause();
            isPlaying = true;
        }
        UpdateMusicUI();
    }

    public void NextSong()
    {
        currentSongIndex = (currentSongIndex + 1) % playlist.Count;
        PlaySong(currentSongIndex);
    }

    public void PreviousSong()
    {
        currentSongIndex = (currentSongIndex - 1 + playlist.Count) % playlist.Count;
        PlaySong(currentSongIndex);
    }

    public Song GetCurrentSong()
    {
        return playlist[currentSongIndex];
    }

    public void StopCarMusic()
    {
        if (audioSource.isPlaying || isPlaying)
        {
            audioSource.Stop();
            isPlaying = false;
            UpdateMusicUI();
            
            Debug.Log("Car music stopped completely");
        }
    }

    private void UpdateMusicUI()
    {
        MusicUIController.Instance?.UpdateUI(GetCurrentSong());
    }
}
