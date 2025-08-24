using Chess.Board.Core;
using Chess.Board.UI;
using Chess.Pieces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Utils
{
    /// <summary>
    /// Provides parsing for FEN strings and PGN files.
    /// </summary>
    public class ChessParser : MonoBehaviour
    {
        /// <summary>
        /// Loads a chess position onto the board from a FEN (Forsyth-Edwards Notation) string.
        /// </summary>
        /// <remarks>The method parses the FEN string and updates the board state accordingly. This
        /// includes placing pieces on the board, setting the active color to move, configuring castling rights,
        /// identifying the en passant target square (if any), and initializing the halfmove clock and fullmove number.
        /// <para> The FEN string must follow the standard format: <c>[Piece Placement] [Active Color] [Castling
        /// Availability] [En Passant Target] [Halfmove Clock] [Fullmove Number]</c>. </para> <para> Example FEN string:
        /// <c>"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"</c> </para></remarks>
        /// <param name="fen">A string representing the chess position in FEN format. The FEN string must include the board layout, active
        /// color, castling availability, en passant target square, halfmove clock, and fullmove number.</param>
        public static void LoadFENOnBoard(string fen)
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

                        BoardHandler.Square[rank * 8 + file] = pieceValue; // Assign the piece to the square

                        file++;
                    }
                }
            }

            BoardHandler.ColorToMove = fenNotationSplit[1] == "w" ? Piece.White : Piece.Black; // Set the color to move based on FEN notation

            BoardHandler.WhiteCastle = (fenNotationSplit[2].Contains('K') ? 0b10 : 0) | (fenNotationSplit[2].Contains('Q') ? 0b01 : 0); // Kingside and queenside castling rights for white
            BoardHandler.BlackCastle = (fenNotationSplit[2].Contains('k') ? 0b10 : 0) | (fenNotationSplit[2].Contains('q') ? 0b01 : 0); // Kingside and queenside castling rights for black

            if (fenNotationSplit[3].Length > 1)
            {
                int enPassantfile = fenNotationSplit[3][0] - 'a'; // Convert file letter to index (0-7)
                int enPassantrank = fenNotationSplit[3][1] - '1'; // Convert rank letter to index (0-7)
                BoardHandler.EnPassantSquare = enPassantrank * 8 + enPassantfile; // Calculate the square index for en passant
            }
            else
            {
                BoardHandler.EnPassantSquare = -1;
            }

            BoardHandler.HalfMoveClock = int.Parse(fenNotationSplit[4]); // Half-move clock for the fifty-move rule

            BoardHandler.FullMoveNumber = int.Parse(fenNotationSplit[5]); // Full move number incremented after each move by black

            Debug.Log($"Board initialized from FEN: {fen}");
            Debug.Log($"Color to move: {(BoardHandler.ColorToMove == Piece.White ? "White" : "Black")}, En Passant Square: {BoardHandler.EnPassantSquare} ({fenNotationSplit[3]}), Half Move Clock: {BoardHandler.HalfMoveClock}, Full Move Number: {BoardHandler.FullMoveNumber}");
            Debug.Log($"White Castling Rights: {BoardHandler.WhiteCastle}, Black Castling Rights: {BoardHandler.BlackCastle}");
        }
    }
}
