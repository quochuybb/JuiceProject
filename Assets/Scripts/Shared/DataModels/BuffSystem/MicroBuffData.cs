using System.Collections.Generic;
using UnityEngine;

namespace Core.BuffSystem
{
    [CreateAssetMenu(fileName = "NewMicroBuff", menuName = "JuiceProject/Micro Buff Data")]
    public class MicroBuffData : ScriptableObject
    {
        public string formulaId;
        public string skillName;
        [TextArea]
        public string description;

        public bool overrideBaseDamage = false;

        public List<BuffEffect> effects = new List<BuffEffect>();
    }
}
