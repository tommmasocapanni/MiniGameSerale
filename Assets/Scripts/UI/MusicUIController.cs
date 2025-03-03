using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicUIController : MonoBehaviour
{
    public static MusicUIController Instance;
    
    [Header("UI References")]
    public Image coverImage;
    public TMP_Text titleText;
    public TMP_Text artistText;
    public Button playPauseButton;
    public Button nextButton;
    public Button previousButton;
    public Sprite playSprite;
    public Sprite pauseSprite;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupButtons();
        
        // Nascondi la UI all'inizio
        ShowMusicUI(false);
    }

    private void SetupButtons()
    {
        playPauseButton.onClick.AddListener(AudioManager.Instance.PlayPause);
        nextButton.onClick.AddListener(AudioManager.Instance.NextSong);
        previousButton.onClick.AddListener(AudioManager.Instance.PreviousSong);
    }

    public void UpdateUI(Song song)
    {
        if (song != null)
        {
            coverImage.sprite = song.coverArt;
            titleText.text = song.title;
            artistText.text = song.artist;
        }
        UpdatePlayPauseButton(AudioManager.Instance.isPlaying);
    }

    public void UpdatePlayPauseButton(bool isPlaying)
    {
        Image buttonImage = playPauseButton.GetComponent<Image>();
        buttonImage.sprite = isPlaying ? pauseSprite : playSprite;
    }

    public void ShowMusicUI(bool show)
    {
        Debug.Log("Setting MusicUI visibility to: " + show);
        gameObject.SetActive(show);
    }
}
