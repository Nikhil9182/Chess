using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BoardPiece : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Value;
    public int Square; // Square index (0-63)

    public Sprite Sprite { get { return image.sprite; } set { image.sprite = value; image.SetNativeSize(); } }

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Piece.IsColor(Value, Board.ColorToMove)) return;

        List<Move> moves = new();

        if (Piece.IsSlidingPiece(Value))
            moves = Moves.GenerateSlidingPieceMoves(Value, Square);
        if (Piece.IsType(Value, Piece.Knight))
            moves = Moves.GenerateKnightMoves(Value, Square);
        if (Piece.IsType(Value, Piece.Pawn))
            moves = Moves.GeneratePawnMoves(Value, Square);
        if (Piece.IsType(Value, Piece.King))
            moves = Moves.GenerateKingMoves(Value, Square);

        //foreach (Move move in moves)
        //{
        //    Debug.Log($"Move: {move.StartingSquare} -> {move.TargetSquare}");
        //}

        BoardManager.Instance.ResetSquares(true, true);
        BoardManager.Instance.SetMoves(moves, this);
        BoardManager.Instance.SetColor(Square, BoardManager.Instance.BoardVisuals.selectedColor);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsDraggable()) return;
        transform.SetAsLastSibling(); // Bring the piece to the front while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDraggable()) return;

        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPos
        );
        transform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsDraggable()) return;

        // Raycast to see if dropped on a square
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            var square = r.gameObject.GetComponent<BoardSquare>();
            if (square != null && square.MoveOnClicked != null)
            {
                square.OnSquareSelected(); // Trigger the square's click action
                return;
            }
        }

        transform.position = Board.Square[Square].transform.position; // Reset position if not dropped on a square
    }

    private bool IsDraggable()
    {
        return Piece.IsColor(Value, Board.ColorToMove);
    }

    internal void OnTurnChanged()
    {
        image.raycastTarget = Piece.IsColor(Value, Board.ColorToMove);
    }
}
