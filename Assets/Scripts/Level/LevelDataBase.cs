using UnityEngine;

namespace Level
{
    [CreateAssetMenu(menuName = "Game/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        public LevelData[] levels;
    }
}