using Chess.Board.ScriptableObjects;
using Chess.Board.UI;
using Chess.Pieces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chess.Board.Core
{
    public static class BoardHandler
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

        //public static void MakeMove(Move move)
        //{
        //    EnPassantSquare = -1; // Reset en passant square after a move

        //    switch (move.MoveFlag)
        //    {
        //        case Move.DoublePush:
        //            {
        //                // Set the en passant square for the next turn
        //                var offset = ColorToMove == Piece.White ? 8 : -8; // Determine the direction based on color
        //                EnPassantSquare = move.TargetSquare - offset;
        //                break;
        //            }
        //        case Move.QueenCastle or Move.KingCastle:
        //            {
        //                // Handle castling logic
        //                int targetSquareFile = move.TargetSquare % 8; // Get the file of the target square
        //                int rookStartingSquare;
        //                int rookTargetSquare;

        //                if (targetSquareFile == 2) // Queen-side castling
        //                {
        //                    rookStartingSquare = move.TargetSquare - 2; // Rook starts two squares left of the king
        //                    rookTargetSquare = move.TargetSquare + 1; // Rook starts on the right of the king
        //                }
        //                else // King-side castling
        //                {
        //                    rookStartingSquare = move.TargetSquare + 1; // Rook starts one square right of the king
        //                    rookTargetSquare = move.TargetSquare - 1; // Rook starts on the left of the king
        //                }

        //                Square[rookTargetSquare].Piece = Square[rookStartingSquare].Piece; // Move the rook
        //                Square[rookStartingSquare].Piece = null; // Clear the old rook position

        //                Square[rookTargetSquare].Piece.Square = rookTargetSquare; // Update the rook's square index
        //                Square[rookTargetSquare].Piece.transform.position = Square[rookTargetSquare].transform.position; // Update rook position visually

        //                break;
        //            }
        //    }

        //    Moves.GenerateAttackedSquares(ColorToMove); // Update attacked squares after the move

        //    // Switch color to move
        //    SwitchColorToMove();
        //    // Add the move to the move list
        //    MoveList.Add(move);
        //}

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
}