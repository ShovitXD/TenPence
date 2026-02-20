using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlaylist : MonoBehaviour
{
    [SerializeField] private AudioClip[] playlist;
    [SerializeField] private bool loopPlaylist = true;
    [SerializeField] private bool shuffle = false;
    [SerializeField] private bool playOnStart = true;

    private AudioSource _source;
    private int _index;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (playOnStart) PlayFirst();
    }

    private void Update()
    {
        if (playlist == null || playlist.Length == 0) return;

        // If nothing is playing, start next
        if (!_source.isPlaying && _source.clip != null)
        {
            PlayNext();
        }
    }

    public void PlayFirst()
    {
        if (playlist == null || playlist.Length == 0) return;

        _index = 0;
        PlayIndex(_index);
    }

    public void PlayNext()
    {
        if (playlist == null || playlist.Length == 0) return;

        if (shuffle)
        {
            _index = Random.Range(0, playlist.Length);
        }
        else
        {
            _index++;
            if (_index >= playlist.Length)
            {
                if (!loopPlaylist)
                {
                    _source.clip = null;
                    return;
                }
                _index = 0;
            }
        }

        PlayIndex(_index);
    }

    private void PlayIndex(int i)
    {
        if (i < 0 || i >= playlist.Length) return;
        if (playlist[i] == null) return;

        _source.clip = playlist[i];
        _source.Play();
    }
}