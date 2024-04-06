using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerMaterial : MonoBehaviour
{
    [SerializeField] float flickerDuration = 0.05f;
    [SerializeField] Renderer targetRenderer;
    private Color originalColor;

    private Coroutine flickerCoroutine;

    private void Awake()
    {
        originalColor = targetRenderer.material.color;
    }

    public void Flicker(Color flickerToColor)
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }
        flickerCoroutine = StartCoroutine(FlickerRoutine(flickerToColor));
    }

    private IEnumerator FlickerRoutine(Color flickerColor)
    {
        targetRenderer.material.color = flickerColor;
        yield return new WaitForSeconds(flickerDuration);
        targetRenderer.material.color = originalColor;
    }
}
