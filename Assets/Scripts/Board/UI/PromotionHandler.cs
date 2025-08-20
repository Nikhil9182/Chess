using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Chess.Pieces;
using Chess.Board.Core;
using Chess.Board.Managers;

namespace Chess.Board.UI
{
    public class PromotionHandler : MonoBehaviour
    {
        [SerializeField]
        private List<Button> promotionButtons = new List<Button>();

        public void ShowPromotionChoices(int starting, int target)
        {
            SetActive(true);

            for (int i = 0; i < promotionButtons.Count; i++)
            {
                var button = promotionButtons[i];

                button.onClick.RemoveAllListeners(); // Clear previous listeners
                var image = button.transform.GetChild(0).GetComponent<Image>();

                var value = (Piece.Knight + i) | BoardHandler.ColorToMove; // Knight, Bishop, Rook, Queen
                image.sprite = Piece.PiecesSprites[value];
                image.SetNativeSize(); // Adjust the size of the image to fit
                var index = i; // Capture the current index for the listener

                button.onClick.AddListener(() => 
                {
                    BoardManager.Instance.OnMakeMove(new Move(starting, target,  Move.KnightPromotion + index));
                    SetActive(false); // Hide the promotion choices after selection
                });
            }
        }

        public void SetActive(bool active)
        {
            transform.GetChild(0).gameObject.SetActive(active);
        }
    }
}

