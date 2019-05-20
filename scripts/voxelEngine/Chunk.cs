using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using utils;
using urand = UnityEngine.Random;

public class Chunk : MonoBehaviour
{
    private MeshRenderer mr;
    private MeshFilter mf;
    private PolygonCollider2D col;
    public static readonly int chunk_width = 64;
    public static readonly int chunk_height = 64;
    public static readonly Vector2 voxel_col_offset = new Vector2(0.51f, 0.50f);

    public int[,] voxelmap = new int[chunk_width, chunk_height];
    public float noise_power = 0.15f;
    public Vector2 coloffset;

    private float[,] mass = new float[chunk_width + 2, chunk_height + 2], new_mass = new float[chunk_width + 2, chunk_height + 2];
    private Mesh m;

    private int vert_index = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> indices = new List<int>();

    private bool updatingvoxels = false;
    private bool updatingwater = false;


    public Vector2 voffset;
    public static readonly int atlas_width = 512;
    public static readonly int atlas_height = 512;

    public static readonly int voxel_width = 16;
    public static readonly int voxel_height = 16;

    private BoxCollider2D[,] colliders = new BoxCollider2D[chunk_width, chunk_height];

    readonly float maxMass = 1.0f;
    readonly float maxCompress = 0.02f;
    readonly float minMass = 0.005f;
    readonly float minFlow = 0.005f;
    float maxSpeed = 1.0f;

    VoxelDatabase db;
    void Start()
    {
        db = VoxelDatabase.loadVoxelTypes();

        populateVoxelMap();
        generateMesh();
        m = new Mesh();
        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();


        m.vertices = vertices.ToArray();
        m.triangles = indices.ToArray();
        m.RecalculateNormals();

        mf.mesh = m;

        transform.localScale = new Vector3(1, 1, -1);

        c_setvoxel(0, 0, voxelmap[0, 0]);


        explosion = (GameObject)Resources.Load("particles/explosion");
    }
    void Update()
    {
        updateboxcolliders();

        //if (!updatingwater)
        //{
        //    StartCoroutine(updateWater());
        //}
        if (!updatingvoxels)
        {
            StartCoroutine(updatevoxels());
        }
    }
    int perlin(int x, int y, float power)
    {
        return (Mathf.FloorToInt(Mathf.PerlinNoise(x, y) * power));
    }
    float getstablestate(float tmass)
    {
        if (tmass <= 1) { return 1.0f; }
        else if(tmass < 2 * maxMass + maxCompress)
        {
            return maxMass * maxMass + tmass * maxCompress/(maxMass+maxCompress);
        }
        else
        {
            return (tmass + maxCompress) / 2;
        }
    }
    bool updatechange = false;
    IEnumerator updateWater()
    {
        updatingwater = true;
        resetMesh();
        float flow = 0, remainingMass = 0;


        for(int x = 0; x < chunk_width; x++)
        {
            for (int y = 0; y < chunk_height; y++)
            {

                if (db.voxelTypes[voxelmap[x, y]].solid && !db.voxelTypes[voxelmap[x, y]].liquid) { continue; }

                flow = 0;
                remainingMass = mass[x, y];
                if (remainingMass <= 0) { continue; }

                //below
                if (!db.voxelTypes[voxelmap[x, y-1]].solid)
                {
                    flow = getstablestate(remainingMass + mass[x, y - 1]);
                    if (flow > minFlow) { flow *= 0.5f; }

                    flow = Mathf.Clamp(flow, 0, Mathf.Min(maxSpeed, remainingMass));
                    new_mass[x, y] -= flow;
                    new_mass[x, y - 1] += flow;
                    remainingMass -= flow;
                }

                if (remainingMass <= 0) { continue; }

                //left
                if (x > 0 && !db.voxelTypes[voxelmap[x - 1, y]].solid)
                {
                    flow = (mass[x, y] - mass[x - 1, y]) / 4;
                    if (flow > minFlow) { flow *= 0.5f; }
                    flow = Mathf.Clamp(flow, 0, remainingMass);
                    new_mass[x, y] -= flow;
                    new_mass[x - 1, y] += flow;
                    remainingMass -= flow;
                }

                if (remainingMass <= 0) { continue; }

                //right
                if (x < chunk_width-1 && !db.voxelTypes[voxelmap[x + 1, y]].solid)
                {
                    flow = (mass[x, y] - mass[x + 1, y]) / 4;
                    if (flow > minFlow) { flow *= 0.5f; }
                    flow = Mathf.Clamp(flow, 0, remainingMass);

                    new_mass[x, y] -= flow;
                    new_mass[x + 1, y] += flow;
                    remainingMass -= flow;
                }

                if (remainingMass <= 0) { continue; }

                //up
                if(y < chunk_height -1 && !db.voxelTypes[voxelmap[x, y + 1]].solid)
                {
                    flow = remainingMass - getstablestate(remainingMass + mass[x, y + 1]);
                    if (flow > minFlow) { flow *= 0.5f; }
                    flow = Mathf.Clamp(flow, 0, Mathf.Min(maxSpeed, remainingMass));

                    new_mass[x, y] -= flow;
                    new_mass[x, y + 1] += flow;
                    remainingMass -= flow;
                }

            }
        }
        for (int x = 0; x < chunk_width + 2; x++)
        {
            for (int y = 0; y < chunk_height + 2; y++)
            {
                mass[x, y] = new_mass[x, y];
            }
        }
        for(int x = 0; x < chunk_width; x++)
        {
            for(int y = 0; y < chunk_height; y++)
            {
                if (db.voxelTypes[voxelmap[x, y]].solid) { continue; }
                if (new_mass[x, y] > minMass)
                {
                    voxelmap[x, y] = 4;
                }
                else
                {
                    voxelmap[x, y] = 0;
                }

            }
        }

        for(int x = 0; x < chunk_width + 2; x++)
        {
            mass[x, 0] = 0;
            mass[x, chunk_height + 1] = 0;
        }
        for(int y = 1; y < chunk_height + 1; y++)
        {
            mass[0, y] = 0;
            mass[chunk_width + 1, y] = 0;
        }
        yield return null;
        regen();
        updatingwater = false;
    }
    // addition 1
    IEnumerator updatevoxels()
    {
        updatingvoxels = true;
        for(int x = 0; x < chunk_width; x++)
        {
            for(int y = 0; y < chunk_height; y++)
            {
                if (db.voxelTypes[voxelmap[x, y]].placeoncvoxel && (db.voxelTypes[voxelmap[x, y - 1]].placeoncvoxel || db.voxelTypes[voxelmap[x, y - 1]].liquid || voxelmap[x, y - 1] == 0))
                {
                    voxelmap[x, y] = 0;

                    updatechange = true;
                }
                else
                {
                    updatechange = false;
                }
            }
        }
        if (updatechange)
        {
            resetMesh();
            regen();
        }
        updatingvoxels = false;
        yield return null;
    }
    ///////////////////////
    void populateVoxelMap()
    {
        for (int x = 0; x < chunk_width; x++)
        {
            for (int y = 0; y < chunk_height; y++)
            {
                // uses perlin noise to generate the height of the terrain 
                // i hate putting lots of comments
                int height = Mathf.FloorToInt(chunk_height * Mathc.perlinNoise(x, y, 0, noise_power)) + 10;

                // starts from top to bottom

                //air
                if (y > height)
                {
                    voxelmap[x, y] = 0;
                }
                //grass
                else if (y == height)
                {
                    voxelmap[x, y] = 2;
                }
                //dirt
                else if (y < height && y > height - 4)
                {
                    voxelmap[x, y] = 1;
                }
                // cobblestone
                else
                {
                    voxelmap[x, y] = 3;
                }

            }
        }
    }
    void resetMesh()
    {
        // clears vertices,texture coords,and indices
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        indices = new List<int>();

        //resets the mesh
        m = new Mesh();
        vert_index = 0;
    }
    public void fullregen()
    {
        resetMesh();
        //resets the voxelmap
        voxelmap = new int[chunk_width, chunk_height];
        populateVoxelMap();
        regen();
    }
    public void regen()
    {
        generateMesh();
        //converts the list of vertices to an array
        //im not gonna add that many comments anymore
        m.vertices = vertices.ToArray();
        m.triangles = indices.ToArray();
        m.uv = uvs.ToArray();

        m.RecalculateNormals();

        mf.mesh = m;
    }
    void generateMesh()
    {
        List<Vector2> p = new List<Vector2>();
        Vector3 pos = new Vector3();
        for (int x = 0; x < chunk_width; x++)
        {
            for (int y = 0; y < chunk_height; y++)
            {
                if (db.voxelTypes[voxelmap[x, y]].solid)
                {
                    pos = new Vector3(x, y, 0);

                    for (int i = 0; i < VoxelData.vertices.Length; i++)
                    {
                        vertices.Add(pos + VoxelData.vertices[0]);
                        vertices.Add(pos + VoxelData.vertices[1]);
                        vertices.Add(pos + VoxelData.vertices[2]);
                        vertices.Add(pos + VoxelData.vertices[3]);

                        addTex(voxelmap[x, y]);

                        indices.Add(vert_index);
                        indices.Add(vert_index + 1);
                        indices.Add(vert_index + 2);
                        indices.Add(vert_index + 2);
                        indices.Add(vert_index + 1);
                        indices.Add(vert_index + 3);

                        vert_index += 4;
                    }

                }
            }
        }

    }
    public int getvoxelidfromvector2(Vector2 v)
    {
        try
        {
            int x = Mathf.FloorToInt(v.x) - (int)transform.position.x;
            int y = Mathf.FloorToInt(v.y) - (int)transform.position.y;

            return voxelmap[x, y];
        }
        catch(IndexOutOfRangeException e)
        {
            return 0;
        }
    }
    void updateboxcolliders()
    {
        Vector2 campos = new Vector2();
        Vector2 cpref = Camera.main.transform.position;

        //rounds the x and y axis of the main camera's position to ints
        Util.Roundvec2toint(ref cpref, out campos);

        for (int x = 0; x < chunk_width; x++)
        {
            for(int y = 0; y < chunk_height; y++)
            {
                // change 2                                vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                if (db.voxelTypes[voxelmap[x, y]].solid && db.voxelTypes[voxelmap[x,y]].collidable)
                {
                    // checks if the distance between a voxel and the camera's position is less than or equal to 5.0
                    if (Vector2.Distance(new Vector2(x,y),campos) <= 5.0f)
                    {
                        // makes sure the collider doesn't exist
                        if(colliders[x,y] == null && db.voxelTypes[voxelmap[x,y]].solid &&!db.voxelTypes[voxelmap[x,y]].liquid)
                        {
                            BoxCollider2D bcol = gameObject.AddComponent<BoxCollider2D>();
                            bcol.size = new Vector2(1, 1);
                            // offsets the collider to the voxel's position                                                                       change 3 vvvv
                            bcol.offset = new Vector2(x, y) + new Vector2(Mathf.Abs(transform.position.x)/2, Mathf.Abs(transform.position.y)/2) + voxel_col_offset;
                            colliders[x, y] = bcol;
                        }
                    }
                    // checks if the distance between the camera and the voxel isn't ess than or equal to 5.0 and the collider at the voxel position exist
                    else if (colliders[x, y] != null)
                    {
                        // destroys the collider
                        Destroy(colliders[x, y]);
                        //removes the collider from the array
                        colliders[x, y] = null;
                    }
                }
            }
        }
    }
    void addTex(int tid)
    {
        float n = 1f / 32;
        float y = tid / 32;
        float x = tid - (y * 32);
        

        x *= n;
        y *= n;
        y = 1f - y - n;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + n));
        uvs.Add(new Vector2(x + n, y));
        uvs.Add(new Vector2(x + n, y + n));

    }
    public void setVoxel(int x,int y,int id)
    {
        int _x = x - (int)transform.position.x;
        int _y = y - (int)transform.position.y;

        try
        {
            resetMesh();
            voxelmap[_x, _y] = id;
            if (db.voxelTypes[id].liquid)
            {
                mass[_x, _y] = 4.0f;
            }
            else if (!db.voxelTypes[id].solid)
            {
                mass[_x, _y] = 0.0f;
            }
            if (!db.voxelTypes[voxelmap[_x,_y]].solid && colliders[_x,_y] != null)
            {
                Destroy(colliders[_x, _y]);
                colliders[_x, _y] = null;
               
            }
            try
            {
                if (db.voxelTypes[id].placeoncvoxel && (db.voxelTypes[voxelmap[x,y-1]].placeoncvoxel || db.voxelTypes[voxelmap[x,y-1]].liquid || voxelmap[x,y-1] == 0))
                {
                    voxelmap[x, y] = 0;
                }
            }
            catch(IndexOutOfRangeException e) { }
            regen();
        }
        catch (IndexOutOfRangeException e) { }
    }
    GameObject explosion;

    public void explodeVoxels(int x,int y,int radius)
    {
        int fx = x - (int)transform.position.x;
        int fy = y - (int)transform.position.y;

        try
        {
            resetMesh();
            for (int xx = 0; xx < radius; xx++)
            {
                for (int yy = 0; yy < radius; yy++)
                {
                    int _x = fx + xx, _y = fy + yy;
                    voxelmap[_x, _y] = 0;
                    if (!db.voxelTypes[voxelmap[_x, _y]].solid && colliders[_x, _y] != null)
                    {
                        Destroy(colliders[_x, _y]);
                        colliders[_x, _y] = null;

                    }
                }
            }
            regen();
            
            Instantiate(explosion, new Vector2(fx, fy), Quaternion.identity, null);

        }
        catch (IndexOutOfRangeException e) { }
    }
    void c_setvoxel(int x,int y,int id)
    {
        resetMesh();
        voxelmap[x, y] = id;
        regen();
    }
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
}
