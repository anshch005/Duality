using UnityEngine;
using UnityEngine.UI;

public class SensitivityManager : MonoBehaviour
{
    public static SensitivityManager Instance { get; private set; }

    [Header("UI Sliders (Optional)")]
    [SerializeField] private Slider blueSensitivitySlider;
    [SerializeField] private Slider pinkSensitivitySlider;
    [SerializeField] private Slider globalSensitivitySlider;

    [Header("Sensitivity Range Settings")]
    [SerializeField] private float minSensitivity = 0.2f;
    [SerializeField] private float maxSensitivity = 2.5f;
    [SerializeField] private float defaultSensitivity = 1.0f;

    private const string BlueSensitivityKey = "BlueSensitivity";
    private const string PinkSensitivityKey = "PinkSensitivity";
    private const string GlobalSensitivityKey = "GlobalSensitivity";

    private float blueSensitivity = 1.0f;
    private float pinkSensitivity = 1.0f;
    private float globalSensitivity = 1.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void Start()
    {
        SyncSliders();
    }

    private void LoadSettings()
    {
        blueSensitivity = PlayerPrefs.GetFloat(BlueSensitivityKey, defaultSensitivity);
        pinkSensitivity = PlayerPrefs.GetFloat(PinkSensitivityKey, defaultSensitivity);
        globalSensitivity = PlayerPrefs.GetFloat(GlobalSensitivityKey, defaultSensitivity);
    }

    private void SyncSliders()
    {
        if (blueSensitivitySlider != null)
        {
            blueSensitivitySlider.minValue = minSensitivity;
            blueSensitivitySlider.maxValue = maxSensitivity;
            blueSensitivitySlider.SetValueWithoutNotify(blueSensitivity);
        }

        if (pinkSensitivitySlider != null)
        {
            pinkSensitivitySlider.minValue = minSensitivity;
            pinkSensitivitySlider.maxValue = maxSensitivity;
            pinkSensitivitySlider.SetValueWithoutNotify(pinkSensitivity);
        }

        if (globalSensitivitySlider != null)
        {
            globalSensitivitySlider.minValue = minSensitivity;
            globalSensitivitySlider.maxValue = maxSensitivity;
            globalSensitivitySlider.SetValueWithoutNotify(globalSensitivity);
        }
    }

    public void SetBlueSensitivity(float value)
    {
        blueSensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity);
        PlayerPrefs.SetFloat(BlueSensitivityKey, blueSensitivity);
        PlayerPrefs.Save();

        if (blueSensitivitySlider != null && !Mathf.Approximately(blueSensitivitySlider.value, blueSensitivity))
        {
            blueSensitivitySlider.SetValueWithoutNotify(blueSensitivity);
        }
    }

    public void SetPinkSensitivity(float value)
    {
        pinkSensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity);
        PlayerPrefs.SetFloat(PinkSensitivityKey, pinkSensitivity);
        PlayerPrefs.Save();

        if (pinkSensitivitySlider != null && !Mathf.Approximately(pinkSensitivitySlider.value, pinkSensitivity))
        {
            pinkSensitivitySlider.SetValueWithoutNotify(pinkSensitivity);
        }
    }

    public void SetGlobalSensitivity(float value)
    {
        globalSensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity);
        PlayerPrefs.SetFloat(GlobalSensitivityKey, globalSensitivity);
        PlayerPrefs.Save();

        if (globalSensitivitySlider != null && !Mathf.Approximately(globalSensitivitySlider.value, globalSensitivity))
        {
            globalSensitivitySlider.SetValueWithoutNotify(globalSensitivity);
        }
    }

    public float GetBlueSensitivity()
    {
        return blueSensitivity;
    }

    public float GetPinkSensitivity()
    {
        return pinkSensitivity;
    }

    public float GetGlobalSensitivity()
    {
        return globalSensitivity;
    }

    public float GetSensitivityForPlayer(Player player)
    {
        if (player == null)
            return globalSensitivity;

        if (player.IsPlayerBlue())
        {
            return blueSensitivity * globalSensitivity;
        }

        if (player.IsPlayerPink())
        {
            return pinkSensitivity * globalSensitivity;
        }

        return globalSensitivity;
    }

    public float GetSensitivity(string playerTagOrName)
    {
        if (string.IsNullOrEmpty(playerTagOrName))
            return globalSensitivity;

        if (playerTagOrName.Contains("Blue"))
        {
            return blueSensitivity * globalSensitivity;
        }

        if (playerTagOrName.Contains("Pink"))
        {
            return pinkSensitivity * globalSensitivity;
        }

        return globalSensitivity;
    }
}
