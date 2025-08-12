using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Board
{
    public static BoardSquare[] Square;
    public static List<Move> MoveList = new List<Move>();

    public static Action OnTurnChanged; // Action to notify when the turn changes

    public static int ColorToMove;
    public static int EnPassantSquare = -1; // No en passant square by default
    public static int PromotedPiece = Piece.None; // Default promotion piece

    static Board()
    {
        Square = new BoardSquare[64];
        MoveList = new List<Move>();
        OnTurnChanged = null;
        ColorToMove = Piece.White;
    }

    public static void InitializeBoard(string fen, BoardSquare squarePrefab, BoardPiece piecePrefab, GraphicalBoard boardVisuals, Transform parent, bool isPlayingWhite)
    {
        LoadBoardSquares(parent, squarePrefab, boardVisuals);
        LoadBoardPosition(fen, piecePrefab, parent);
        SetSides(isPlayingWhite, boardVisuals);
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

                    BoardPiece newPiece = UnityEngine.Object.Instantiate(piecePrefab, boardPiecesTransform);

                    Square[square].Piece = newPiece; // Assign the piece to the square
                    OnTurnChanged += newPiece.OnTurnChanged; // Subscribe to turn change event

                    newPiece.Sprite = Piece.PiecesSprites[pieceValue];
                    newPiece.Value = pieceValue; //use 7 to get piece and 24 to get color
                    newPiece.Square = square; // Store the square index in the piece

                    file++;
                }
            }
        }
    }

    public static void LoadBoardSquares(Transform boardPlaceContainer, BoardSquare squarePrefab, GraphicalBoard graphicBoard)
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                bool isLightSquare = (file + rank) % 2 != 0;
                Color squareColor = (isLightSquare) ? graphicBoard.lightColor : graphicBoard.darkColor;
                var square = DrawSquare(squareColor, boardPlaceContainer, squarePrefab);
                Square[rank * 8 + file] = square;
            }
        }
    }

    public static BoardSquare DrawSquare(Color squareColor, Transform boardPlaceContainer, BoardSquare squarePrefab)
    {
        BoardSquare square = UnityEngine.Object.Instantiate(squarePrefab, boardPlaceContainer);
        square.SetSquareColor(squareColor);
        return square;
    }

    public static void SetSides(bool isPlayingWhite, GraphicalBoard boardVisuals)
    {
        var markers = isPlayingWhite ? 0 : 7; // Adjust markers based on the player's color
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int squareIndex = rank * 8 + file;
                bool isLightSquare = (file + rank) % 2 != 0;


                Square[squareIndex].transform.position = GetSquarePosition(rank, file, isPlayingWhite);

                if (Square[squareIndex].Piece != null)
                {
                    Square[squareIndex].Piece.transform.position = Square[squareIndex].transform.position; // Update piece position visually
                }

                if (rank == markers) Square[squareIndex].ShowFile(true, ((char)('a' + file)).ToString(), (isLightSquare) ? boardVisuals.darkColor : boardVisuals.lightColor);
                else Square[squareIndex].ShowFile(false);

                if (file == markers) Square[squareIndex].ShowRank(true, (rank + 1).ToString(), (isLightSquare) ? boardVisuals.darkColor : boardVisuals.lightColor);
                else Square[squareIndex].ShowRank(false);
            }
        }
    }

    public static void MakeMove(Move move)
    {
        // Update the board state
        var opponentPiece = Square[move.TargetSquare].Piece;
        if (opponentPiece != null)
        {
            // Handle capture logic if the target square already has a piece
            opponentPiece.gameObject.SetActive(false); // Optionally deactivate the captured piece
        }

        var pieceMoved = Square[move.StartingSquare].Piece;

        Square[move.TargetSquare].Piece = pieceMoved; // Move the piece to the target square
        Square[move.StartingSquare].Piece = null;

        pieceMoved.transform.position = Square[move.TargetSquare].transform.position; // Move the piece to the target square
        pieceMoved.Square = move.TargetSquare; // Update the square index in the piece
        pieceMoved.Value |= 32; // Set the piece as moved (assuming 32 is the bit for moved)

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
            case MoveType.Promotion:
            {
                // Handle promotion logic
                pieceMoved.Sprite = Piece.PiecesSprites[PromotedPiece]; // Update the piece sprite
                move.PromotedPiece = PromotedPiece; // Set the promoted piece type
                pieceMoved.Value = PromotedPiece | 32;
                break;
            }
            case MoveType.Castling:
            {
                // Handle castling logic
                int targetSquareFile = move.TargetSquare % 8; // Get the file of the target square
                int rookStartingSquare;
                int rookTargetSquare;

                if (targetSquareFile == 2) // Queen-side castling
                {
                    rookStartingSquare = move.TargetSquare - 2; // Rook starts two squares left of the king
                    rookTargetSquare = move.TargetSquare + 1; // Rook starts on the right of the king
                }
                else // King-side castling
                {
                    rookStartingSquare = move.TargetSquare + 1; // Rook starts one square right of the king
                    rookTargetSquare = move.TargetSquare - 1; // Rook starts on the left of the king
                }

                Square[rookTargetSquare].Piece = Square[rookStartingSquare].Piece; // Move the rook
                Square[rookStartingSquare].Piece = null; // Clear the old rook position

                Square[rookTargetSquare].Piece.Square = rookTargetSquare; // Update the rook's square index
                Square[rookTargetSquare].Piece.Value |= 32; // Set the rook as moved (assuming 32 is the bit for moved)
                Square[rookTargetSquare].Piece.transform.position = Square[rookTargetSquare].transform.position; // Update rook position visually
                
                break;
            }
        }

        Moves.GenerateAttackedSquares(ColorToMove); // Update attacked squares after the move

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

    public static Vector2 GetSquarePosition(int rank, int file, bool isPlayingWhite)
    {
        float squareSize = 1f; // Adjust to match your prefab spacing
        float halfBoard = (8 - 1) / 2f; // = 3.5

        float x = file - halfBoard;
        float y = rank - halfBoard;

        if (!isPlayingWhite)
        {
            x = -x;
            y = -y;
        }

        return new Vector2(x * squareSize, y * squareSize);
    }


    public static void SwitchColorToMove()
    {
        ColorToMove = ColorToMove == Piece.White ? Piece.Black : Piece.White;

        OnTurnChanged?.Invoke(); // Notify subscribers that the turn has changed
    }
}