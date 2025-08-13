using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct Move
{
    // Move data packed into 16 bit value
    // The format is as follows: FFFFTTTTTTSSSSSS
    // (where F = move Flag, T = target square, S = starting square)
    readonly ushort moveValue;

    // Flag
    public const int NoFlag = 0b00000; // 0
    public const int DoublePush = 0b00001; // 1
    public const int KingCastle = 0b00010; // 2
    public const int QueenCastle = 0b00011; // 3
    public const int Capture = 0b00100; // 4
    public const int EnPassantCapture = 0b00101; // 5
    public const int KnightPromotion = 0b01000; // 8
    public const int BishopPromotion = 0b01001; // 9
    public const int RookPromotion = 0b01010; // 10
    public const int QueenPromotion = 0b01011; // 11
    public const int KnightPromotionCapture = 0b01100; // 12
    public const int BishopPromotionCapture = 0b01101; // 13
    public const int RookPromotionCapture = 0b01110; // 14
    public const int QueenPromotionCapture = 0b01111; // 15

    // Create move
    public Move(int startingSquare, int targetSquare, int flag)
    {
        moveValue = (ushort)(startingSquare | targetSquare << 6 | flag << 12);
    }

    // Extract move info value from movevalue
    public int StartingSquare => moveValue & 0b0000000000111111;
    public int TargetSquare => (moveValue & 0b0000111111000000) >> 6;
    public int MoveFlag => moveValue >> 12;
}
