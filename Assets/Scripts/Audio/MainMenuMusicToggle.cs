using UnityEngine;
using UnityEngine.UI;

public class MainMenuMusicToggle : MonoBehaviour
{
    private Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    void Start()
    {
        if (toggle == null)
        {
            Debug.LogError("MainMenuMusicToggle needs to be placed on da toggle");
            return;
        }

        if (AudioManager2.instance == null)
        {
            Debug.LogError("No Audio Manager found in the scene.");
            return;
        }

        // Toggle ON means music is muted.
        toggle.isOn = AudioManager2.instance.IsMusicMuted();

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

        
        AudioManager2.instance.SetMusicMuted(isOn);
    }
}