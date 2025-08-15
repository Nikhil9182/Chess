using UnityEngine;

namespace Chess.Board.ScriptableObjects
{
    [CreateAssetMenu]
    public class BoardPositionInFen : ScriptableObject
    {
        public string fenNotation;
    }
}
