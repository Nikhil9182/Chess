using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public GraphicalBoard Graphic;
    public Transform BTransform;

    public BoardPositionInFen CustomPosition;

    public BoardSquare SquarePrefab;
    public BoardPiece PiecePrefab;

    public bool LoadDefaultPosition = true;

    private string DefaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

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
        //Graphical Setup
        Graphic.CreateGraphicalBoard(BTransform.transform.GetChild(0), SquarePrefab);

        //Load Position
        var notation = (CustomPosition == null || CustomPosition.fenNotation.Length == 0 || LoadDefaultPosition) ? DefaultPosition : CustomPosition.fenNotation;
        Board.LoadBoardPosition(notation, PiecePrefab, BTransform.GetChild(1));
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
                    Board.Square[rank * 8 + file].SetSquareColor((isLightSquare) ? Graphic.lightColor : Graphic.darkColor);
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
}
