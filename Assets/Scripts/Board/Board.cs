using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Board
{
    public static BoardSquare[] Square;
    public static List<Move> MoveList = new List<Move>();

    public static int ColorToMove;
    public static int EnPassantSquare = -1; // No en passant square by default

    static Board()
    {
        ColorToMove = Piece.White;
        Square = new BoardSquare[64];
    }

    public static void LoadBoardPosition(string fen, BoardPiece piecePrefab, Transform boardPiecesTransform) 
    {
        var pieceTypeFromSymbol = new Dictionary<char, int>()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook,
            ['q'] = Piece.Queen
        };

        string fenBoard = fen.Split(' ')[0];
        int file = 0, rank = 7;

        foreach (char symbol in fenBoard)
        {
            if (symbol == '/')
            {
                file = 0;
                rank--;
            }
            else
            {
                if (char.IsDigit(symbol))
                {
                    file += (int)char.GetNumericValue(symbol);
                }
                else
                {
                    int pieceColor = char.IsUpper(symbol) ? Piece.White : Piece.Black;
                    int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    int pieceValue = pieceType | pieceColor;
                    int square = rank * 8 + file;

                    var newPiecePos = new Vector2(-3.5f + file, -3.5f + rank);
                    BoardPiece newPiece = Object.Instantiate(piecePrefab, newPiecePos, Quaternion.identity, boardPiecesTransform);

                    newPiece.Sprite = Piece.PiecesSprites[pieceValue];
                    newPiece.Value = pieceValue; //use 7 to get piece and 24 to get color
                    newPiece.Square = square; // Store the square index in the piece

                    Square[square].Piece = newPiece; // Assign the piece to the square
                    //Debug.Log("Rank : " + rank + "  File : " + file + "  Square : " + Square[rank * 8 + file]);
                    file++;
                }
            }
        }
    }

    public static void MakeMove(Move move)
    {
        // Update the board state

        if (Square[move.TargetSquare].Piece != null)
        {
            // Handle capture logic if the target square already has a piece
            Square[move.TargetSquare].Piece.gameObject.SetActive(false); // Optionally deactivate the captured piece
        }

        Square[move.TargetSquare].Piece = Square[move.StartingSquare].Piece;
        Square[move.StartingSquare].Piece = null;

        EnPassantSquare = -1; // Reset en passant square after a move

        switch(move.Type)
        {
            case MoveType.DoublePush:
            {
                // Set the en passant square for the next turn
                var offset = ColorToMove == Piece.White ? 8 : -8; // Determine the direction based on color
                EnPassantSquare = move.TargetSquare - offset;
                break;
            }
            case MoveType.EnPassant:
            {
                // Handle en passant capture
                var lastMove = MoveList.Last();
                Square[lastMove.TargetSquare].Piece.gameObject.SetActive(false); // Optionally deactivate the captured pawn
                Square[lastMove.TargetSquare].Piece = null; // Remove the captured pawn
                break;
            }
        }

        // Switch color to move
        SwitchColorToMove();
        // Add the move to the move list
        MoveList.Add(move);
    }

    public static void UnmakeMove()
    {
        if (MoveList.Count == 0) return; // No moves to unmake

        Move lastMove = MoveList[MoveList.Count - 1];
        MoveList.RemoveAt(MoveList.Count - 1);

        if (lastMove.Type == MoveType.EnPassant)
        {
            EnPassantSquare = lastMove.TargetSquare; // Restore the en passant square
        }

        // Restore the board state
        Square[lastMove.StartingSquare].Piece = Square[lastMove.TargetSquare].Piece;
        Square[lastMove.TargetSquare].Piece = null;
        // Switch color back to the previous player
        SwitchColorToMove();
    }

    public static void SwitchColorToMove()
    {
        ColorToMove = ColorToMove == Piece.White ? Piece.Black : Piece.White;
    }
}