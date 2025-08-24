using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Board.ScriptableObjects;
using Chess.Board.Core;
using Chess.Board.UI;
using Chess.Pieces;
using Chess.Utils;
using System.Linq;

namespace Chess.Board.Managers
{
    public class BoardVisualsManager : MonoBehaviour
    {
        public static BoardVisualsManager Instance;

        public BoardVisualData BoardVisualData;
        public BoardPositionInFen CustomPosition;
        public SquareVisual SquarePrefab;
        public PieceVisual PiecePrefab;
        public Canvas BoardCanvas; // Canvas that holds the board

        public bool LoadDefaultPosition = true;
        public bool PlayAsWhite = true; // If true, white pieces are at the bottom of the board

        private List<SquareVisual> BoardSquares = new List<SquareVisual>(); // List of squares on the board
        private Dictionary<int, PieceVisual> BoardPieces = new Dictionary<int, PieceVisual>(); // Dictionary to hold pieces by their square index
        private RectTransform _boardRectTransform;
        private PromotionHandler _promotionUI; // Reference to the promotion UI handler

        private string _defaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0";
        private PieceVisual _selectedPiece = null;

        private void Awake() 
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
            }
            else {
                Instance = this;
            }
        }

        /// <summary>
        /// Initializes the board and its associated components.
        /// </summary>
        /// <remarks>This method sets up the board by locating required components and performing any
        /// necessary initialization steps. It should be called to ensure the board is ready for use.</remarks>
        private void Start()
        {
            _promotionUI = FindObjectOfType<PromotionHandler>();
            _boardRectTransform = GetComponent<RectTransform>();

            OnBoardInitialization();
        }

        /// <summary>
        /// Initializes the chessboard by loading the specified position and setting up the board state.
        /// </summary>
        /// <remarks>If a custom position is provided via <see cref="CustomPosition"/> and is valid, it
        /// will be used to initialize the board.  Otherwise, the default position is loaded. This method also sets up
        /// the board squares, pieces, and sides.</remarks>
        public void OnBoardInitialization()
        {
            var notation = (CustomPosition == null || CustomPosition.fenNotation.Length == 0 || LoadDefaultPosition) ? _defaultPosition : CustomPosition.fenNotation;
            ChessParser.LoadFENOnBoard(notation);

            LoadBoardSquares();
            LoadBoardPieces();
            SetBoardSides();
        }

        /// <summary>
        /// Instantiates and places chess pieces on the board based on the current board state.
        /// </summary>
        /// <remarks>This method iterates through all 64 squares of the chessboard, instantiating a piece
        /// for each square that contains a non-zero value in the board state. The instantiated pieces are positioned on
        /// the corresponding board squares and configured with the appropriate sprite and value.</remarks>
        public void LoadBoardPieces()
        {
            for (int square = 0; square < 64; square++)
            {
                var pieceValue = BoardHandler.Square[square];
                if (pieceValue == 0) continue; // No piece on this square
                var piece = Instantiate(PiecePrefab, BoardSquares[square].transform.position, Quaternion.identity, transform.GetChild(1));
                piece.SetSprite(Piece.PiecesSprites[pieceValue], pieceValue);
                piece.Square = square;
                piece.Value = pieceValue;
                BoardPieces.Add(square, piece);
            }
        }

        /// <summary>
        /// Initializes and populates the chessboard with squares, alternating between light and dark colors.
        /// </summary>
        /// <remarks>This method creates an 8x8 grid of squares, assigning a color to each square based on
        /// its position. Light and dark colors alternate, starting with a dark square in the top-left corner.</remarks>
        public void LoadBoardSquares()
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    bool isLightSquare = (file + rank) % 2 != 0;
                    Color squareColor = (isLightSquare) ? BoardVisualData.lightColor : BoardVisualData.darkColor;
                    BoardSquares.Add(DrawSquare(squareColor));
                }
            }
        }

        /// <summary>
        /// Creates and returns a new square visual with the specified color.
        /// </summary>
        /// <remarks>The square is instantiated using the predefined <c>SquarePrefab</c> and is added as a
        /// child  to the first child of the current transform. Ensure that <c>SquarePrefab</c> is properly  assigned
        /// and that the transform has at least one child before calling this method.</remarks>
        /// <param name="squareColor">The color to apply to the square.</param>
        /// <returns>A <see cref="SquareVisual"/> instance representing the newly created square.</returns>
        public SquareVisual DrawSquare(Color squareColor)
        {
            SquareVisual square = Instantiate(SquarePrefab, transform.GetChild(0));
            square.SetSquareColor(squareColor);
            return square;
        }

        public bool TryMovePiece()
        {
            return true;
        }

        /// <summary>
        /// This code handles the square select and unselect part
        /// Also it handles the making move part by selecting highlighted squares
        /// </summary>
        /// <param name="squareIndex"></param>
        public void OnSquareSelect(int squareIndex)
        {
            var isActive = IsSquareHighlightActive(squareIndex);

            // Check if the square has active highlight and then make move 
            if (isActive)
            {
                // if highlight is active it means piece is selected and we can make move
                var moves = MoveGenerator.Moves[_selectedPiece.Square];
                // search through the possible move for this target square
                foreach (var m in moves)
                {
                    if (m.TargetSquare == squareIndex)
                    {
                        // Make move here 
                        // Write making move in BoardHandler code here
                        if (m.MoveFlag == Move.QueenPromotion)
                        {
                            Debug.Log($"Promotion UI");
                            _promotionUI.ShowPromotionChoices(m.StartingSquare, m.TargetSquare);
                            return; // Handle promotion logic here if needed
                        }

                        Debug.Log($"Made Move To ---> {squareIndex}");

                        OnMakeMove(m);
                    }
                }
            }
            else if (BoardHandler.Square[squareIndex] == 0 && !isActive) // if the index is empty and no highlight then we deselect the current selected piece
            {
                OnPieceUnselect();
            }
            else // if the current square is not empty then we select that piece
            {
                if (BoardPieces.ContainsKey(squareIndex)) OnPieceSelect(BoardPieces[squareIndex]);
                else Debug.LogError("No piece is present!! Check this code, its faulty or BoardHandler.Square is not updated");
            }
        }

        /// <summary>
        /// Sets the selected piece and highlights its possible moves.
        /// </summary>
        /// <param name="piece"></param>
        public void OnPieceSelect(PieceVisual piece)
        {
            if (_selectedPiece != null)
            {
                if (_selectedPiece == piece)
                {
                    OnPieceUnselect();
                    return;
                }

                if (_selectedPiece != null) OnPieceUnselect();
            }

            _selectedPiece = piece;

            var square = piece.Square;

            SetSquareColor(square, BoardVisualData.selectedColor);

            if (!Piece.IsColor(piece.Value, BoardHandler.ColorToMove)) return;

            HandleSquareHighlight(true, MoveGenerator.Moves[square]);

            return;
        }

        /// <summary>
        /// Unselects the currently selected piece, resets square colors and highlights
        /// back to their default state, and clears the piece selection.
        /// </summary>
        public void OnPieceUnselect()
        {
            if (_selectedPiece == null) return;
            int square = _selectedPiece.Square;
            bool isLightSquare = ((square % 8) + (square / 8)) % 2 != 0;
            SetSquareColor(square, isLightSquare ? BoardVisualData.lightColor : BoardVisualData.darkColor);
            if (MoveGenerator.Moves.ContainsKey(square)) HandleSquareHighlight(false, MoveGenerator.Moves[square]);
            _selectedPiece = null; // Deselect the piece
        }

        /// <summary>
        /// Handles the highlighting of squares based on the active state and the list of moves.
        /// </summary>
        /// <param name="active"></param>
        /// <param name="moves"></param>
        public void HandleSquareHighlight(bool active, List<Move> moves)
        {
            if (moves.Count == 0) return;
            foreach (var m in moves) BoardSquares[m.TargetSquare].SetHighlightActive(active);
        }

        /// <summary>
        /// Sets the color of a specific square on the board.
        /// </summary>
        /// <param name="square"></param>
        /// <param name="assignColor"></param>
        public void SetSquareColor(int square, Color assignColor)
        {
            BoardSquares[square].SetSquareColor(assignColor);
        }

        /// <summary>
        /// Updates the BoardPieces dictionary and the visual representation of the board when a move is made.
        /// </summary>
        /// <param name="move"></param>
        public void OnMakeMove(Move move)
        {
            if (BoardPieces.ContainsKey(move.TargetSquare)) // Probably we capture the opponent piece so we just disable and remove it
            {
                BoardPieces[move.TargetSquare].gameObject.SetActive(false); // Disable the captured piece
                BoardPieces.Remove(move.TargetSquare);
                Debug.Log("Captured Piece At : " + move.TargetSquare);
            }

            // enpassant handling
            if (move.MoveFlag == Move.EnPassantCapture)
            {
                // Handle en passant capture
                var lastMove = BoardHandler.MoveList.Last();
                if (BoardPieces.ContainsKey(lastMove.TargetSquare))
                {
                    BoardPieces[lastMove.TargetSquare].gameObject.SetActive(false); // Optionally deactivate the captured pawn
                    BoardPieces.Remove(lastMove.TargetSquare);
                }
                else
                {
                    Debug.LogError("En Passant Capture Error: No piece found on the expected square.");
                }
            }

            BoardPieces.Remove(move.StartingSquare);
            BoardPieces.Add(move.TargetSquare, _selectedPiece);

            // promotion handling
            var promotionFlagMask = 0b01000; // Mask for promotion flags
            if ((move.MoveFlag & promotionFlagMask) == promotionFlagMask)
            {
                // Promoted piece logic
                 Debug.Log($"Piece Promotion ---> {move.MoveFlag}");

                var newValue = Piece.Knight + move.MoveFlag - promotionFlagMask;
                newValue |= BoardHandler.ColorToMove; // Combine with color
                _selectedPiece.SetSprite(Piece.PiecesSprites[newValue], newValue);
                _selectedPiece.Value = newValue; // Update the piece value
            }

            _selectedPiece.transform.position = BoardSquares[move.TargetSquare].transform.position;

            OnPieceUnselect(); // Unselect the piece after making the move

            SetSquareColor(move.TargetSquare, BoardVisualData.targetColor);
            SetSquareColor(move.StartingSquare, BoardVisualData.selectedColor);
        }

        /// <summary>
        /// Determines whether the highlight is active for the specified square.
        /// </summary>
        /// <param name="square">The index of the square to check.</param>
        /// <returns><see langword="true"/> if the highlight is active for the specified square; otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsSquareHighlightActive(int square)
        {
            return BoardSquares[square].IsHighlightActive(); // Check if the square can be moved over (highlighted)
        }

        /// <summary>
        /// Toggles the board orientation between playing as white and playing as black.
        /// </summary>
        /// <remarks>This method switches the value of the <see cref="PlayAsWhite"/> property and updates
        /// the board sides accordingly. It is typically used to change the perspective of the board during
        /// gameplay.</remarks>
        [ContextMenu("Switch Board")]
        public void SwitchBoard()
        {
            PlayAsWhite = !PlayAsWhite;
            SetBoardSides();
        }

        public void SetBoardSides()
        {
            var markers = PlayAsWhite ? 0 : 7; // Adjust markers based on the player's color
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int squareIndex = rank * 8 + file;
                    bool isLightSquare = (file + rank) % 2 != 0;

                    var square = BoardSquares[squareIndex];

                    square.transform.position = GetSquarePosition(rank, file);

                    if (BoardPieces.ContainsKey(squareIndex)) BoardPieces[squareIndex].transform.position = square.transform.position;

                    if (rank == markers) square.ShowFile(true, ((char)('a' + file)).ToString(), (isLightSquare) ? BoardVisualData.darkColor : BoardVisualData.lightColor);
                    else square.ShowFile(false);

                    if (file == markers) square.ShowRank(true, (rank + 1).ToString(), (isLightSquare) ? BoardVisualData.darkColor : BoardVisualData.lightColor);
                    else square.ShowRank(false);
                }
            }
        }

        public Vector2 GetSquarePosition(int rank, int file)
        {
            float squareSize = 100f; // Adjust to match your prefab spacing
            float halfBoard = (8 - 1) / 2f; // = 3.5

            float x = file - halfBoard;
            float y = rank - halfBoard;

            if (!PlayAsWhite)
            {
                x = -x;
                y = -y;
            }

            var canvasSize = _boardRectTransform.sizeDelta;

            return new Vector2(x * squareSize + (canvasSize.x / 2), y * squareSize + (canvasSize.y / 2));
        }

        public Vector2 GetSquarePosition(int squareIndex)
        {
            int rank = squareIndex / 8;
            int file = squareIndex % 8;
            return GetSquarePosition(rank, file);
        }

        public int GetSquareFromPosition(Vector2 position)
        {
            float squareSize = 100f; // same as in GetSquarePosition
            float halfBoard = (8 - 1) / 2f; // = 3.5

            var canvasSize = _boardRectTransform.sizeDelta;
            position -= new Vector2(canvasSize.x / 2, canvasSize.y / 2);

            float x = position.x / squareSize;
            float y = position.y / squareSize;

            if (!PlayAsWhite)
            {
                x = -x;
                y = -y;
            }

            int file = Mathf.RoundToInt(x + halfBoard);
            int rank = Mathf.RoundToInt(y + halfBoard);

            if (file < 0 || file > 7 || rank < 0 || rank > 7) return -1; // invalid square

            return rank * 8 + file; // square index 0..63
        }
    }
}
