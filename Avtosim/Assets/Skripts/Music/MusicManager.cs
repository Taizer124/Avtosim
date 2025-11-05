using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;                      // добавлено
using LogitechG29.Sample.Input;  // Logitech G29 Input System

public class MusicManager : MonoBehaviour, IPointerClickHandler
{
    [Header("Music Settings")]
    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Playback Settings")]
    [SerializeField] private bool shuffle = true;
    [SerializeField] private bool loopPlaylist = true;
    [SerializeField] private bool waitForUserInteraction = true;

    [Header("Logitech G29 Input")]
    [SerializeField] private InputControllerReader inputController;  // перетащи сюда объект InputControllerReader в Inspector

    [Header("UI Display")]
    [SerializeField] private TMP_Text trackNameText;  // сюда перетащи TextMeshPro объект дл€ вывода текущего трека

    private List<int> playlist = new List<int>();
    private int currentTrackIndex = -1;
    private bool isFading = false;
    private bool audioInitialized = false;
    private bool isWaitingForClick = true;
    private float switchCooldown = 0.5f;
    private float lastSwitchTime = 0f;

    public static MusicManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (waitForUserInteraction)
            {
                audioSource.Stop();
                isWaitingForClick = true;

                if (GetComponent<Collider2D>() == null && GetComponent<Collider>() == null)
                    gameObject.AddComponent<BoxCollider2D>().size = Vector2.one * 0.1f;
            }
            else
            {
                InitializeAudio();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (inputController != null)
        {
            inputController.OnPlusCallback += OnPlusPressed;
            inputController.OnMinusCallback += OnMinusPressed;
        }
    }

    private void OnDisable()
    {
        if (inputController != null)
        {
            inputController.OnPlusCallback -= OnPlusPressed;
            inputController.OnMinusCallback -= OnMinusPressed;
        }
    }

    private void Update()
    {
        // первичное включение
        if (isWaitingForClick && Input.GetMouseButtonDown(0))
        {
            InitializeAudio();
            isWaitingForClick = false;
        }

        // клавиатура Ч , и .
        if (audioInitialized && Time.time - lastSwitchTime > switchCooldown)
        {
            if (Input.GetKeyDown(KeyCode.Period))
            {
                PlayNextTrack();
                Debug.Log("Next track via keyboard (.)");
                lastSwitchTime = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                PlayPreviousTrack();
                Debug.Log("Previous track via keyboard (,)");
                lastSwitchTime = Time.time;
            }
        }

        // автоматическое переключение после конца трека
        if (audioInitialized && !isFading && !audioSource.isPlaying && currentTrackIndex >= 0)
            PlayNextTrack();
    }

    private void OnPlusPressed(bool isPressed)
    {
        if (isPressed && audioInitialized && Time.time - lastSwitchTime > switchCooldown)
        {
            PlayNextTrack();
            Debug.Log("Next track via G29 Plus button");
            lastSwitchTime = Time.time;
        }
    }

    private void OnMinusPressed(bool isPressed)
    {
        if (isPressed && audioInitialized && Time.time - lastSwitchTime > switchCooldown)
        {
            PlayPreviousTrack();
            Debug.Log("Previous track via G29 Minus button");
            lastSwitchTime = Time.time;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isWaitingForClick)
        {
            InitializeAudio();
            isWaitingForClick = false;
        }
    }

    public void InitializeAudio()
    {
        if (audioInitialized) return;
        CreatePlaylist();
        PlayRandomTrack();
        audioInitialized = true;
    }

    private void CreatePlaylist()
    {
        playlist.Clear();
        for (int i = 0; i < musicTracks.Length; i++)
            playlist.Add(i);
        if (shuffle) ShufflePlaylist();
    }

    private void ShufflePlaylist()
    {
        for (int i = playlist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = playlist[i];
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
    }

    private void PlayRandomTrack()
    {
        if (musicTracks.Length == 0) return;
        currentTrackIndex = shuffle ? Random.Range(0, playlist.Count) : 0;
        StartCoroutine(FadeToNextTrack());
    }

    public void PlayNextTrack()
    {
        if (!audioInitialized || musicTracks.Length == 0) return;
        int nextIndex = currentTrackIndex + 1;

        if (nextIndex >= playlist.Count)
        {
            if (loopPlaylist)
            {
                nextIndex = 0;
                if (shuffle) ShufflePlaylist();
            }
            else
            {
                currentTrackIndex = -1;
                return;
            }
        }

        currentTrackIndex = nextIndex;
        StartCoroutine(FadeToNextTrack());
    }

    public void PlayPreviousTrack()
    {
        if (!audioInitialized || musicTracks.Length == 0) return;
        int prevIndex = currentTrackIndex - 1;

        if (prevIndex < 0)
        {
            if (loopPlaylist)
            {
                prevIndex = playlist.Count - 1;
                if (shuffle) ShufflePlaylist();
            }
            else
            {
                currentTrackIndex = -1;
                return;
            }
        }

        currentTrackIndex = prevIndex;
        StartCoroutine(FadeToNextTrack());
    }

    private IEnumerator FadeToNextTrack()
    {
        isFading = true;

        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = startVolume;
        }

        int trackIndex = playlist[currentTrackIndex];
        audioSource.clip = musicTracks[trackIndex];

        float targetVolume = audioSource.volume;
        audioSource.volume = 0f;
        audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        isFading = false;

        // --- ќбновл€ем UI и лог ---
        UpdateTrackNameUI(audioSource.clip.name);
        Debug.Log("Now playing: " + audioSource.clip.name);
    }

    private void UpdateTrackNameUI(string trackName)
    {
        if (trackNameText != null)
            trackNameText.text = trackName;
    }
}
