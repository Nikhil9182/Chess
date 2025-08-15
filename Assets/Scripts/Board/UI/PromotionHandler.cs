using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Chess.Pieces;
using Chess.Board.Core;

namespace Chess.Board.UI
{
    public class PromotionHandler : MonoBehaviour
    {
        [SerializeField]
        private List<Button> promotionButtons = new List<Button>();

        public void ShowPromotionChoices(UnityAction promotionAction)
        {
            transform.GetChild(0).gameObject.SetActive(true);

            int[] pieces = new int[] { Piece.Queen, Piece.Rook, Piece.Bishop, Piece.Knight };

            for (int i = 0; i < promotionButtons.Count; i++)
            {
                var button = promotionButtons[i];
                button.onClick.RemoveAllListeners(); // Clear previous listeners

                var image = button.transform.GetChild(0).GetComponent<Image>();

                var pieceValue = pieces[i] | BoardHandler.ColorToMove; // Combine piece type with color
                image.sprite = Piece.PiecesSprites[pieceValue];
                image.SetNativeSize(); // Adjust the size of the image to fit

                button.onClick.AddListener(() => 
                {
                    // Set the piece type for promotion
                    //Board.PromotedPiece = pieceValue; // Store the promoted piece value
                    // Hide the promotion choices after selection
                    promotionAction.Invoke(); // Invoke the action passed to ShowPromotionChoices

                    HidePromotionChoices();
                });
            }
        }

        public void HidePromotionChoices()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}

