using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string playerName = "User1";
    public int currentMoney = 100000;
    public int currentMapIndex = 0;
    public int playerVendorLevel = 0;
    public int currentDay = 1;
    
    public int baksoSupply = 4;
    public int mangkokSupply = 4;
    public int msgSupply = 4;
    public int mieSupply = 4;

    public int sellingPrice = 15000;
    public int advertisingPrice = 0;

    public int dailyIncome = 0;
    public int dailyExpense = 0;
    public int dailyCustomersServed = 0;
    public int dailyHappinessScore = 0;

    public int baksoRecipeQty = 2;
    public int mieRecipeQty = 2;
    public int msgRecipeQty = 1;
}
