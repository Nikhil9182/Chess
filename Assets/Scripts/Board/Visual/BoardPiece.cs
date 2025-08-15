using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BoardPiece : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public int Value;
    public int Square; // Square index (0-63)

    private Image _image;

    private void Awake()
    {
        if (_image == null) _image = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        BoardManager.Instance.OnPieceSelect(this); // Notify the board manager that this piece was selected
        transform.SetAsLastSibling(); // Bring the piece to the front while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPos
        );
        transform.position = worldPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Piece.IsColor(Value, Board.ColorToMove))
        {
            var lastSquare = BoardManager.Instance.GetSquareFromPosition(eventData.position);

            if (lastSquare != Square && lastSquare != -1)
            {
                //bool canMove = BoardManager.Instance.TryMovePiece(Square, lastSquare);
            }
        }

        transform.position = BoardManager.Instance.GetSquarePosition(Square); // Reset position if not dropped on a square
    }

    public void SetSprite(Sprite sprite)
    {
        _image.sprite = sprite;
        _image.SetNativeSize();
    }

    internal void OnTurnChanged()
    {
        _image.raycastTarget = Piece.IsColor(Value, Board.ColorToMove);
    }
}
