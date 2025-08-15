using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Board
{
    public static int[] Square = new int[64];
    public static List<Move> MoveList = new List<Move>();

    public static Action OnTurnChanged = null; // Action to notify when the turn changes

    public static int ColorToMove;
    public static int EnPassantSquare = -1; // No en passant square by default
    public static int WhiteCastle = 0b11; // 0b11 means both kingside and queenside castling are available
    public static int BlackCastle = 0b11; // 0b11 means both kingside and queenside castling are available
    public static int HalfMoveClock = 0; // Half-move clock for the fifty-move rule
    public static int FullMoveNumber = 0; // Full move number incremented after each move by black

    public static void InitializeBoard(string fen, BoardSquare squarePrefab, BoardPiece piecePrefab, GraphicalBoard boardVisuals, Transform parent, bool isPlayingWhite, out List<BoardSquare> boardSquares, out Dictionary<int, BoardPiece> boardPieces)
    {
        boardSquares = LoadBoardSquares(parent, squarePrefab, boardVisuals);
        boardPieces = LoadBoardPositioFromFEN(fen, piecePrefab, parent);
        MoveGenerator.GenerateMoves(); // Generate initial moves for all pieces
    }

    public static Dictionary<int, BoardPiece> LoadBoardPositioFromFEN(string fen, BoardPiece piecePrefab, Transform boardPiecesTransform) 
    {
        var boardPieces = new Dictionary<int, BoardPiece>();
        var pieceTypeFromSymbol = new Dictionary<char, int>()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook,
            ['q'] = Piece.Queen
        };

        var fenNotationSplit = fen.Split(' ');

        string fenBoard = fenNotationSplit[0];
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
                    boardPieces.Add(square, newPiece); // Add the piece to the dictionary
                    Square[square] = pieceValue; // Assign the piece to the square
                    OnTurnChanged += newPiece.OnTurnChanged; // Subscribe to turn change event

                    newPiece.SetSprite(Piece.PiecesSprites[pieceValue]);
                    newPiece.Value = pieceValue; //use 7 to get piece and 24 to get color
                    newPiece.Square = square; // Store the square index in the piece

                    file++;
                }
            }
        }

        ColorToMove = fenNotationSplit[1] == "w" ? Piece.White : Piece.Black; // Set the color to move based on FEN notation

        WhiteCastle = (fenNotationSplit[2].Contains('K') ? 0b10 : 0) | (fenNotationSplit[2].Contains('Q') ? 0b01 : 0); // Kingside and queenside castling rights for white
        BlackCastle = (fenNotationSplit[2].Contains('k') ? 0b10 : 0) | (fenNotationSplit[2].Contains('q') ? 0b01 : 0); // Kingside and queenside castling rights for black

        if (fenNotationSplit[3].Length > 1)
        {
            int enPassantfile =  fenNotationSplit[3][0] - 'a'; // Convert file letter to index (0-7)
            int enPassantrank = fenNotationSplit[3][1] - '1'; // Convert rank letter to index (0-7)
            EnPassantSquare = enPassantrank * 8 + enPassantfile; // Calculate the square index for en passant
        }
        else
        {
            EnPassantSquare =  -1;
        }

        HalfMoveClock = int.Parse(fenNotationSplit[4]); // Half-move clock for the fifty-move rule

        FullMoveNumber = int.Parse(fenNotationSplit[5]); // Full move number incremented after each move by black

        Debug.Log($"Board initialized from FEN: {fen}");
        Debug.Log($"Color to move: {(ColorToMove == Piece.White ? "White" : "Black")}, En Passant Square: {EnPassantSquare} ({fenNotationSplit[3]}), Half Move Clock: {HalfMoveClock}, Full Move Number: {FullMoveNumber}");
        Debug.Log($"White Castling Rights: {WhiteCastle}, Black Castling Rights: {BlackCastle}");

        return boardPieces;
    }

    public static List<BoardSquare> LoadBoardSquares(Transform boardPlaceContainer, BoardSquare squarePrefab, GraphicalBoard graphicBoard)
    {
        List<BoardSquare> boardSquares = new List<BoardSquare>();
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                bool isLightSquare = (file + rank) % 2 != 0;
                Color squareColor = (isLightSquare) ? graphicBoard.lightColor : graphicBoard.darkColor;
                boardSquares.Add(DrawSquare(squareColor, boardPlaceContainer, squarePrefab));
            }
        }

        return boardSquares;
    }

    public static BoardSquare DrawSquare(Color squareColor, Transform boardPlaceContainer, BoardSquare squarePrefab)
    {
        BoardSquare square = UnityEngine.Object.Instantiate(squarePrefab, boardPlaceContainer);
        square.SetSquareColor(squareColor);
        return square;
    }

    //public static void MakeMove(Move move)
    //{
    //    // Update the board state
    //    var opponentPiece = Square[move.TargetSquare].Piece;
    //    if (opponentPiece != null)
    //    {
    //        // Handle capture logic if the target square already has a piece
    //        opponentPiece.gameObject.SetActive(false); // Optionally deactivate the captured piece
    //    }

    //    var pieceMoved = Square[move.StartingSquare].Piece;

    //    Square[move.TargetSquare].Piece = pieceMoved; // Move the piece to the target square
    //    Square[move.StartingSquare].Piece = null;

    //    pieceMoved.transform.position = Square[move.TargetSquare].transform.position; // Move the piece to the target square
    //    pieceMoved.Square = move.TargetSquare; // Update the square index in the piece

    //    EnPassantSquare = -1; // Reset en passant square after a move

    //    switch(move.MoveFlag)
    //    {
    //        case Move.DoublePush:
    //        {
    //            // Set the en passant square for the next turn
    //            var offset = ColorToMove == Piece.White ? 8 : -8; // Determine the direction based on color
    //            EnPassantSquare = move.TargetSquare - offset;
    //            break;
    //        }
    //        case Move.EnPassantCapture:
    //        {
    //            // Handle en passant capture
    //            var lastMove = MoveList.Last();
    //            Square[lastMove.TargetSquare].Piece.gameObject.SetActive(false); // Optionally deactivate the captured pawn
    //            Square[lastMove.TargetSquare].Piece = null; // Remove the captured pawn
    //            break;
    //        }
    //        case Move.QueenPromotion:
    //        {
    //            // Handle promotion logic
    //            pieceMoved.Sprite = Piece.PiecesSprites[PromotedPiece]; // Update the piece sprite
    //            break;
    //        }
    //        case MoveType.Castling:
    //        {
    //            // Handle castling logic
    //            int targetSquareFile = move.TargetSquare % 8; // Get the file of the target square
    //            int rookStartingSquare;
    //            int rookTargetSquare;

    //            if (targetSquareFile == 2) // Queen-side castling
    //            {
    //                rookStartingSquare = move.TargetSquare - 2; // Rook starts two squares left of the king
    //                rookTargetSquare = move.TargetSquare + 1; // Rook starts on the right of the king
    //            }
    //            else // King-side castling
    //            {
    //                rookStartingSquare = move.TargetSquare + 1; // Rook starts one square right of the king
    //                rookTargetSquare = move.TargetSquare - 1; // Rook starts on the left of the king
    //            }

    //            Square[rookTargetSquare].Piece = Square[rookStartingSquare].Piece; // Move the rook
    //            Square[rookStartingSquare].Piece = null; // Clear the old rook position

    //            Square[rookTargetSquare].Piece.Square = rookTargetSquare; // Update the rook's square index
    //            Square[rookTargetSquare].Piece.transform.position = Square[rookTargetSquare].transform.position; // Update rook position visually

    //            break;
    //        }
    //    }

    //    Moves.GenerateAttackedSquares(ColorToMove); // Update attacked squares after the move

    //    // Switch color to move
    //    SwitchColorToMove();
    //    // Add the move to the move list
    //    MoveList.Add(move);
    //}

    public static void MakeMove(Move move)
    {

    }

    public static void UnmakeMove()
    {
        if (MoveList.Count == 0) return; // No moves to unmake

        Move lastMove = MoveList[MoveList.Count - 1];
        MoveList.RemoveAt(MoveList.Count - 1);

        if (lastMove.MoveFlag == Move.EnPassantCapture)
        {
            EnPassantSquare = lastMove.TargetSquare; // Restore the en passant square
        }

        // Restore the board state
        Square[lastMove.StartingSquare] = Square[lastMove.TargetSquare];
        Square[lastMove.TargetSquare] = 0;
        // Switch color back to the previous player
        SwitchColorToMove();
    }

    public static void SwitchColorToMove()
    {
        ColorToMove = ColorToMove == Piece.White ? Piece.Black : Piece.White;

        OnTurnChanged?.Invoke(); // Notify subscribers that the turn has changed
    }
}