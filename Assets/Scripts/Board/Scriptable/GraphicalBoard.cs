using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class GraphicalBoard : ScriptableObject
{
    public Color lightColor;
    public Color darkColor;
    public Color selectedColor;
    public Color targetColor;

    public void CreateGraphicalBoard(Transform boardPlaceContainer, BoardSquare squarePrefab)
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                bool isLightSquare = (file + rank) % 2 != 0;
                Color squareColor = (isLightSquare) ? lightColor : darkColor;
                Vector2 position = new Vector2(-3.5f + file, -3.5f + rank);
                var square = DrawSquare(squareColor, position, boardPlaceContainer, squarePrefab);
                if (rank == 0)
                {
                    square.ShowFile(((char)('a' + file)).ToString(), (isLightSquare) ? darkColor : lightColor);
                }
                if (file == 0)
                {
                    square.ShowRank((rank + 1).ToString(), (isLightSquare) ? darkColor : lightColor);
                }
                Board.Square[rank * 8 + file] = square;
            }
        }
    }

    public BoardSquare DrawSquare(Color squareColor, Vector2 position, Transform boardPlaceContainer, BoardSquare squarePrefab)
    {
        BoardSquare square = Instantiate(squarePrefab, boardPlaceContainer);
        square.gameObject.transform.position = position;
        square.SetSquareColor(squareColor);
        return square;
    }
}
