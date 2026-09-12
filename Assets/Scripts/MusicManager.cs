using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    [SerializeField] private AudioSource musicSource;
    private const string MusicVolumeKey = "MusicVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        musicSource.volume = savedVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;

        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }
}