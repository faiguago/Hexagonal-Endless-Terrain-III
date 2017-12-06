using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshData
{
    public Vector3[] vertices;
    public Vector3[] normals;
    public int[] triangles;
    public Color[] colors;

    public bool[] borders;

    // Indexes
    private float[] i1;
    private float[] i2;
    public Vector2[] indexes;

    public MeshData(int size, int scale)
    {
        CreateVertices(size, scale);
        CreateTriangles(size);
    }

    private void CreateVertices(
        int size, int scale)
    {
        List<bool> borders = new List<bool>();
        List<Vector3> vertices = new List<Vector3>();

        for (int z = 0; z <= size; z++)
        {
            int xSize = size + z;

            for (int x = 0; x <= xSize; x++)
            {
                vertices.Add(new Vector3(x - Hex.cos60 * z - size / 2f,
                    0, (z - size) * Hex.sin60) * scale);

                bool isBorder =
                    z == 0 || x == 0 || x == xSize;

                if (isBorder)
                    borders.Add(true);
                else
                    borders.Add(false);
            }
        }
        for (int z = size + 1, i = 0; z <= 2 * size; z++, i++)
        {
            int xSize = 2 * size - i - 1;

            for (int x = 0; x <= xSize; x++)
            {
                vertices.Add(new Vector3(x - (size - 1 - i) * Hex.cos60 - size / 2f,
                    0, (z - size) * Hex.sin60) * scale);

                bool isBorder =
                    z == 2 * size || x == 0 || x == xSize;

                if (isBorder)
                    borders.Add(true);
                else
                    borders.Add(false);
            }
        }

        this.borders = borders.ToArray();
        this.vertices = vertices.ToArray();
    }

    private void CreateTriangles(int size)
    {
        int i = 0;
        List<int> triangles = new List<int>();

        for (int z = 0; z < size; z++, i++)
        {
            int xSize = size + z;

            for (int x = 0; x < xSize; x++, i++)
            {
                triangles.Add(i);
                triangles.Add(i + xSize + 1);
                triangles.Add(i + xSize + 2);

                triangles.Add(i);
                triangles.Add(i + xSize + 2);
                triangles.Add(i + 1);

                if (x == xSize - 1)
                {
                    triangles.Add(i + 1);
                    triangles.Add(i + xSize + 2);
                    triangles.Add(i + xSize + 3);
                }
            }
        }

        for (int z = 0; z < size; z++, i++)
        {
            int xSize = 2 * size - z;

            for (int x = 0; x < xSize; x++, i++)
            {
                triangles.Add(i);
                triangles.Add(i + xSize + 1);
                triangles.Add(i + 1);

                if (x != xSize - 1)
                {
                    triangles.Add(i + 1);
                    triangles.Add(i + xSize + 1);
                    triangles.Add(i + xSize + 2);
                }
            }
        }

        this.triangles = triangles.ToArray();
    }

    public void FixNormals(int size)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<Vector2> indexes = new List<Vector2>();

        Vector3[] oldVertices = this.vertices;
        Vector3[] oldNormals = this.normals;
        Color[] oldColors = this.colors;
        Vector2[] oldIndexes = this.indexes;

        bool[] borders = this.borders;

        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (!borders[i])
            {
                vertices.Add(oldVertices[i]);
                normals.Add(oldNormals[i]);
                colors.Add(oldColors[i]);
                indexes.Add(oldIndexes[i]);
            }
        }

        // Save mesh values
        this.vertices = vertices.ToArray();
        this.normals = normals.ToArray();
        this.colors = colors.ToArray();
        this.indexes = indexes.ToArray();

        CreateTriangles(size);
    }

    public void AddHeights(ref TerrainVars tVars,
        ref NoiseVars nVars, Vector3 pos)
    {
        float height = tVars.Height;
        Vector3[] vertices = this.vertices;

        AnimationCurve curve = 
            new AnimationCurve(tVars.curve.keys);

        // Get noise
        float[] noise =
            NoiseGenerator.noise(vertices, pos, ref nVars);

        for (int i = 0; i < vertices.Length; i++)
        {
            noise[i] = Mathf.Clamp01(curve.Evaluate(noise[i]));
            vertices[i].y = (noise[i] * 2 - 1) * height;
        }

        // Save mesh values
        this.vertices = vertices;
    }

    public void AddHeightsAndColors(ref TerrainVars tVars,
        ref NoiseVars nVars, Vector3 pos)
    {
        Vector3[] vertices = this.vertices;
        Color[] colors = new Color[vertices.Length];

        Gradient gradient = tVars.gradient;

        // Color keys
        float[] keys = gradient.colorKeys
            .Select(k => k.time).ToArray();

        i1 = new float[vertices.Length];
        i2 = new float[vertices.Length];

        AnimationCurve curve =
            new AnimationCurve(tVars.curve.keys);

        float height = tVars.Height;

        // Get noise
        float[] noise =
            NoiseGenerator.noise(vertices, pos, ref nVars);

        for (int i = 0; i < vertices.Length; i++)
        {
            float cNoise = Mathf.Clamp01(curve.Evaluate(noise[i]));

            colors[i] = gradient.Evaluate(cNoise);

            vertices[i].y = (cNoise * 2 - 1) * height;

            try
            {
                if (cNoise < keys[1])
                    i1[i] = 0;
                if (cNoise > keys[0] && cNoise < keys[3])
                    i2[i] = 1;
                if (cNoise > keys[2] && cNoise < keys[5])
                    i1[i] = 2;
                if (cNoise > keys[4] && cNoise < keys[7])
                    i2[i] = 3;
                if (cNoise > keys[6])
                    i1[i] = 4;
            }
            catch (IndexOutOfRangeException) { };
        }

        // Save mesh values
        this.vertices = vertices;
        this.colors = colors;
        RecalculateNormals();
        SaveIndexes();
    }

    private void SaveIndexes()
    {
        Vector2[] indexes = new Vector2[vertices.Length];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = new Vector2(i1[i], i2[i]);
        }

        this.indexes = indexes;
    }

    public void ForceFlatShaded()
    {
        int t = triangles.Length;

        Vector3[] vertices = new Vector3[t];
        Color[] colors = new Color[t];
        Vector2[] indexes = new Vector2[t];

        for (int i = 0; i < t; i++)
        {
            int triangle = triangles[i];

            vertices[i] = this.vertices[triangle];
            colors[i] = this.colors[triangle];
            indexes[i] = this.indexes[triangle];

            triangles[i] = i;
        }

        this.vertices = vertices;
        this.colors = colors;
        this.indexes = indexes;
        RecalculateNormals();
    }

    public void RecalculateNormals()
    {
        normals = new Vector3[vertices.Length];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 normal =
                CalculateNormal(vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]);

            normals[triangles[i]] += normal;
            normals[triangles[i + 1]] += normal;
            normals[triangles[i + 2]] += normal;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }
    }

    private Vector3 CalculateNormal(
        Vector3 A, Vector3 B, Vector3 C)
    {
        return Vector3.Cross(
            (B - A).normalized,
            (C - A).normalized);
    }

}