using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardSquare : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRen;
    [SerializeField]
    private TextMeshPro RankTMP, FileTMP;

    public BoardPiece Piece; // Reference to the piece on this square, if any

    public int Value { get { return Piece != null ? Piece.Value : 0; } } // Returns the value of the piece on this square, or 0 if no piece

    Action MoveOnClicked; // Action to perform when the square is clicked

    private BoxCollider2D col;

    private void Awake()
    {
        if (spriteRen == null)
        {
            spriteRen = GetComponent<SpriteRenderer>();
        }

        col = GetComponent<BoxCollider2D>();
        col.enabled = false; // Disable the collider by default

        SetHighlightActive(false);

        RankTMP.gameObject.SetActive(false);
        FileTMP.gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        OnSquareSelected();
    }

    public void OnSquareSelected()
    {
        if (MoveOnClicked != null)
        {
            MoveOnClicked.Invoke();
        }
    }

    public void SetSquareColor(Color mainColor)
    {
        spriteRen.material.SetColor("_MainColor", mainColor);
    }

    public void ShowRank(string rank, Color color)
    {
        RankTMP.text = rank;
        RankTMP.color = color;
        RankTMP.gameObject.SetActive(true);
    }

    public void ShowFile(string file, Color color)
    {
        FileTMP.text = file;
        FileTMP.color = color;
        FileTMP.gameObject.SetActive(true);
    }

    public void SetHighlightActive(bool enable)
    {
        spriteRen.material.SetFloat("_HighlightSize", enable ? 0.3f : 0f);
        col.enabled = enable; // Enable or disable the collider based on highlight state
        if (enable) MoveOnClicked = null; // Reset the action when highlight is disabled
    }

    public void SetMoveOnClick(Move move, BoardPiece boardPiece)
    {
        MoveOnClicked = () => 
        {
            Board.MakeMove(move);
            BoardManager.Instance.ResetSquares(false, true);
            BoardManager.Instance.SetColor(move.TargetSquare, BoardManager.Instance.Graphic.targetColor);
            boardPiece.transform.position = transform.position; // Move the piece to the target square
            boardPiece.Square = move.TargetSquare; // Update the square index in the piece
            boardPiece.Value |= 32; // Set the piece as moved (assuming 32 is the bit for moved)
        };
    }
}
