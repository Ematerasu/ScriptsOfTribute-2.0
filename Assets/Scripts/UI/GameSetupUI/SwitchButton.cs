using UnityEngine;
using UnityEngine.UI;

public class SwitchButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject onView;
    [SerializeField] private GameObject offView;

    [SerializeField] private bool startOn = false;
    private bool isOn;

    private void Awake()
    {
        button.onClick.AddListener(Toggle);
        SetState(startOn);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        SetState(!isOn);
    }

    public void SetState(bool value)
    {
        isOn = value;
        if (onView != null) onView.SetActive(isOn);
        if (offView != null) offView.SetActive(!isOn);
    }

    public bool GetState() => isOn;
}
