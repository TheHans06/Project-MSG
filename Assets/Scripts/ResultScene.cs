using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultScene : MonoBehaviour
{
    [Header("Result Settings")]
    public TextMeshProUGUI servingsSoldText;
    public TextMeshProUGUI avgSatisfactionText;
    public TextMeshProUGUI todayIncomeText;
    public TextMeshProUGUI todayExpenseText;

    void Start()
    {
        displayDailyResult();
    }

    void displayDailyResult()
    {
        if (PrepManager.Instance != null)
        {
            PlayerData data = PrepManager.Instance.playerData;

            // 1. Servings Sold (Total Pelanggan Terlayani)
            servingsSoldText.text = data.dailyCustomersServed.ToString() + " Bowls";

            // 2. Average Customer Satisfaction
            int avgHappiness = 0;
            if (data.dailyCustomersServed > 0)
            {
                avgHappiness = data.dailyHappinessScore / data.dailyCustomersServed;
            }
            avgSatisfactionText.text = avgHappiness.ToString() + "%";

            // 3. Today's Income
            todayIncomeText.text = "Rp. " + data.dailyIncome.ToString("N0");

            // 4. Today's Expense
            todayExpenseText.text = "Rp. " + data.dailyExpense.ToString("N0");
        }
    }

    public void NextDayButton()
    {
        // Reset data harian sebelum masuk ke hari baru biar gak akumulasi salah
        if (PrepManager.Instance != null)
        {
            PrepManager.Instance.playerData.dailyIncome = 0;
            PrepManager.Instance.playerData.dailyExpense = 0;
            PrepManager.Instance.playerData.dailyCustomersServed = 0;
            PrepManager.Instance.playerData.dailyHappinessScore = 0;
        }

        // Balik ke Prep Scene buat belanja/atur resep lagi
        SceneManager.LoadScene("PrepScene");
    }

    public void quitGame()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
