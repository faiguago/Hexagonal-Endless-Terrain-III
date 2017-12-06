using UnityEngine;

public static class ColliderGenerator
{
    public static MeshData Generate(Datas datas, Vector3 pos)
    {
        MeshData mesh = new MeshData(
            datas.tVars.ColSize, datas.tVars.ColScale);

        mesh.AddHeights(ref datas.tVars, ref datas.nVars, pos);

        return mesh;
    }
}