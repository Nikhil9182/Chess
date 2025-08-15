using Chess.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator
{
    /// <summary>
    /// This dictionary stores all the moves for the current color in turn.
    /// Use this to access moves for a specific starting square.
    /// </summary>
    public static Dictionary<int, List<Move>> Moves = new Dictionary<int, List<Move>>();
    public static List<int> AttackSquares = new List<int>();

    public static readonly int[] KnightDirectionOffsets = { 15, 17, 10, 6, -15, -17, -10, -6 };
    public static readonly int[] SlidingDirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
    public static readonly int[][] NumSquaresToEdge;

    static MoveGenerator()
    {
        NumSquaresToEdge = PrecomputedMoveData();
    }

    static int[][] PrecomputedMoveData()
    {
        int[][] numSquaresToEdge = new int[64][];
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                int numNorth = 7 - rank;
                int numSouth = rank;
                int numWest = file;
                int numEast = 7 - file;

                int squareIndex = rank * 8 + file;

                numSquaresToEdge[squareIndex] = new int[]
                {
                numNorth,
                numSouth,
                numWest,
                numEast,
                Mathf.Min(numNorth, numWest),
                Mathf.Min(numSouth, numEast),
                Mathf.Min(numNorth, numEast),
                Mathf.Min(numSouth, numWest)
                };
            }
        }
        return numSquaresToEdge; // Return the initialized array
    }

    public static void GenerateMoves()
    {
        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            int piece = Board.Square[startSquare];

            if (Piece.IsColor(piece, Board.ColorToMove))
            {
                // Sliding Pieces (Rooks, Bishops, Queens)
                if (Piece.IsSlidingPiece(piece))
                {
                    Moves.AddMoves(GenerateSlidingPieceMoves(piece, startSquare));
                }
                // Knights
                if (Piece.IsType(piece, Piece.Knight))
                {
                    Moves.AddMoves(GenerateKnightMoves(piece, startSquare));
                }
                if (Piece.IsType(piece, Piece.Pawn))
                {
                    Moves.AddMoves(GeneratePawnMoves(piece, startSquare));
                }
                if (Piece.IsType(piece, Piece.King))
                {
                    Moves.AddMoves(GenerateKingMoves(piece, startSquare));
                }
            }
        }
    }

    public static List<Move> GenerateSlidingPieceMoves(int piece, int startSquare)
    {
        var generatedMoves = new List<Move>();
        int friendlyColor = piece & 24;
        int opponentColor = friendlyColor == Piece.White ? Piece.Black : Piece.White;

        var startDirIndex = Piece.IsType(piece, Piece.Bishop) ? 4 : 0; // Bishop starts at 4
        var endDirIndex = Piece.IsType(piece, Piece.Rook) ? 4 : 8; // Rook ends at 4

        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            for (int n = 0; n < NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + SlidingDirectionOffsets[directionIndex] * (n + 1);
                int pieceOnTargetSquare = Board.Square[targetSquare];

                // Blocked by friendly piece, so dont move further in this direction
                if (Piece.IsColor(pieceOnTargetSquare, friendlyColor))
                {
                    break;
                }

                generatedMoves.Add(new Move(startSquare, targetSquare, Move.NoFlag));

                // Can't capture after capturing an enemy piece, so stop here
                if (Piece.IsColor(pieceOnTargetSquare, opponentColor))
                {
                    break;
                }
            }
        }

        return generatedMoves;
    }

    public static List<Move> GenerateKnightMoves(int piece, int startSquare)
    {
        var generatedMoves = new List<Move>();
        int friendlyColor = piece & 24;
        int opponentColor = friendlyColor == Piece.White ? Piece.Black : Piece.White;

        int startFile = startSquare % 8;
        int startRank = startSquare / 8;

        foreach (var offset in KnightDirectionOffsets)
        {
            int targetSquare = startSquare + offset;
            if (targetSquare < 0 || targetSquare >= 64) continue;

            int targetFile = targetSquare % 8;
            int targetRank = targetSquare / 8;

            int pieceOnTargetSquare = Board.Square[targetSquare];

            if (Piece.IsColor(pieceOnTargetSquare, friendlyColor))
            {
                continue; // Can't move to a square occupied by friendly piece
            }

            // Ensure move is a valid L-shape without wrap
            if (Mathf.Abs(startFile - targetFile) > 2 || Mathf.Abs(startRank - targetRank) > 2) continue;

            if ((Mathf.Abs(startFile - targetFile) == 2 && Mathf.Abs(startRank - targetRank) == 1) ||
                (Mathf.Abs(startFile - targetFile) == 1 && Mathf.Abs(startRank - targetRank) == 2))
            {

                generatedMoves.Add(new Move(startSquare, targetSquare, Move.NoFlag));
            }
        }

        return generatedMoves;
    }


    public static List<Move> GeneratePawnMoves(int piece, int startSquare)
    {
        var generatedMoves = new List<Move>();
        int friendlyColor = piece & 24;
        int opponentColor = friendlyColor == Piece.White ? Piece.Black : Piece.White;

        int startFile = startSquare % 8;
        int startRank = startSquare / 8;

        int forwardDir = friendlyColor == Piece.White ? 8 : -8;
        int captureLeft = friendlyColor == Piece.White ? 7 : -9;
        int captureRight = friendlyColor == Piece.White ? 9 : -7;

        // One-step forward
        int oneStep = startSquare + forwardDir;
        if (oneStep >= 0 && oneStep < 64 && Board.Square[oneStep] == Piece.None)
        {
            int targetRank = oneStep / 8;
            if (targetRank == 0 || targetRank == 7) // Promotion condition
            {
                // Add promotions to all piece types
                generatedMoves.Add(new Move(startSquare, oneStep, Move.QueenPromotion));
                generatedMoves.Add(new Move(startSquare, oneStep, Move.RookPromotion));
                generatedMoves.Add(new Move(startSquare, oneStep, Move.BishopPromotion));
                generatedMoves.Add(new Move(startSquare, oneStep, Move.KnightPromotion));
            }
            else
            {
                generatedMoves.Add(new Move(startSquare, oneStep, Move.NoFlag));
            }

            // Two-step forward if not moved
            if ((Piece.IsColor(piece, Piece.White) && startRank == 1) || (Piece.IsColor(piece, Piece.Black) && startRank == 6))
            {
                int twoStep = startSquare + forwardDir * 2;
                if (Board.Square[twoStep] == Piece.None)
                {
                    generatedMoves.Add(new Move(startSquare, twoStep, Move.DoublePush));
                }
            }
        }

        // Normal capture left
        int leftSquare = startSquare + captureLeft;
        if (leftSquare >= 0 && leftSquare < 64)
        {
            int targetFile = leftSquare % 8;
            int targetRank = leftSquare / 8;
            if (Mathf.Abs(startFile - targetFile) == 1 && Piece.IsColor(Board.Square[leftSquare], opponentColor))
            {
                if (targetRank == 0 || targetRank == 7) // Promotion condition
                {
                    // Add promotions to all piece types
                    generatedMoves.Add(new Move(startSquare, leftSquare, Move.QueenPromotion));
                    generatedMoves.Add(new Move(startSquare, leftSquare, Move.RookPromotion));
                    generatedMoves.Add(new Move(startSquare, leftSquare, Move.BishopPromotion));
                    generatedMoves.Add(new Move(startSquare, leftSquare, Move.KnightPromotion));
                }
                else
                {
                    generatedMoves.Add(new Move(startSquare, leftSquare, Move.NoFlag));
                }
            }
        }

        // Normal capture right
        int rightSquare = startSquare + captureRight;
        if (rightSquare >= 0 && rightSquare < 64)
        {
            int targetFile = rightSquare % 8;
            int targetRank = rightSquare / 8;
            if (Mathf.Abs(startFile - targetFile) == 1 && Piece.IsColor(Board.Square[rightSquare], opponentColor))
            {
                if (targetRank == 0 || targetRank == 7) // Promotion condition
                {
                    // Add promotions to all piece types
                    generatedMoves.Add(new Move(startSquare, rightSquare, Move.QueenPromotion));
                    generatedMoves.Add(new Move(startSquare, rightSquare, Move.RookPromotion));
                    generatedMoves.Add(new Move(startSquare, rightSquare, Move.BishopPromotion));
                    generatedMoves.Add(new Move(startSquare, rightSquare, Move.KnightPromotion));
                }
                else
                {
                    generatedMoves.Add(new Move(startSquare, rightSquare, Move.NoFlag));
                }
            }
        }

        // --- En Passant Capture ---
        if (Board.EnPassantSquare != -1) // You need to store this in Board after double push
        {
            // Left en passant
            if (startFile > 0 && Board.EnPassantSquare == startSquare + captureLeft)
            {
                generatedMoves.Add(new Move(startSquare, Board.EnPassantSquare, Move.EnPassantCapture));
            }
            // Right en passant
            if (startFile < 7 && Board.EnPassantSquare == startSquare + captureRight)
            {
                generatedMoves.Add(new Move(startSquare, Board.EnPassantSquare, Move.EnPassantCapture));
            }
        }

        return generatedMoves;
    }

    public static List<Move> GenerateKingMoves(int piece, int startSquare)
    {
        var generatedMoves = new List<Move>();
        int friendlyColor = piece & 24;
        int opponentColor = friendlyColor == Piece.White ? Piece.Black : Piece.White;

        int startFile = startSquare % 8;
        int startRank = startSquare / 8;

        foreach (var offset in SlidingDirectionOffsets)
        {
            int targetSquare = startSquare + offset;
            if (targetSquare < 0 || targetSquare >= 64) continue;

            int targetFile = targetSquare % 8;

            // Ensure we don't wrap horizontally (file diff should be <= 1)
            if (Mathf.Abs(startFile - targetFile) > 1) continue;

            int pieceOnTargetSquare = Board.Square[targetSquare];
            if (pieceOnTargetSquare == Piece.None || Piece.IsColor(pieceOnTargetSquare, opponentColor))
            {
                generatedMoves.Add(new Move(startSquare, targetSquare, Move.NoFlag));
            }
        }

        if (!Piece.HasMoved(piece))
        {
            // Kingside castling
            int rookSquareKingside = startRank * 8 + 7;
            int rookPieceKingside = Board.Square[rookSquareKingside];
            if (Piece.IsType(rookPieceKingside, Piece.Rook) && !Piece.HasMoved(rookPieceKingside) && Piece.IsColor(rookPieceKingside, friendlyColor))
            {
                if (Board.Square[startSquare + 1] == Piece.None &&
                    Board.Square[startSquare + 2] == Piece.None &&
                    !AttackSquares.Contains(startSquare) &&
                    !AttackSquares.Contains(startSquare + 1) &&
                    !AttackSquares.Contains(startSquare + 2))
                {
                    generatedMoves.Add(new Move(startSquare, startSquare + 2, Move.KingCastle)); // Castle kingside
                }
            }

            // Queenside castling
            int rookSquareQueenside = startRank * 8;
            int rookPieceQueenside = Board.Square[rookSquareQueenside];
            if (Piece.IsType(rookPieceQueenside, Piece.Rook) && !Piece.HasMoved(rookPieceQueenside) && Piece.IsColor(rookPieceKingside, friendlyColor))
            {
                if (Board.Square[startSquare - 1] == Piece.None &&
                    Board.Square[startSquare - 2] == Piece.None &&
                    Board.Square[startSquare - 3] == Piece.None &&
                    !AttackSquares.Contains(startSquare) &&
                    !AttackSquares.Contains(startSquare - 1) &&
                    !AttackSquares.Contains(startSquare - 2))
                {
                    generatedMoves.Add(new Move(startSquare, startSquare - 2, Move.QueenCastle)); // Castle queenside
                }
            }
        }

        return generatedMoves;
    }

}
