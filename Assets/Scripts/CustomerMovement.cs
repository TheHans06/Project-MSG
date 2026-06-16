using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class CustomerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 90f; 
    public float waitTime = 2.5f;

    public GameObject emotionPrefab;
    public Sprite emoteNeutral;
    public Sprite emoteHappy;
    public Sprite emoteAngry;
    
    // Centang/uncentang manual lewat Inspector kalau mau ngetes dia berhenti
    public bool isInterested = false; 

    // Disembunyikan karena udah diatur otomatis sama CustomerSpawner
    [HideInInspector] public Transform startPoint;
    [HideInInspector] public Transform endPoint;

    private int currentTargetIndex = 1; // 1 = ke gerobak, 2 = ke ujung map
    private bool isWaiting = false;

    private Vector2 queueTargetPosition;
    private bool isAtfrontOfQueue = false;
    private bool isCurrentlyBuying = false;

    void Start()
    {
        // Pastikan pas muncul, dia ada di titik awal
        if (startPoint != null) transform.position = startPoint.position;

        if (isInterested)
        {
            bool gotSpot = QueueManager.Instance.RequestQueueSpot(this);
            if (!gotSpot)
            {
                isInterested = false;
                currentTargetIndex = 2;
            }
        }
        else
        {
            currentTargetIndex = 2;
        }
    }

    public void UpdateTargetPosition(Vector2 newTarget, bool isFront)
    {
        queueTargetPosition = newTarget;
        isAtfrontOfQueue = isFront;
        isWaiting = false;
    }

    void Update()
    {
        // Berhenti gerak kalau lagi nunggu atau data jalanan belum masuk
        if (isWaiting || startPoint == null) return;

        // Tentukan target
        Vector2 target = (currentTargetIndex == 1) ? queueTargetPosition : (Vector2)endPoint.position;

        // Jalan!
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Kalau jarak udah deket sama titik target (sampai)
        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            if (currentTargetIndex == 1)
            {
                isWaiting = true;

                if (isAtfrontOfQueue && !isCurrentlyBuying)
                {
                    StartCoroutine(WaitAtCart());
                }
            }
            else if (currentTargetIndex == 2)
            {
                Destroy(gameObject); // Menghilang di ujung map
            }
        }
    }

    IEnumerator WaitAtCart()
    {
        isCurrentlyBuying = true;
        yield return new WaitForSeconds(waitTime); // Nunggu beli bakso
        
        // PANGGIL KASIR BUAT BAYAR!
        if (GampelayManager.Instance != null)
        {
            GampelayManager.Instance.serveCustomer();
        }

        //Keluar Antrean
        if (QueueManager.Instance != null)
        {
            QueueManager.Instance.LeaveQueue(this);
            showEmotion();
        }
        
        isWaiting = false;
        currentTargetIndex = 2; // Lanjut jalan ke ujung map
    }

    void showEmotion()
    {
        // 1. Spawn emotnya di posisi yang sama kayak pelanggan, taruh di dalem container yang sama (transform.parent)
        GameObject newEmote = Instantiate(emotionPrefab, transform.position, Quaternion.identity, transform.parent);

        // 2. Ambil komponen Image buat ganti gambarnya (pakai UnityEngine.UI.Image karena ini UI)
        UnityEngine.UI.Image emoteImage = newEmote.GetComponent<UnityEngine.UI.Image>();

        if (PrepManager.Instance != null && emoteImage != null)
        {
            PlayerData data = PrepManager.Instance.playerData;

            // 3. Kalkulasi ulang Total Error resep
            int errorBakso = Mathf.Abs(data.baksoRecipeQty - 4);
            int errorMie = Mathf.Abs(data.mieRecipeQty - 2);
            int errorMSG = Mathf.Abs(data.msgRecipeQty - 1);
            int totalError = errorBakso + errorMie + errorMSG;

            // 4. Ganti sprite sesuai Tier DAN kirim nilai ke Manager
            if (totalError <= 1) 
            {
                emoteImage.sprite = emoteHappy;    // Sempurna
                if (GampelayManager.Instance != null) GampelayManager.Instance.AddHappinessScore(100);
            }
            else if (totalError <= 3) 
            {
                emoteImage.sprite = emoteNeutral;  // Biasa aja
                if (GampelayManager.Instance != null) GampelayManager.Instance.AddHappinessScore(50);
            }
            else 
            {
                emoteImage.sprite = emoteAngry;    // Hancur
                if (GampelayManager.Instance != null) GampelayManager.Instance.AddHappinessScore(0);
            }
        }
    }
}