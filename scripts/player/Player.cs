using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    public float health = 100.0f;
    public float walkspeed = 1.0f;
    public float runspeed = 2.0f;
    public float jumpspeed = 4.0f;
    public float zoomSensitivity = 2.0f;
    public VoxelHighlight voxelHighlight;
    public Chunk chunk;
    [Range(1,5)]
    public int v;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private static float min_camsize = 2.0f;
    private static float max_camsize = 30.0f;
    private Camera cam;
    private bool grounded { get { return Physics2D.Raycast(transform.position, Vector3.down, 0.1f); } }
    private Vector2 selectedVoxelPos = new Vector2();
    
    public static Player findPlayer()
    {
        return GameObject.FindWithTag("Player").GetComponent<Player>();
    }
	void Start () 
	{
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        cam = Camera.main;
        Cursor.visible = false;
	}
	void Update () 
	{
        movement();
        float scrollwheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollwheel > 0.0f || scrollwheel < 0.0f)
        {
            cam.orthographicSize -= scrollwheel * zoomSensitivity;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, min_camsize, max_camsize);
        }
        worldInteraction();
    }
    void movement()
    {
        float x = walkspeed * Input.GetAxis("Horizontal");
        rb.AddForce(Vector2.right * x, ForceMode2D.Impulse);

        if (grounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(Vector2.up * jumpspeed, ForceMode2D.Impulse);
            }
        }
        // limits the x velocity of the player
        Vector2 tmp;
        tmp.x = Mathf.Clamp(rb.velocity.x, -walkspeed, walkspeed);
        tmp.y = rb.velocity.y;
        rb.velocity = tmp;
    }
    void worldInteraction()
    {
        Vector3 mpos = cam.ScreenToWorldPoint(Input.mousePosition, cam.stereoActiveEye);
        selectedVoxelPos = new Vector2(Mathf.RoundToInt(mpos.x), Mathf.RoundToInt(mpos.y)) + Chunk.voxel_col_offset;

        voxelHighlight.transform.position = new Vector3(selectedVoxelPos.x, selectedVoxelPos.y, 0);

        if (Input.GetMouseButton(0))
        {
            chunk.setVoxel((int)selectedVoxelPos.x, (int)selectedVoxelPos.y, 0);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            chunk.explodeVoxels((int)selectedVoxelPos.x, (int)selectedVoxelPos.y, 5);
        }
        else if (Input.GetMouseButton(1) && chunk.getvoxelidfromvector2(selectedVoxelPos) == 0)
        {
            chunk.setVoxel((int)selectedVoxelPos.x, (int)selectedVoxelPos.y, v);
        }
    }
}
