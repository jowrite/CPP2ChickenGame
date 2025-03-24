using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class ToneOverlay : MonoBehaviour
{
    [Header("Overlay Settings")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float darkAlpha = 0.5f;

    private Coroutine currentFade;

    private void Start()
    {
        if (overlayImage == null)
        {
            overlayImage = GetComponent<Image>();
        }

        SetOverlayAlpha(0f);

        GameManager.instance.OnToneChanged += HandleToneChanged;
    }


    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnToneChanged -= HandleToneChanged;
        }
    }
    private void HandleToneChanged(GameTone tone)
    {
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }

        if (tone == GameTone.Dark)
        {
            currentFade = StartCoroutine(FadeOverlay(overlayImage.color.a, darkAlpha));
        }
        else
        {
            currentFade = StartCoroutine(FadeOverlay(overlayImage.color.a, 0f));
        }
    }

    private IEnumerator FadeOverlay(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        while(elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            SetOverlayAlpha(newAlpha);
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = alpha;
            overlayImage.color = c;
        }
    }
}
