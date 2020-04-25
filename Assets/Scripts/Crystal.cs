﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrystalType
{
    White = 0,
    Black = 1,
    Red = 2,
    Green = 3,
    Blue = 4,
    Yellow = 5,
}

public class Crystal : MonoBehaviour
{
    public int x = 0;
    public int y = 0;
    public CrystalType crystalType = 0;

    private Field field;
}