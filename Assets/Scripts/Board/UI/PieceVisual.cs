using Chess.Board.Core;
using Chess.Board.Managers;
using Chess.Pieces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Chess.Board.UI
{
    public class PieceVisual : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
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
            if (BoardVisualsManager.Instance.IsSquareHighlightActive(Square))
            {
                BoardVisualsManager.Instance.OnSquareSelect(Square);
                return;
            }

            BoardVisualsManager.Instance.OnPieceSelect(this); // Notify the board manager that this piece was selected
            transform.SetAsLastSibling(); // Bring the piece to the front while dragging
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (BoardVisualsManager.Instance.IsSquareHighlightActive(Square)) return;

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
            if (BoardVisualsManager.Instance.IsSquareHighlightActive(Square)) return;

            if (Piece.IsColor(Value, BoardHandler.ColorToMove))
            {
                var lastSquare = BoardVisualsManager.Instance.GetSquareFromPosition(eventData.position);

                if (lastSquare != Square && lastSquare != -1 && BoardVisualsManager.Instance.IsSquareHighlightActive(lastSquare))
                {
                    // Make move here
                    // Write move code here
                }
            }

            transform.position = BoardVisualsManager.Instance.GetSquarePosition(Square); // Reset position if not dropped on a square
        }

        public void SetSprite(Sprite sprite, int value)
        {
            _image.sprite = sprite;
            _image.SetNativeSize();
            gameObject.name = sprite.name;
            Value = value; // Set the piece value
        }

        internal void OnTurnChanged()
        {
            _image.raycastTarget = Piece.IsColor(Value, BoardHandler.ColorToMove);
        }
    }
}
