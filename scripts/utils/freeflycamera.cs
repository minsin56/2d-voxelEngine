using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class freeflycamera : MonoBehaviour 
{
    public float movespeed;
	
	void Update () 
	{
        float x = movespeed * Input.GetAxis("Horizontal");
        float y = movespeed * Input.GetAxis("Vertical");

        transform.position += new Vector3(x, y,0);
	}
}
