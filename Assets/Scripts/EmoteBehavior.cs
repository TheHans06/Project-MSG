using UnityEngine;

public class EmoteBehavior : MonoBehaviour
{
    [Header("Emote Float Settings")]
    public float floatSpeed = 50f;
    public float lifetime = 1f;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += new Vector2(0, floatSpeed * Time.deltaTime);
        }
    }
}
