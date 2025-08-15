using System.Collections.Generic;
using Chess.Board.Core;

namespace Chess.Utils
{
    /// <summary>
    /// Provides extension methods for dictionaries that store chess moves.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds a move to the dictionary, grouping moves by their starting square.
        /// If the starting square key doesn't exist, it creates a new list for it.
        /// </summary>
        /// <param name="dict">
        /// The dictionary to add the move to.  
        /// Key: Starting square index (0–63).  
        /// Value: List of moves from that starting square.
        /// </param>
        /// <param name="move">The move to add.</param>
        public static void AddMove(this Dictionary<int, List<Move>> dict, Move move)
        {
            if (dict.ContainsKey(move.StartingSquare))
            {
                // Add move to existing list for that square
                dict[move.StartingSquare].Add(move);
            }
            else
            {
                // Create a new entry with a list containing the move
                dict[move.StartingSquare] = new List<Move> { move };
            }
        }

        /// <summary>
        /// Adds multiple moves to the dictionary, grouping them by their starting square.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="moves"></param>
        public static void AddMoves(this Dictionary<int, List<Move>> dict, IEnumerable<Move> moves)
        {
            foreach (var move in moves)
            {
                dict.AddMove(move);
            }
        }
    }
}
