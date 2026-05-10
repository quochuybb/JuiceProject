namespace Core.Gem
{
    public class MissionCollectGem
    {
        public GemType gemType; 
        public int requiredAmount; 
        public int collectedAmount; 
    
        public bool IsCompleted => collectedAmount >= requiredAmount;
    }
}