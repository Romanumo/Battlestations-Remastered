//Bloom effect written by catlikecoding or JasperFlick
//https://catlikecoding.com/unity/tutorials/advanced-rendering/bloom/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class BloomEffect : MonoBehaviour
{
    [Range(1, 16)] public int distortionIterations;
    RenderTexture[] textures = new RenderTexture[16];
    [Range(0, 5)] public float threshold = 1;
    [Range(0, 1)] public float softThreshold = 0.5f;
    [Range(0, 5)] public float intensity = 1;

    public Shader bloomShader;

    Material bloom;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(bloom == null)
        {
            bloom = new Material(bloomShader);
            bloom.hideFlags = HideFlags.HideAndDontSave;
        }
        bloom.SetFloat("_Threshold", threshold);
        bloom.SetFloat("_SoftThreshold", softThreshold);
        bloom.SetFloat("_Intensity", Mathf.GammaToLinearSpace(intensity));

        int width = source.width;
        int height = source.height;
        RenderTextureFormat format = source.format;

        RenderTexture procedureTex = textures[0] = RenderTexture.GetTemporary(width, height, 0, format);

        //Copy frame to destination, with the 3rd arg it is shader apply
        Graphics.Blit(source, procedureTex, bloom, 3);

        RenderTexture currentSource = procedureTex;
        int i = 1;
        for (;i<distortionIterations;i++)
        {
            width /= 2;
            height /= 2;
            if (height < 2)
                break;

            //The algorithm is that way that currentsource saves previous blurred iterationa nd multiply it to new one(ig)
            procedureTex = textures[i] = RenderTexture.GetTemporary(width, height, 0, format, 0);
            //Blit is not kist copy and replacing but here is rather multiplying the distorion fx
            Graphics.Blit(currentSource, procedureTex, bloom, 0);
            currentSource = procedureTex;
        }

        for (i -= 2; i >= 0;i--)
        {
            procedureTex = textures[i];
            textures[i] = null;
            Graphics.Blit(currentSource, procedureTex, bloom, 1);
            RenderTexture.ReleaseTemporary(currentSource);
            currentSource = procedureTex;
        }

        bloom.SetTexture("_SourceTex", source);
        //Applying blurred imgae to the original
        Graphics.Blit(procedureTex, destination, bloom, 2);
        RenderTexture.ReleaseTemporary(procedureTex);
    }
}
