using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.Board.ScriptableObjects
{
    [CreateAssetMenu]
    public class GraphicalBoard : ScriptableObject
    {
        public Color lightColor;
        public Color darkColor;
        public Color selectedColor;
        public Color targetColor;
    }
}
