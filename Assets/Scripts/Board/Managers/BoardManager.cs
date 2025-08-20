using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Board.ScriptableObjects;
using Chess.Board.Core;
using Chess.Board.UI;
using Chess.Pieces;

namespace Chess.Board.Managers
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance;

        public GraphicalBoard BoardVisuals;
        public BoardPositionInFen CustomPosition;
        public BoardSquare SquarePrefab;
        public BoardPiece PiecePrefab;
        public Canvas BoardCanvas; // Canvas that holds the board

        public bool LoadDefaultPosition = true;
        public bool PlayAsWhite = true; // If true, white pieces are at the bottom of the board

        private List<BoardSquare> BoardSquares = new List<BoardSquare>(); // List of squares on the board
        private Dictionary<int, BoardPiece> BoardPieces = new Dictionary<int, BoardPiece>(); // Dictionary to hold pieces by their square index
        private RectTransform _boardRectTransform;
        private PromotionHandler _promotionUI; // Reference to the promotion UI handler

        private string _defaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0";
        private BoardPiece _selectedPiece = null;

        private void Awake() 
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
            }
            else {
                Instance = this;
            }
        }

        private void Start()
        {
            _promotionUI = FindObjectOfType<PromotionHandler>();
            _boardRectTransform = GetComponent<RectTransform>();
            OnBoardInitialization();
        }

        /// <summary>
        /// Initializes the board with the default or custom position.
        /// Also sets the board sides based on the player's color.
        /// </summary>
        public void OnBoardInitialization()
        {
            //Load Board
            var notation = (CustomPosition == null || CustomPosition.fenNotation.Length == 0 || LoadDefaultPosition) ? _defaultPosition : CustomPosition.fenNotation;
            BoardHandler.InitializeBoard(notation, SquarePrefab, PiecePrefab, BoardVisuals, transform, PlayAsWhite, out BoardSquares, out BoardPieces);

            SetBoardSides();
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
        public void OnPieceSelect(BoardPiece piece)
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

            SetSquareColor(square, BoardVisuals.selectedColor);

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
            SetSquareColor(square, isLightSquare ? BoardVisuals.lightColor : BoardVisuals.darkColor);
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
                BoardPieces[move.TargetSquare].gameObject.SetActive(false);
                BoardPieces.Remove(move.TargetSquare);
                Debug.Log("Captured Piece At : " + move.TargetSquare);
            }

            BoardPieces.Remove(move.StartingSquare);
            BoardPieces.Add(move.TargetSquare, _selectedPiece);

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

            SetSquareColor(move.TargetSquare, BoardVisuals.targetColor);
            SetSquareColor(move.StartingSquare, BoardVisuals.selectedColor);
        }

        public bool IsSquareHighlightActive(int square)
        {
            return BoardSquares[square].IsHighlightActive(); // Check if the square can be moved over (highlighted)
        }

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

                    if (rank == markers) square.ShowFile(true, ((char)('a' + file)).ToString(), (isLightSquare) ? BoardVisuals.darkColor : BoardVisuals.lightColor);
                    else square.ShowFile(false);

                    if (file == markers) square.ShowRank(true, (rank + 1).ToString(), (isLightSquare) ? BoardVisuals.darkColor : BoardVisuals.lightColor);
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
