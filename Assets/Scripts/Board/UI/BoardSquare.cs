using Chess.Board.Core;
using Chess.Board.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chess.Board.UI
{
    public class BoardSquare : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private Image img;
        [SerializeField]
        private Image highlightImg; // Optional highlight image for visual effects
        [SerializeField]
        private TextMeshProUGUI RankTMP, FileTMP;

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
        }

        public bool IsHighlightActive()
        {
            //Debug.Log($"IsHighlightActive: {highlightImg.enabled}");
            return highlightImg.enabled;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var square = BoardManager.Instance.GetSquareFromPosition(eventData.position);
            BoardManager.Instance.OnSquareSelect(square);
        }
    }
}
