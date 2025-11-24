using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuUI : MonoBehaviour
{
    public UIDocument uiDocument;

    private VisualElement duck;
    private bool isWiggling = false;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError("MenuUI: No UIDocument found!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        duck = root.Q<VisualElement>("duck");

        if (duck != null)
        {
            duck.RegisterCallback<ClickEvent>(OnDuckClick);
        }
        else
        {
            Debug.LogWarning("MenuUI: 'duck' element not found in UXML.");
        }
    }

    private void OnDisable()
    {
        if (duck != null)
        {
            duck.UnregisterCallback<ClickEvent>(OnDuckClick);
        }
    }

    private void OnDuckClick(ClickEvent evt)
    {
        if (!isWiggling)
        {
            StartCoroutine(WiggleDuck());
        }
    }

    private IEnumerator WiggleDuck()
    {
        isWiggling = true;
        float duration = 0.5f;
        float elapsed = 0f;
        float magnitude = 15f; // Degrees

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Sin(elapsed * 20f) * magnitude * (1f - elapsed / duration);
            duck.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
            yield return null;
        }

        duck.style.rotate = new Rotate(new Angle(0, AngleUnit.Degree));
        isWiggling = false;
    }
}
