using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectMSG.SaveSystem
{
    public sealed class SaveSystemNotification : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private float visibleSeconds = 2f;

        private Coroutine hideRoutine;

        public static void Show(string message)
        {
            SaveSystemNotification notification = FindAnyObjectByType<SaveSystemNotification>(FindObjectsInactive.Include);
            if (notification == null)
            {
                notification = CreateRuntimeNotification();
            }

            notification.ShowMessage(message);
        }

        public void ShowMessage(string message)
        {
            EnsureReferences();

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(visibleSeconds);

            if (panel != null)
            {
                panel.SetActive(false);
            }

            hideRoutine = null;
        }

        private void EnsureReferences()
        {
            if (panel == null)
            {
                panel = gameObject;
            }

            if (messageText == null)
            {
                messageText = GetComponentInChildren<TMP_Text>(true);
            }
        }

        private static SaveSystemNotification CreateRuntimeNotification()
        {
            GameObject canvasObject = new GameObject("Save System Notification Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 450f);
            canvasObject.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasObject);

            GameObject panelObject = new GameObject("Save Notification Panel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.78f);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -32f);
            panelRect.sizeDelta = new Vector2(520f, 72f);

            GameObject textObject = new GameObject("Message Text");
            textObject.transform.SetParent(panelObject.transform, false);
            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24f;
            text.color = Color.white;

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 8f);
            textRect.offsetMax = new Vector2(-16f, -8f);

            SaveSystemNotification notification = canvasObject.AddComponent<SaveSystemNotification>();
            notification.panel = panelObject;
            notification.messageText = text;
            panelObject.SetActive(false);
            return notification;
        }
    }
}
