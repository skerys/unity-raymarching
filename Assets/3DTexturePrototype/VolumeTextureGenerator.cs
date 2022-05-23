using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VolumeTextureGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    [MenuItem("Test/Create Noise Texture")]
    static void CreateTexture()
    {
        Texture3D tex3D = new Texture3D(128,128,128, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        Color[] pixels = new Color[128*128*128];

        Vector3 center = new Vector3(64, 64, 64);

        for(int i = 0; i < 128; i++)
        {
            for(int j = 0; j < 128; j++)
            {
                for(int k = 0; k < 128; k++)
                {
                    float value = 1.0f - Vector3.Distance(new Vector3(i, j, k), center) / 64.0f;

                    pixels[128 * 128 * i + 128 * j + k] = new Color(0.0f, value, 0.5f, 1.0f);
                }
            }
                
        }

        tex3D.SetPixels(pixels);
        tex3D.Apply();

        AssetDatabase.CreateAsset(tex3D, "Assets/WhiteNoise.asset");
    }

    
}
