using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPiece : MonoBehaviour
{
    public int Value;
    public int Square; // Square index (0-63)

    public Sprite Sprite { get { return spriteRenderer.sprite; } set { spriteRenderer.sprite = value; } }

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        if (!Piece.IsColor(Value, Board.ColorToMove)) return;

        spriteRenderer.sortingOrder = 2;

        List<Move> moves = new();

        if (Piece.IsSlidingPiece(Value))
            moves = Moves.GenerateSlidingPieceMoves(Value, Square);
        if (Piece.IsType(Value, Piece.Knight))
            moves = Moves.GenerateKnightMoves(Value, Square);
        if (Piece.IsType(Value, Piece.Pawn))
            moves = Moves.GeneratePawnMoves(Value, Square);
        if (Piece.IsType(Value, Piece.King))
            moves = Moves.GenerateKingMoves(Value, Square);

        BoardManager.Instance.ResetSquares(true, true);
        BoardManager.Instance.SetMoves(moves, this);
        BoardManager.Instance.SetColor(Square, BoardManager.Instance.Graphic.selectedColor);
    }

    //private void OnMouseDrag()
    //{
    //    if (!Piece.IsColor(Value, Board.ColorToMove)) return;
    //    transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //}

    //private void OnMouseUp() 
    //{
    //    if (!Piece.IsColor(Value, Board.ColorToMove)) return;

    //    spriteRenderer.sortingOrder = 1;

    //    Vector2 position = transform.position;
    //    position = new Vector2(Mathf.Floor(position.x) + 0.5f, Mathf.Floor(position.y) + 0.5f);

    //    if (position.x < -3.5f || position.x > 3.5f || position.y < -3.5f || position.y > 3.5f) 
    //    {
    //        transform.position = initialPosition;
    //    }
    //    else 
    //    {
    //        int newfile = (int) (position.x + 3.5f);
    //        int newrank = (int) (position.y + 3.5f);

    //        transform.position = position;

    //        int newSquare = newrank * 8 + newfile;

    //        if (newSquare == Square) return;

    //        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.4f);
    //        if (colliders != null) 
    //        {
    //            foreach (Collider2D col in colliders) 
    //            {
    //                if (col.gameObject == gameObject) continue; // Skip the piece itself
    //                if (col.TryGetComponent(out BoardSquare square)) 
    //                {
    //                    square.OnSquareSelected();
    //                    BoardManager.Instance.ResetSquares(false, true);
    //                    BoardManager.Instance.SetColor(Square, BoardManager.Instance.Graphic.targetColor);
    //                    return; // Exit after handling the first valid square
    //                }
    //            }
    //        }

    //        transform.position = initialPosition;
    //    }
    //}
}
