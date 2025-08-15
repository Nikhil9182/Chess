using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public GraphicalBoard BoardVisuals;
    public PromotionHandler PromotionHandlerUI;

    public BoardPositionInFen CustomPosition;

    public BoardSquare SquarePrefab;
    public BoardPiece PiecePrefab;

    public bool LoadDefaultPosition = true;
    public bool PlayAsWhite = true; // If true, white pieces are at the bottom of the board

    private List<BoardSquare> BoardSquares = new List<BoardSquare>(); // List of squares on the board
    private Dictionary<int, BoardPiece> BoardPieces = new Dictionary<int, BoardPiece>(); // Dictionary to hold pieces by their square index
    private RectTransform _boardRectTransform;

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
        Board.InitializeBoard(notation, SquarePrefab, PiecePrefab, BoardVisuals, transform, PlayAsWhite, out BoardSquares, out BoardPieces);

        SetBoardSides();
    }

    //public bool TryMovePiece(, int )
    //{
    //    return true;
    //}

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

        if (!Piece.IsColor(piece.Value, Board.ColorToMove)) return;

        HandleSquareHighlight(true, MoveGenerator.Moves[square]);

        return;
    }

    /// <summary>
    /// Unselects the currently selected piece, resets square colors and highlights
    /// back to their default state, and clears the piece selection.
    /// </summary>
    public void OnPieceUnselect()
    {
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

        if (Board.ColorToMove == Piece.White)
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
