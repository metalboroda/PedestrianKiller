using System;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class CoinSave
    {
        public int overallCoinAmount;

        public CoinSave SaveCoins(int newOverallCoinAmount)
        {
            overallCoinAmount = newOverallCoinAmount;
            return this;
        }
    }
}