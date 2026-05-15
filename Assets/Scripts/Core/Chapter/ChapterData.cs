using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Chapter", menuName = "JuiceProject/Chapter")]
public class ChapterData : ScriptableObject
{
    public string chapterID;
    public string chapterName;
    public int energyCost = 1;

    public List<LayerConfig> layers;

    public List<string> availableRecipeIDsInShop;
}