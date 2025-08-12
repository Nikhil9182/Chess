using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Piece
{
    public const int None = 0;

    public const int King = 1;
    public const int Pawn = 2;
    public const int Knight = 3;
    public const int Bishop = 4;
    public const int Rook = 5;
    public const int Queen = 6;

    public const int White = 8;
    public const int Black = 16;

    public const string folderName = "Sprites/Pieces_0/";

    public static Dictionary<int, Sprite> PiecesSprites = new Dictionary<int, Sprite>() 
    {
        [King | Black] = Resources.Load<Sprite>(folderName + "k 1"),
        [Pawn | Black] = Resources.Load<Sprite>(folderName + "p 1"),
        [Knight | Black] = Resources.Load<Sprite>(folderName + "n 1"),
        [Bishop | Black] = Resources.Load<Sprite>(folderName + "b 1"),
        [Rook | Black] = Resources.Load<Sprite>(folderName + "r 1"),
        [Queen | Black] = Resources.Load<Sprite>(folderName + "q 1"),
        [King | White] = Resources.Load<Sprite>(folderName + "K"),
        [Pawn | White] = Resources.Load<Sprite>(folderName + "P"),
        [Knight | White] = Resources.Load<Sprite>(folderName + "N"),
        [Bishop | White] = Resources.Load<Sprite>(folderName + "B"),
        [Rook | White] = Resources.Load<Sprite>(folderName + "R"),
        [Queen | White] = Resources.Load<Sprite>(folderName + "Q"),
    };

    public static bool IsSlidingPiece(int pieceType)
    {
        pieceType &= 7; // Mask to get the piece type only (ignoring color)
        return pieceType == Bishop || pieceType == Rook || pieceType == Queen;
    }

    public static bool IsColor(int piece, int color)
    {
        return (piece & 24) == color;
    }

    public static bool IsType(int piece, int type)
    {
        return (piece & 7) == type;
    }

    public static bool HasMoved(int piece)
    {
        return (piece & 32) != 0; // Check if the HasMoved bit is set
    }
}
