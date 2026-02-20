using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodUIRow : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text ageText;
    [SerializeField] private Slider progressSlider;

    [Header("Optional: Assign Fill Image (recommended)")]
    [SerializeField] private Image fillImage; // the Image on "Fill"

    [Header("Marker Line (|)")]
    [SerializeField] private RectTransform markerParent; // optional, else uses slider RectTransform
    [SerializeField] private float markerWidth = 4f;
    [SerializeField] private float markerHeightMultiplier = 1.2f;

    [Header("Fill Colors")]
    [SerializeField] private Color rawFillColor = Color.yellow;
    [SerializeField] private Color agedFillColor = Color.green;
    [SerializeField] private Color spoiledFillColor = Color.red;

    private RectTransform _sliderRT;
    private RectTransform _markerRT;

    public void Setup(Sprite icon, string foodName)
    {
        if (iconImage != null) iconImage.sprite = icon;
        if (nameText != null) nameText.text = foodName;

        if (progressSlider != null)
        {
            progressSlider.interactable = false;
            _sliderRT = progressSlider.GetComponent<RectTransform>();
        }

        // Auto-find Fill image if not assigned
        if (fillImage == null && progressSlider != null)
        {
            var fill = progressSlider.transform.Find("Fill Area/Fill");
            if (fill != null) fillImage = fill.GetComponent<Image>();
        }

        if (markerParent == null && progressSlider != null)
        {
            var fa = progressSlider.transform.Find("Fill Area") as RectTransform;
            markerParent = fa != null ? fa : _sliderRT;
        }

        CreateMarkerIfNeeded();
    }

    public void SetProgress(int ageYears, int rawToAgedYears, int agedToSpoiledYears)
    {
        if (progressSlider == null) return;

        int totalYears = Mathf.Max(1, Mathf.Max(0, rawToAgedYears) + Mathf.Max(0, agedToSpoiledYears));
        int clampedAge = Mathf.Clamp(ageYears, 0, totalYears);

        progressSlider.minValue = 0;
        progressSlider.maxValue = totalYears;
        progressSlider.value = clampedAge;

        // Update marker position (raw->aged point)
        UpdateMarker(rawToAgedYears, totalYears);

        // Update ONLY the fill color
        UpdateFillColor(ageYears, rawToAgedYears, totalYears);
    }

    private void UpdateFillColor(int ageYears, int rawToAgedYears, int totalYears)
    {
        if (fillImage == null) return;

        Color c;
        if (ageYears >= totalYears) c = spoiledFillColor; // spoiled or above
        else if (ageYears >= rawToAgedYears) c = agedFillColor;    // aged
        else c = rawFillColor;     // raw

        fillImage.color = c;
    }

    private void CreateMarkerIfNeeded()
    {
        if (_markerRT != null || markerParent == null) return;

        var go = new GameObject("RawToAgedMarker", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(markerParent, false);

        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        img.color = Color.white;

        _markerRT = img.rectTransform;
    }

    private void UpdateMarker(int rawToAgedYears, int totalYears)
    {
        if (_markerRT == null || markerParent == null) return;

        float t = Mathf.Clamp01(rawToAgedYears / (float)totalYears);

        _markerRT.anchorMin = new Vector2(t, 0.5f);
        _markerRT.anchorMax = new Vector2(t, 0.5f);
        _markerRT.pivot = new Vector2(0.5f, 0.5f);
        _markerRT.anchoredPosition = Vector2.zero;

        float h = (_sliderRT != null ? _sliderRT.rect.height : 20f) * markerHeightMultiplier;
        _markerRT.sizeDelta = new Vector2(markerWidth, h);
    }
}