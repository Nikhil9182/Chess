using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardSquare : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private Image img;
    [SerializeField]
    private Image highlightImg; // Optional highlight image for visual effects
    [SerializeField]
    private TextMeshProUGUI RankTMP, FileTMP;

    public BoardPiece Piece; // Reference to the piece on this square, if any

    public int Value { get { return Piece != null ? Piece.Value : 0; } } // Returns the value of the piece on this square, or 0 if no piece

    public Action MoveOnClicked; // Action to perform when the square is clicked

    private void Awake()
    {
        if (img == null)
        {
            img = GetComponent<Image>();
        }

        SetHighlightActive(false);

        RankTMP.gameObject.SetActive(false);
        FileTMP.gameObject.SetActive(false);
    }

    public void OnSquareSelected()
    {
        if (MoveOnClicked != null)
        {
            MoveOnClicked.Invoke();
        }

        MoveOnClicked = null; // Reset the action after invoking it
    }

    public void SetSquareColor(Color mainColor)
    {
        img.color = mainColor;
    }

    public void ShowRank(bool enable, string rank = null, Color color = new())
    {
        RankTMP.gameObject.SetActive(enable);
        RankTMP.text = rank;
        RankTMP.color = color;
    }

    public void ShowFile(bool enable, string file = null, Color color = new())
    {
        FileTMP.gameObject.SetActive(enable);
        FileTMP.text = file;
        FileTMP.color = color;
    }

    public void SetHighlightActive(bool enable)
    {
        highlightImg.enabled = enable; // Enable or disable the highlight image
        MoveOnClicked = null; // Reset the action when highlight is disabled
    }

    public void SetMoveOnClick(Move move, BoardPiece boardPiece)
    {
        Action moveAction = () =>
        {
            Board.MakeMove(move);
            BoardManager.Instance.ResetSquares(false, true);
            BoardManager.Instance.SetColor(move.TargetSquare, BoardManager.Instance.BoardVisuals.targetColor);
        };

        if (move.MoveFlag == Move.QueenPromotion)
        {
            MoveOnClicked = () =>
            {
                BoardManager.Instance.PromotionHandlerUI.ShowPromotionChoices(new UnityEngine.Events.UnityAction(moveAction)); // Set the promotion action
            };
        }
        else
        {
            MoveOnClicked = moveAction; // Set the action to be performed when this square is clicked
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnSquareSelected();
    }
}
