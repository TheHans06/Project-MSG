using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class PrepManager : MonoBehaviour
{
    public static PrepManager Instance {get; private set; }
    
    [Header("PlayerData")]
    public PlayerData playerData = new PlayerData();

    [Header("Harga Supply (Rp)")]
    public int hargaBakso = 5000;
    public int hargaMie = 2000;
    public int hargaMangkok = 1000;
    public int hargaMSG = 500;

    [Header("UI References - Top Info Bar")]
    public TextMeshProUGUI userName;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI topBaksoText;
    public TextMeshProUGUI topMangkokText;
    public TextMeshProUGUI topMSGText;
    public TextMeshProUGUI topMieText;

    [Header("Header(UI References - Supply Grid)")]
    public TextMeshProUGUI supplyBaksoText;
    public TextMeshProUGUI supplyMangkokText;
    public TextMeshProUGUI supplyMSGText;
    public TextMeshProUGUI supplyMieText;

    [Header("Price & Advertising Grid")]
    public TextMeshProUGUI mieBaksoPrice;
    public TextMeshProUGUI mieBaksoAdvertising;

    [Header("Recipe Grid")]
    public TextMeshProUGUI baksoRecipeText;
    public TextMeshProUGUI mieRecipeText;
    public TextMeshProUGUI msgRecipeText;

    void Start()
    {
        UpdateUI();
    }

// Main Function untuk refresh semua text di layar
    public void UpdateUI()
    {
        //Player Name & Money
        userName.text = playerData.playerName.ToString();
        moneyText.text = playerData.currentMoney.ToString("N0");

        //Update Top Info Bar
        topBaksoText.text = playerData.baksoSupply.ToString();
        topMangkokText.text = playerData.mangkokSupply.ToString();
        topMSGText.text = playerData.msgSupply.ToString();
        topMieText.text = playerData.mieSupply.ToString();

        //Update Supply Grid
        supplyBaksoText.text = playerData.baksoSupply.ToString();
        supplyMangkokText.text = playerData.mangkokSupply.ToString();
        supplyMSGText.text = playerData.msgSupply.ToString();
        supplyMieText.text = playerData.mieSupply.ToString();

        //Update Recipe Grid
        baksoRecipeText.text = playerData.baksoRecipeQty.ToString();
        mieRecipeText.text = playerData.mieRecipeQty.ToString();
        msgRecipeText.text = playerData.msgRecipeQty.ToString();

        //Update Price & Advertising Grid
        mieBaksoPrice.text = playerData.sellingPrice.ToString();
        mieBaksoAdvertising.text = playerData.advertisingPrice.ToString();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Function Bakso Button
    public void BeliBakso()
    {
        if (playerData.currentMoney >= hargaBakso)
        {
            playerData.currentMoney -= hargaBakso;
            playerData.dailyExpense += hargaBakso; //Catat sebagai pengeluaran
            playerData.baksoSupply++;
            UpdateUI();
        }
    }

    public void JualBakso()
    {
        if (playerData.baksoSupply > 0)
        {
            playerData.currentMoney += hargaBakso;
            playerData.dailyExpense -= hargaBakso; //Tarik balik pengeluaran
            playerData.baksoSupply--;
            UpdateUI();
        }
    }

    // Function Mangkok Button
    public void BeliMangkok()
    {
        if (playerData.currentMoney >= hargaMangkok)
        {
            playerData.currentMoney -= hargaMangkok;
            playerData.dailyExpense += hargaMangkok;
            playerData.mangkokSupply++;
            UpdateUI();
        }
    }

    public void JualMangkok()
    {
        if (playerData.mangkokSupply > 0)
        {
            playerData.currentMoney += hargaMangkok;
            playerData.dailyExpense -= hargaMangkok;
            playerData.mangkokSupply--;
            UpdateUI();
        }
    }

    //Function Button MSG
    public void BeliMSG()
    {
        if (playerData.currentMoney >= hargaMSG)
        {
            playerData.currentMoney -= hargaMSG;
            playerData.dailyExpense += hargaMSG;
            playerData.msgSupply++;
            UpdateUI();
        }
    }

    public void JualMSG()
    {
        if (playerData.msgSupply > 0)
        {
            playerData.currentMoney += hargaMSG;
            playerData.dailyExpense -= hargaMSG;
            playerData.msgSupply--;
            UpdateUI();
        }
    }

    //Function Button Mie

    public void BeliMie()
    {
        if(playerData.currentMoney >= hargaMie)
        {
            playerData.currentMoney -= hargaMie;
            playerData.dailyExpense += hargaMie;
            playerData.mieSupply++;
            UpdateUI();
        }
    }

    public void JualMie()
    {
        if(playerData.mieSupply > 0)
        {
            playerData.currentMoney += hargaMie;
            playerData.dailyExpense -= hargaMie;
            playerData.mieSupply--;
            UpdateUI();
        }
    }

    public void risePrice()
    {
        playerData.sellingPrice += 1000;
        UpdateUI();
    }

    public void lowerPrice()
    {
        if (playerData.sellingPrice >= 1000)
        {
            playerData.sellingPrice -= 1000;
            UpdateUI();
        }
    }

    public void riseAds()
    {
        playerData.advertisingPrice += 1000;
        playerData.dailyExpense += playerData.advertisingPrice;
        UpdateUI();
    }

    public void lowerAds()
    {
        if (playerData.advertisingPrice >= 1000)
        {
            playerData.advertisingPrice -= 1000;
            UpdateUI();
        }
    }

    public void addBaksoRecipe()
    {
        
        playerData.baksoRecipeQty++;
        UpdateUI();
    }

    public void reduceBaksoRecipe()
    {
        if (playerData.baksoRecipeQty > 0)
        {
            playerData.baksoRecipeQty--;
            UpdateUI();
        }
    }

    public void addMieRecipe()
    {
        playerData.mieRecipeQty++;
        UpdateUI();
    }

    public void reduceMieRecipe()
    {
        if (playerData.mieRecipeQty > 0)
        {
            playerData.mieRecipeQty--;
            UpdateUI();
        }
    }

    public void addMSGRecipe()
    {
        playerData.msgRecipeQty++;
        UpdateUI();
    }

    public void reduceMSGRecipe()
    {
        if (playerData.msgRecipeQty > 0)
        {
            playerData.msgRecipeQty--;
            UpdateUI();
        }
    }
}
