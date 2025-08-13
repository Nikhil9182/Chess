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

    private string DefaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0";

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
        //Load Board
        var notation = (CustomPosition == null || CustomPosition.fenNotation.Length == 0 || LoadDefaultPosition) ? DefaultPosition : CustomPosition.fenNotation;
        Board.InitializeBoard(notation, SquarePrefab, PiecePrefab, BoardVisuals, transform, PlayAsWhite);
    }

    public void ResetSquares(bool resetColors, bool resetHighlights)
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                if (resetHighlights)
                {
                    Board.Square[rank * 8 + file].SetHighlightActive(false);
                }
                if (resetColors)
                {
                    bool isLightSquare = (file + rank) % 2 != 0;
                    Board.Square[rank * 8 + file].SetSquareColor((isLightSquare) ? BoardVisuals.lightColor : BoardVisuals.darkColor);
                }
            }
        }
    }

    public void SetMoves(List<Move> moves, BoardPiece boardPiece)
    {
        foreach (var move in moves)
        {
            Board.Square[move.TargetSquare].SetHighlightActive(true);
            Board.Square[move.TargetSquare].SetMoveOnClick(move, boardPiece);
        }
    }

    public void SetColor(int square, Color assignColor)
    {
        Board.Square[square].SetSquareColor(assignColor);
    }

    [ContextMenu("Switch Board")]
    public void SwitchBoard()
    {
        Board.SwitchSides();
        Board.SetSides(BoardVisuals);
    }
}
