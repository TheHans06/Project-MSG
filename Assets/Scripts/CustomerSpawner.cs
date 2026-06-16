using UnityEngine;
using System.Collections;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer References")]
    public GameObject customerPrefab;
    public Transform startPoint;
    public Transform cartPoint;
    public Transform endPoint;

    [Header("Spawn Settings")]
    public float spawnCheckInterval = 2f;
    private int spawnChance = 50;
    private bool isSpawning = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {   
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnCheckInterval);

            int roll = Random.Range(1, 101);

            if (roll <= spawnChance)
            {
                spawnCustomer();
            }
        }
    }

    void spawnCustomer()
    {
        GameObject newCustomer = Instantiate(customerPrefab, startPoint.position, Quaternion.identity, transform);

        CustomerMovement moveScript = newCustomer.GetComponent<CustomerMovement>();

        if (moveScript != null)
        {
            moveScript.startPoint = this.startPoint;
            moveScript.endPoint = this.endPoint;

            //buy chance logice untuk customer
            if (PrepManager.Instance != null)
            {
                PlayerData data = PrepManager.Instance.playerData;

                int buyChance = 40;

                // --- MODIFIKATOR HARGA ---
                if (data.sellingPrice > 15000) buyChance -= 20;
                else if (data.sellingPrice < 15000) buyChance += 20;

                // --- MODIFIKATOR RESEP ---
                // Panggil function penilai resep
                int totalError = CalculateRecipeError(data);
                int recipeBonus = 0;

                // Rahasia Dapur (Tiers)
                if (totalError <= 1) recipeBonus = 20;       // Sempurna
                else if (totalError <= 3) recipeBonus = 5;   // Biasa aja
                else recipeBonus = -15;                      // Hancur total

                buyChance += recipeBonus;

                buyChance = Mathf.Clamp(buyChance, 0, 100);
                int roll = Random.Range(1, 101);
                moveScript.isInterested = (roll <= buyChance);
            }
        }
    }
    
    public void StopSpawning()
    {
        // Ubah variabel izin spawn jadi false
        isSpawning = false; 
        
        Debug.Log("Spawner dimatikan! Pelanggan baru berhenti datang.");
        
        // (Opsional) Kalau lu pakai InvokeRepeating buat spawn, tambahin ini:
        // CancelInvoke("NamaFungsiSpawnLu");
    }

    public int CalculateRecipeError(PlayerData data)
    {
        // The Golden Recipe: Bakso 4, Mie 2, MSG 1
        int errorBakso = Mathf.Abs(data.baksoRecipeQty - 4);
        int errorMie = Mathf.Abs(data.mieRecipeQty - 2);
        int errorMSG = Mathf.Abs(data.msgRecipeQty - 1);

        // Balikin total poin kesalahannya
        return errorBakso + errorMie + errorMSG;
    }
}
