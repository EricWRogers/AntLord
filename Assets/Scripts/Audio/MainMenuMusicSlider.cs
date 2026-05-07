using UnityEngine;
using UnityEngine.UI;

public class MainMenuMusicSlider : MonoBehaviour
{
    private Slider slider;

    public float CurrentValue
    {
        get
        {
            if (slider == null) return 1f;
            return slider.value;
        }
    }

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Start()
    {
        if (slider == null)
        {
            Debug.LogError("MainMenuMusicSlider needs to be placed on the Slider object.");
            return;
        }

        if (AudioManager2.instance == null)
        {
            Debug.LogError("No AudioManager2 found in the scene.");
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;

        slider.value = AudioManager2.instance.GetMusicVolume();

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (AudioManager2.instance == null)
            return;

        AudioManager2.instance.SetMusicVolume(value);

        
        if (value > 0f && AudioManager2.instance.IsMusicMuted())
        {
            AudioManager2.instance.SetMusicMuted(false);
        }
    }

    public void SetSliderValue(float value)
    {
        if (slider == null)
            return;

        slider.value = Mathf.Clamp01(value);
    }

    public void SetSliderValueWithoutCallingEvent(float value)
    {
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }
}