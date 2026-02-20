using System.Collections;
using TMPro;
using UnityEngine;

public class GSandwichScorePopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Auto Find")]
    [SerializeField] private GGameManager gameManager;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Coroutine popupRoutine;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GGameManager>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GGameManager>();

        if (gameManager != null)
            gameManager.OnSandwichSubmitted += HandleSandwichSubmitted;
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnSandwichSubmitted -= HandleSandwichSubmitted;
    }

    private void HandleSandwichSubmitted(GGameManager.SandwichScore score)
    {
        if (scoreText != null)
            scoreText.text = $"{score.starsEarned}/{score.starsPossible} Stars";

        if (debugLogs)
            Debug.Log($"[GSandwichScorePopup] Showing popup: {score.starsEarned}/{score.starsPossible}");

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(ShowPopupRoutine());
    }

    private IEnumerator ShowPopupRoutine()
    {
        if (canvasGroup == null)
            yield break;

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(showDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        popupRoutine = null;
    }
}