using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]

public class SurfaceNets : MonoBehaviour
{
    ComputeBuffer chunkVertices;
    ComputeBuffer netsX;
    ComputeBuffer netsY;
    ComputeBuffer netsZ;
    ComputeBuffer FinalVertices;
    ComputeBuffer FinalTriangles;
    ComputeBuffer XTriangleFaces;
    ComputeBuffer YTriangleFaces;
    ComputeBuffer ZTriangleFaces;
    ComputeBuffer vertMapper;

    public ComputeShader chunkGenerator;
    public ComputeShader NetsSampler;
    public ComputeShader SurfaceNetter;
    public ComputeShader SNTriangler;

    private Mesh mesh;
    private MeshCollider meshCollider;

    public Vector4[] c;
    public uint[] tfx;
    public uint[] tfy;
    public uint[] tfz;
    public Vector3[] vertices;
    public int[] triangles;

    Vector3[] NX;
    Vector3[] NY;
    Vector3[] NZ;

    bool showGrid;
    bool showFV;
    bool showNX;
    bool showNY;
    bool showNZ;

    void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();

        chunkVertices = new ComputeBuffer(9 * 9 * 9, sizeof(float) * 4);
        netsX = new ComputeBuffer(9 * 9 * 9, sizeof(float) * 3);
        netsY = new ComputeBuffer(9 * 9 * 9, sizeof(float) * 3);
        netsZ = new ComputeBuffer(9 * 9 * 9, sizeof(float) * 3);

        XTriangleFaces = new ComputeBuffer(9 * 9 * 9, sizeof(uint));
        YTriangleFaces = new ComputeBuffer(9 * 9 * 9, sizeof(uint));
        ZTriangleFaces = new ComputeBuffer(9 * 9 * 9, sizeof(uint));
        FinalVertices = new ComputeBuffer(8*8*8, sizeof(float) * 3, ComputeBufferType.Counter);
        vertMapper = new ComputeBuffer(8 * 8 * 8, sizeof(uint));
        FinalTriangles = new ComputeBuffer(12000, sizeof(int)*3, ComputeBufferType.Counter);


        chunkGenerator.SetBuffer(0, "vertices", chunkVertices);

        chunkGenerator.Dispatch(0, 1, 1, 1);
        //actually when i check in unity i find 512 vertices on the mesh, but when i check ther data inside the buffer i find 
        


        NetsSampler.SetBuffer(0, "netsX", netsX);
        NetsSampler.SetBuffer(1, "netsZ", netsZ);
        NetsSampler.SetBuffer(2, "netsY", netsY);

        NetsSampler.SetBuffer(0, "vertices", chunkVertices);
        NetsSampler.SetBuffer(1, "vertices", chunkVertices);
        NetsSampler.SetBuffer(2, "vertices", chunkVertices);
        
        NetsSampler.SetBuffer(0, "XTriangleFaces", XTriangleFaces);
        NetsSampler.SetBuffer(1, "ZTriangleFaces", ZTriangleFaces);
        NetsSampler.SetBuffer(2, "YTriangleFaces", YTriangleFaces);

        NetsSampler.Dispatch(0, 1, 1, 1);
        NetsSampler.Dispatch(1, 1, 1, 1);
        NetsSampler.Dispatch(2, 1, 1, 1);


        SurfaceNetter.SetBuffer(0, "FinalVertices", FinalVertices);

        SurfaceNetter.SetBuffer(0, "netsX", netsX);
        SurfaceNetter.SetBuffer(0, "netsY", netsY);
        SurfaceNetter.SetBuffer(0, "netsZ", netsZ);
        SurfaceNetter.SetBuffer(0, "vertMapper", vertMapper);

        SurfaceNetter.SetBuffer(0, "XTriangleFaces", XTriangleFaces);
        SurfaceNetter.SetBuffer(0, "ZTriangleFaces", ZTriangleFaces);
        SurfaceNetter.SetBuffer(0, "YTriangleFaces", YTriangleFaces);

        SurfaceNetter.Dispatch(0, 1, 1, 1);

        SNTriangler.SetBuffer(0, "vertices", chunkVertices);
        SNTriangler.SetBuffer(0, "XTriangleFaces", XTriangleFaces);
        SNTriangler.SetBuffer(0, "ZTriangleFaces", ZTriangleFaces);
        SNTriangler.SetBuffer(0, "YTriangleFaces", YTriangleFaces);
        SNTriangler.SetBuffer(0, "FinalVertices", FinalVertices);
        SNTriangler.SetBuffer(0, "FinalTriangles", FinalTriangles);
        SNTriangler.SetBuffer(0, "vertMapper", vertMapper);


        SNTriangler.Dispatch(0, 1, 1, 1);

        c = new Vector4[chunkVertices.count];

        chunkVertices.GetData(c);

        vertices = new Vector3[FinalVertices.count];
        FinalVertices.GetData(vertices);

        tfx = new uint[XTriangleFaces.count];
        tfy = new uint[YTriangleFaces.count];
        tfz = new uint[ZTriangleFaces.count];

        XTriangleFaces.GetData(tfx);
        YTriangleFaces.GetData(tfy);
        ZTriangleFaces.GetData(tfz);


        triangles = new int[FinalTriangles.count];
        
        FinalTriangles.GetData(triangles);

        NX = new Vector3[netsX.count];
        NY = new Vector3[netsY.count];
        NZ = new Vector3[netsZ.count];

        netsX.GetData(NX);
        netsY.GetData(NY);
        netsZ.GetData(NZ);

        //Debug.Log(vertices[2]);


        mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
    }
    private void OnDrawGizmos()
    {
        if(showFV)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < vertices.Length; i++)
            {

                Gizmos.DrawCube(vertices[i], 0.2f * Vector3.one);
            }
        }
        if (showGrid)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < c.Length; i++)
            {
                Gizmos.DrawSphere(c[i], .2f);
                /*if(i % 9 != 8)
                {
                    Gizmos.DrawLine(c[i], c[i + 1]);
                }*/

            }
        }

        if (showNX)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < NX.Length; i++)
            {
                Gizmos.DrawSphere(NX[i], .2f);
                /*if(i % 9 != 8)
                {
                    Gizmos.DrawLine(c[i], c[i + 1]);
                }*/

            }
        }
        if (showNY)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < NY.Length; i++)
            {
                Gizmos.DrawSphere(NY[i], .2f);
                /*if(i % 9 != 8)
                {
                    Gizmos.DrawLine(c[i], c[i + 1]);
                }*/

            }
        }
        if (showNZ)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < NZ.Length; i++)
            {
                Gizmos.DrawSphere(NZ[i], .2f);
                /*if(i % 9 != 8)
                {
                    Gizmos.DrawLine(c[i], c[i + 1]);
                }*/

            }
        }
        
            /*for (int i = 0; i < c.Length; i++)
            {
                if(i % 9 != 8)
                {
                    Gizmos.DrawLine(c[i], c[9 * i]);
                }
            }
            for (int i = 0; i < c.Length; i++)
            {
                if(i % 9 != 8)
                {
                    Gizmos.DrawLine(c[i], c[9 * 9 * i]);
                }
            }*/
        }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1)) { showGrid = !showGrid;}
        if (Input.GetKeyDown(KeyCode.Keypad2)) { showFV = !showFV;}
        if (Input.GetKeyDown(KeyCode.Keypad3)) { showNX = !showNX; }
        if (Input.GetKeyDown(KeyCode.Keypad4)) { showNY = !showNY; }
        if (Input.GetKeyDown(KeyCode.Keypad5)) { showNZ = !showNZ; }
    }

}
