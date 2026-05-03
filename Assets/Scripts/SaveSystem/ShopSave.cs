using System;
using System.Collections.Generic;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class ShopSave
    {
        public List<string> unlockedItems = new();
        public string selectedItemName = "Player_1";
    }
}