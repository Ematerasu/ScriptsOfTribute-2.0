using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceProgressSlider : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int minChoices = 1;
    [SerializeField] private int maxChoices = 5;
    [SerializeField] private int selectedCount = 0;

    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI sliderText;

    [Header("Colors")]
    [SerializeField] private Color tooFewColor = Color.gray;
    [SerializeField] private Color enoughColor = new Color(0.6f, 1f, 0.6f); // light green
    [SerializeField] private Color maxedColor = new Color(0.2f, 0.6f, 0.2f); // dark green

    private void Reset()
    {
        slider = GetComponent<Slider>();
        if (sliderText == null)
            sliderText = GetComponentInChildren<TextMeshProUGUI>();
        if (fillImage == null && slider != null)
            fillImage = slider.fillRect?.GetComponent<Image>();
    }

    public void SetBounds(int min, int max)
    {
        minChoices = min;
        maxChoices = max;
        slider.maxValue = maxChoices;
        UpdateVisuals();
    }

    public void SetSelectedCount(int count)
    {
        selectedCount = count;
        slider.value = selectedCount;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (selectedCount < minChoices)
        {
            fillImage.color = tooFewColor;
            sliderText.text = $"You need to choose at least {minChoices} card{(minChoices == 1 ? "" : "s")}";
        }
        else if (selectedCount >= minChoices && selectedCount < maxChoices)
        {
            fillImage.color = enoughColor;
            sliderText.text = $"{selectedCount} of {maxChoices} selected";
        }
        else // selectedCount == maxChoices
        {
            fillImage.color = maxedColor;
            sliderText.text = $"{selectedCount} of {maxChoices} selected (Max)";
        }
    }
}
