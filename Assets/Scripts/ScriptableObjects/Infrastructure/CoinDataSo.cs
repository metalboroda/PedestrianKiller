using System;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Infrastructure
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Infrastructure/CoinData", fileName = "CoinData")]
    public class CoinDataSo : ScriptableObject
    {
        public event Action<int> CoinAmountChanged;

        private int _coinAmount;

        public int CoinAmount
        {
            get => _coinAmount;
            set
            {
                if (_coinAmount == value) return;

                _coinAmount = value;

                CoinAmountChanged?.Invoke(_coinAmount);
            }
        }
    }
}