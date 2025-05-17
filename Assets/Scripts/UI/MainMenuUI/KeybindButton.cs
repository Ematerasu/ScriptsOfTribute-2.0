using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeybindButton : MonoBehaviour, IPointerClickHandler
{
    public enum KeybindType { AiMove, ToggleAutoPlay, EndTurn }
    public SettingsUI settingsUI;
    public KeybindType keyType;
    public TextMeshProUGUI labelText;

    private bool isWaitingForKey = false;
    public System.Action<KeyCode> OnKeyAssigned;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isWaitingForKey)
        {
            labelText.text = "<Press any key>";
            isWaitingForKey = true;
        }
    }

    public void SetKey(KeyCode current)
    {
        labelText.text = current.ToString();
    }

    private void Update()
    {
        if (!isWaitingForKey) return;

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                ApplyKey(key);
                break;
            }
        }
    }

    private void ApplyKey(KeyCode key)
    {
        if (settingsUI != null && settingsUI.IsKeyAlreadyUsed(key, keyType))
        {
            labelText.text = "<Already used>";
            return;
        }

        OnKeyAssigned?.Invoke(key);
        labelText.text = key.ToString();
        isWaitingForKey = false;
    }

    private void OnEnable()
    {
        RefreshLabel();
    }

    public void RefreshLabel()
    {
        var sm = SettingsManager.Instance;
        labelText.text = keyType switch
        {
            KeybindType.AiMove => sm.keyAiMove.ToString(),
            KeybindType.ToggleAutoPlay => sm.keyToggleAutoPlay.ToString(),
            KeybindType.EndTurn => sm.keyEndTurn.ToString(),
            _ => "?"
        };
    }

    
}
