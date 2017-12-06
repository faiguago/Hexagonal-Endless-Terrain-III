using UnityEngine;

public static class TerrainGenerator
{
    public static MeshData Generate(Datas datas, Vector3 pos)
    {
        bool isFlatShaded = datas.tVars.terrainType == TerrainTypes.FlatShaded;

        MeshData mesh = new MeshData(isFlatShaded ? datas.tVars.Size :
            datas.tVars.Size + 1, datas.tVars.Scale);

        mesh.AddHeightsAndColors(ref datas.tVars, ref datas.nVars, pos);

        if (!isFlatShaded)
            mesh.FixNormals(datas.tVars.Size);
        else
            mesh.ForceFlatShaded();

        return mesh;
    }
}
