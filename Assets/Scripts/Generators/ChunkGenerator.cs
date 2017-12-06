using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter),
    typeof(MeshRenderer))]
public class ChunkGenerator : MonoBehaviour
{
    public bool dummyVar;
    public bool showVertices;
    public bool showNormals;
    public Color normalsColor;

    public Datas datas;
    private Mesh mesh;
    private MeshRenderer meshRenderer;

    private MeshData meshData;
    private Queue<Action> actionsToDo
        = new Queue<Action>();

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Generate(
        float x, float z)
    {
        Vector3 pos;

        if (Application.isPlaying)
        {
            meshRenderer.enabled = false;

            WaitCallback callback = new WaitCallback(delegate
            {
                pos = new Vector3(x, 0, z);
                meshData = TerrainGenerator.Generate(datas, pos);
                actionsToDo.Enqueue(PutMeshValues);
            });

            ThreadPool.QueueUserWorkItem(callback);
        }
        else
        {
            pos = transform.position;
            meshData = TerrainGenerator.Generate(datas, pos);
            PutMeshValues();
        }
    }

    private void PutMeshValues()
    {
        Mesh temp = GetComponent<MeshFilter>().sharedMesh;
        if (temp)
        {
            if (!Application.isPlaying)
                DestroyImmediate(temp);
            else
                Destroy(temp);
        }

        mesh = new Mesh();
        mesh.vertices = meshData.vertices;
        mesh.normals = meshData.normals;
        mesh.triangles = meshData.triangles;
        mesh.colors = meshData.colors;
        // Saving the indexes as uvs
        mesh.SetUVs(0, meshData.indexes.ToList());

        if (Application.isPlaying)
            meshRenderer.enabled = true;

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void Update()
    {
        // Execute actions in main thread
        if (actionsToDo.Count > 0)
        {
            actionsToDo.Dequeue()();
        }
    }

    private void OnDrawGizmos()
    {
        if (mesh && showVertices)
        {
            foreach (Vector3 v in mesh.vertices)
                Gizmos.DrawSphere(transform.TransformPoint(v), 0.1f);
        }
        if (mesh && showNormals)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            Gizmos.color = normalsColor;

            for (int i = 0; i < vertices.Length; i++)
            {
                Ray ray = new Ray(transform.TransformPoint(vertices[i]),
                    transform.TransformDirection(normals[i]));
                Gizmos.DrawRay(ray);
            }
        }
    }

    private void OnValidate()
    {
        if (datas)
        {
            datas.DiscardEventReferences();
            datas.updateEvent += delegate
            {
                Generate(0, 0);
            };
        }
    }

    private void OnDestroy()
    {
        Mesh temp = GetComponent<MeshFilter>().sharedMesh;
        if (temp)
            Destroy(temp);
    }
}
