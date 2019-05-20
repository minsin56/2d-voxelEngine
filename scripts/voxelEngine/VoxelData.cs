using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static Vector3[] vertices =
    {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(0,1,0),
        new Vector3(1,1,0)
    };
    public static Vector2[] uvs =
    {
        new Vector2(0,0),
        new Vector2(1,0),
        new Vector2(0,1),
        new Vector2(1,1)
    };
    public static int[] indices =
    {
        0,2,1,
        2,3,1
    };
}
