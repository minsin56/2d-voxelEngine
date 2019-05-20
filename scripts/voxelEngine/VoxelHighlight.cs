using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class VoxelHighlight : MonoBehaviour 
{
    public Texture2D atlas;
    private MeshRenderer mr;
    private MeshFilter mf;
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> indices = new List<int>();
    int vert_index = 0;
	void Start () 
	{
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        
    }
	void Update () 
	{
		
	}
}
