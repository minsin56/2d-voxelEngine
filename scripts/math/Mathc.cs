using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mathc 
{
    public static float perlinNoise(float x,float y, float offset, float scale)
    {
        return Mathf.PerlinNoise((x + 0.1f) / Chunk.chunk_width * scale + offset, (y + 0.1f) / Chunk.chunk_width * scale + offset);
    }
}
