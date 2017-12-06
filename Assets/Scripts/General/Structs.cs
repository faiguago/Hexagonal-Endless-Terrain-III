using System;
using UnityEngine;

public enum TerrainTypes
{
    Normal,
    FlatShaded
}

[Serializable]
public struct TerrainVars
{
    public TerrainTypes terrainType;

    [SerializeField] [Range(16, 145)] private int size;
    [SerializeField] private int scale;
    [SerializeField] private float height;
    [SerializeField] private int gridSize;
    [SerializeField] [Range(1, 16)] private int colLOD;

    public int Size
    {
        get
        {
            if (terrainType != TerrainTypes.FlatShaded)
                return size >= 16 ? (size / ColLOD) * ColLOD : 16;
            else
                return size >= 16 && size <= 50
                    ? (size / ColLOD) * ColLOD
                    : (50 / ColLOD) * ColLOD;
        }
    }
    public int Scale { get { return scale >= 1 ? scale : 1; } }
    public float Height { get { return height; } }

    public int WorldSize { get { return Size * Scale; } }
    public int GridSize
    {
        get
        {
            return gridSize >= 1 ? gridSize : 1;
        }
    }
    private int ColLOD
    {
        get
        {
            if (colLOD <= 1)
            {
                return 1;
            }
            else
            {
                return (colLOD / 2) * 2;
            }
        }
    }
    public int ColSize { get { return Size / ColLOD; } }
    public int ColScale { get { return Scale * ColLOD; } }

    public Gradient gradient;
    public AnimationCurve curve;
    public Material material;
}

[Serializable]
public struct NoiseVars
{
    [SerializeField] private float frequency;
    [Range(1, 8)]
    [SerializeField]
    private int octaves;
    [Range(1f, 4f)]
    [SerializeField]
    private float lacunarity;
    [Range(0f, 1f)]
    [SerializeField]
    private float persistence;
    [SerializeField] private int seed;

    public float Frequency { get { return frequency; } }
    public int Octaves { get { return octaves >= 1 ? octaves : 1; } }
    public float Lacunarity { get { return lacunarity >= 1f ? lacunarity : 1f; } }
    public float Persistence { get { return persistence; } }
    public int Seed { get { return seed; } }
}