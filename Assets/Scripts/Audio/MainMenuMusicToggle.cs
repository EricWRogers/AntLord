using UnityEngine;
using UnityEngine.UI;

public class MainMenuMusicToggle : MonoBehaviour
{
    private Toggle toggle;

    [Header("Slider Reference")]
    [SerializeField] private MainMenuMusicSlider musicSlider;

    private float volumeBeforeMute = 1f;
    private bool settingUp = true;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    void Start()
    {
        if (toggle == null)
        {
            Debug.LogError("MainMenuMusicToggle needs to be placed on the Toggle object.");
            return;
        }

        if (AudioManager2.instance == null)
        {
            Debug.LogError("No AudioManager2 found in the scene.");
            return;
        }

        if (musicSlider == null)
        {
            musicSlider = FindFirstObjectByType<MainMenuMusicSlider>();
        }

        float savedVolume = AudioManager2.instance.GetMusicVolume();
        bool savedMuted = AudioManager2.instance.IsMusicMuted();

        if (savedVolume > 0f)
        {
            volumeBeforeMute = savedVolume;
        }

        toggle.isOn = savedMuted;

        if (musicSlider != null)
        {
            if (savedMuted)
                musicSlider.SetSliderValueWithoutCallingEvent(0f);
            else
                musicSlider.SetSliderValueWithoutCallingEvent(savedVolume);
        }

        settingUp = false;

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (AudioManager2.instance == null)
            return;

        if (settingUp)
            return;

        if (isOn)
        {
            
            if (musicSlider != null && musicSlider.CurrentValue > 0f)
            {
                volumeBeforeMute = musicSlider.CurrentValue;
            }

            AudioManager2.instance.SetMusicMuted(true);
            AudioManager2.instance.SetMusicVolume(0f);

            if (musicSlider != null)
                musicSlider.SetSliderValueWithoutCallingEvent(0f);
        }
        else
        {
            
            if (volumeBeforeMute <= 0f)
                volumeBeforeMute = 1f;

            AudioManager2.instance.SetMusicMuted(false);
            AudioManager2.instance.SetMusicVolume(volumeBeforeMute);

            if (musicSlider != null)
                musicSlider.SetSliderValueWithoutCallingEvent(volumeBeforeMute);
        }
    }
}