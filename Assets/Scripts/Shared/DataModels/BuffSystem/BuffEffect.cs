using UnityEngine;

namespace Core.BuffSystem
{
    [System.Serializable]
    public struct BuffEffect
    {
        public EffectType effectType;
        public EffectTarget target;

        public float value;

        public float duration;
    }
}
