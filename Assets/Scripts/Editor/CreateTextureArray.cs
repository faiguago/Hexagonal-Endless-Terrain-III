using UnityEditor;
using UnityEngine;

public class CreateTextureArray : ScriptableWizard
{
    public Texture2D[] textures;

    [MenuItem("Assets/Create/Texture Array")]
    private static void CreateWizard()
    {
        DisplayWizard<CreateTextureArray>
            ("Create texture array", "Create");
    }

    private void OnWizardCreate()
    {
        if (textures.Length != 0)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save texture array", "Texture array", "asset", "Save texture array");
            if (path.Length != 0)
            {
                Texture2D t = textures[0];
                Texture2DArray textureArray = new Texture2DArray(
                    t.width, t.height, textures.Length, t.format, t.mipmapCount > 1);
                textureArray.anisoLevel = t.anisoLevel;
                textureArray.filterMode = t.filterMode;
                textureArray.wrapMode = t.wrapMode;

                for (int i = 0; i < textures.Length; i++)
                {
                    for (int m = 0; m < t.mipmapCount; m++)
                    {
                        Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);
                    }
                }

                AssetDatabase.CreateAsset(textureArray, path);
            }
        }
    }
}
