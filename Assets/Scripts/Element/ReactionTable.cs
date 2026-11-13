using UnityEngine;

[System.Serializable]
public class ReactionEntry
{
    public ElementType A;
    public ElementType B;
    public ReactionType Result;
    public int Damage;
}

[CreateAssetMenu(fileName = "ReactionTable", menuName = "ScriptableObjects/ReactionTable")]
public class ReactionTable : ScriptableObject
{
    [SerializeField] private ReactionEntry[] _entries;

    public ReactionEntry Find(ElementType x, ElementType y)
    {
        foreach (ReactionEntry entry in _entries)
        {
            bool sameOrder = entry.A == x && entry.B == y;
            bool reverseOrder = entry.A == y && entry.B == x;
            if (sameOrder || reverseOrder)
            {
                return entry;
            }
        }
        return null;
    }
}