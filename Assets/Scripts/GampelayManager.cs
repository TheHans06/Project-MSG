using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GampelayManager : MonoBehaviour
{
    public static GampelayManager Instance { get; private set; }
    
    [Header("Vendor Visuals")]
    public Image vendorDisplay;
    public Sprite[] vendorUpgradeSprites;

    [Header("UI References - Top Bar")]
    public TextMeshProUGUI topPlayerName;
    public TextMeshProUGUI topDayCounter;
    public TextMeshProUGUI topPriceDisplay;

    public UnityEngine.UI.Image topBarFace;
    public Sprite uiHappy;
    public Sprite uiNeutral;
    public Sprite uiAngry;
    private int totalCustomersServed = 0;
    private int totalHappinessScore = 0;

    [Header("UI References - Recipe Qty")]
    public TextMeshProUGUI recipeBaksoText;
    public TextMeshProUGUI recipeMieText;
    public TextMeshProUGUI recipeMSGText;
    public TextMeshProUGUI recipeMangkokText;
    public TextMeshProUGUI bwlAvailableText;

    [Header("UI Reference - Income & Expense Grid")]
    public TextMeshProUGUI dailyIncomeText;
    public TextMeshProUGUI dailyExpenseText;

    [Header("Game Duration Settings")]
    public int maxServingsPot = 18;
    public int currentServings;
    public TextMeshProUGUI potSupply;

    public bool isShopOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {        
        isShopOpen = true;
        
        UpdateVendorVisual();
        LoadDataFromPrep();

        cookPot();
    }

    void Awake()
    {
        Instance = this;
    }

    void LoadDataFromPrep()
    {
        if (PrepManager.Instance != null)
        {
            PlayerData data = PrepManager.Instance.playerData;
    
            topPlayerName.text = data.playerName;
            topPriceDisplay.text = data.sellingPrice.ToString("N0");
            topDayCounter.text = data.currentDay.ToString();

            recipeBaksoText.text = data.baksoRecipeQty.ToString();
            recipeMieText.text = data.mieRecipeQty.ToString();
            recipeMSGText.text = data.msgRecipeQty.ToString();
            bwlAvailableText.text = data.mangkokSupply.ToString();

            if (recipeMangkokText != null) 
            {
                recipeMangkokText.text = "1";
            }
            if (dailyIncomeText != null) dailyIncomeText.text = "+ Rp. " + data.dailyIncome.ToString("N0");
            if (dailyExpenseText != null) dailyExpenseText.text = "- Rp. " + data.dailyExpense.ToString("N0");
        }
        else
        {
            Debug.LogWarning("PrepManager tidak ditemukan! Pastikan play PrepScene dengan benar!");
        }
    }

    void UpdateVendorVisual()
    {
        int level = 0;
        if (PrepManager.Instance != null)
        {
            level = PrepManager.Instance.playerData.playerVendorLevel;
        }

        if (level >= 0 && level < vendorUpgradeSprites.Length)
        {
            vendorDisplay.sprite = vendorUpgradeSprites[level];
        }
    }

    void cookPot()
    {
        PlayerData data = PrepManager.Instance.playerData;

        if (data.baksoSupply >= data.baksoRecipeQty && 
            data.mieSupply >= data.mieRecipeQty && 
            data.msgSupply >= data.msgRecipeQty)
        {
            data.baksoSupply -= data.baksoRecipeQty;
            data.mieSupply -= data.mieRecipeQty;
            data.msgSupply -= data.msgRecipeQty;

            currentServings = maxServingsPot;
            isShopOpen = true;
            Debug.Log("Panci siap! Total porsi: " + currentServings);
            UpdatePotUI();
        }
        else
        {
            // Bahan kurang, gak bisa jualan hari ini
            currentServings = 0;
            Debug.LogWarning("Bahan kurang! Gak bisa masak panci.");
            closedShop(); // Langsung tutup warung
        }
    }

    void UpdatePotUI()
    {
        if (potSupply != null)
        {
            potSupply.text = currentServings.ToString();
        }
    }

    public bool serveCustomer()
    {
        PlayerData data = PrepManager.Instance.playerData;

        //check stock per supply (recipe selalu butuh 1 mangkok)
        if (currentServings > 0 && data.mangkokSupply >= 1)
        {
            currentServings--; // Kurangi kaldu di panci
            data.mangkokSupply -= 1; // Kurangi mangkuk bersih

            //catat income
            data.dailyIncome += data.sellingPrice;

            //tambah duit ke dompet player
            data.currentMoney += data.sellingPrice;
            Debug.Log("LAKU!");

            LoadDataFromPrep();
            UpdatePotUI();

            // Kalau habis, warung otomatis tutup
            if (currentServings <= 0 || data.mangkokSupply <= 0)
            {
                closedShop();
            }

            return true;
        }
        else
        {
            Debug.Log("Tidak Laku!");

            // Masukin skor jelek (misal 0 atau 10) buat pelanggan yang kecewa
            AddHappinessScore(10);

            return false;
        }
    }

    void closedShop()
    {
        isShopOpen = false;
        Debug.Log("Hari Berakhir, saatnya Tutup!");

        // 1. Ambil referensi data
        if (PrepManager.Instance != null)
        {
            PlayerData data = PrepManager.Instance.playerData;
            
            // Tambah hitungan hari buat besok
            data.currentDay++;

            // Titip data kepuasan dan jumlah porsi terjual hari ini ke PlayerData
            data.dailyCustomersServed = totalCustomersServed;
            data.dailyHappinessScore = totalHappinessScore;
        }

        CustomerSpawner spawner = FindAnyObjectByType<CustomerSpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        // Jalankan coroutine nunggu sepi baru pindah scene
        StartCoroutine(WaitForCustomersAndFinish());
    }

    System.Collections.IEnumerator WaitForCustomersAndFinish()
    {
        Debug.Log("Nungguin sisa pelanggan pulang...");

        // Looping ini bakal terus jalan SELAMA masih ada script CustomerMovement di Scene
        // Kita ngeceknya tiap 0.5 detik aja biar CPU gak engap
        while (FindObjectsByType<CustomerMovement>(FindObjectsInactive.Exclude).Length > 0)
        {
            yield return new WaitForSeconds(0.5f); 
        }

        Debug.Log("Jalanan udah sepi! Gas pindah Scene!");
        
        // Pindah ke Result Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("ResultScene");
    }

    public void AddHappinessScore(int score)
    {
        totalCustomersServed++;
        totalHappinessScore += score;

        // Rumus rata-rata!
        int averageHappiness = totalHappinessScore / totalCustomersServed;

        // Ganti gambar di Top Bar sesuai rata-rata
        if (averageHappiness >= 70) 
        {
            topBarFace.sprite = uiHappy;
        }
        else if (averageHappiness >= 40) 
        {
            topBarFace.sprite = uiNeutral;
        }
        else 
        {
            topBarFace.sprite = uiAngry;
        }
    }
}
