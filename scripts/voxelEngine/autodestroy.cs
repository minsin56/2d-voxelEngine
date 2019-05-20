using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autodestroy : MonoBehaviour 
{
    public float time = 3.0f;
	void Start () 
	{
        Destroy(gameObject, time);
	}
	
	void Update () 
	{
		
	}
}
