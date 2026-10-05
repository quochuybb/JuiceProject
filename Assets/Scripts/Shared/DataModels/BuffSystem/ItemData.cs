using System.Collections.Generic;
using UnityEngine;

namespace Core.BuffSystem
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "JuiceProject/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        [TextArea]
        public string description;
        public Sprite icon;

        public ItemRarity rarity;

        public List<BuffEffect> effects = new List<BuffEffect>();
    }
}
