using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class VATTool : EditorWindow
{
    private const int MAX_TEXTURE_SIZE = 4096;

    //private AnimationClip clip;
    private GameObject animatedGameObject;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private VATAnimationGenerator vatAnimGenerator;
    private VATCharacterAnimator vatCharacterAnimator;
    private float minSamplingRate = 60.0f;
    private bool powerOfTwo = true;

    private bool hasResults = false;
    private Texture2D results_texture;
    private Texture2D normals_results_texture;
    private float results_duration;
    private Vector2 results_bounds;

    [MenuItem("Tools/Vertex Animation Texture Tool")]
    static void Init()
    {
        VATTool window = (VATTool)GetWindow(typeof(VATTool));
        window.Show();
    }

    private void OnGUI()
    {
        animatedGameObject = (GameObject)EditorGUILayout.ObjectField("Animated GameObject", animatedGameObject, typeof(GameObject), true);
        vatCharacterAnimator = (VATCharacterAnimator)EditorGUILayout.ObjectField("VAT Character Animator", vatCharacterAnimator, typeof(VATCharacterAnimator), true);
        EditorGUILayout.Space();
        minSamplingRate = EditorGUILayout.FloatField("Sampling rate (per sec.)", minSamplingRate);
        powerOfTwo = EditorGUILayout.Toggle("Power of two", powerOfTwo);

        GUI.enabled =
            animatedGameObject
            && minSamplingRate > 0;

        if (GUILayout.Button("Generate"))
            GenerateTexture();

        GUI.enabled = true;


        if (hasResults)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Last results:");
            EditorGUI.indentLevel++;
            EditorGUILayout.ObjectField("Vertex Animation Texture: ", results_texture, typeof(Texture), false);
            EditorGUILayout.ObjectField("Normals Animation Texture: ", normals_results_texture, typeof(Texture), false);
            EditorGUILayout.FloatField("Duration: ", results_duration);
            EditorGUILayout.Vector2Field("Bounds: ", results_bounds);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }

    public void GenerateTexture()
    {
        skinnedMeshRenderer = animatedGameObject.GetComponent<SkinnedMeshRenderer>();
        if (!skinnedMeshRenderer)
        {
            skinnedMeshRenderer = animatedGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (!skinnedMeshRenderer)
        {
            Debug.LogError("No skinned mesh renderer found in assigne animatedGameObject");
            return;
        }

        vatAnimGenerator = animatedGameObject.GetComponent<VATAnimationGenerator>();
        if (!vatAnimGenerator)
        {
            vatAnimGenerator = animatedGameObject.GetComponentInChildren<VATAnimationGenerator>();
        }

        if (!vatAnimGenerator)
        {
            Debug.LogError("No skinned mesh renderer found in assigne vatAnimGenerator");
            return;
        }



        Vector3[] defaultVertexPositions = skinnedMeshRenderer.sharedMesh.vertices; //the vertex positions when the mesh is not animated

        int textureHeight = defaultVertexPositions.Length;
        if (powerOfTwo)
            textureHeight = GetNearestPowerOfTwo(textureHeight);

        if (textureHeight > MAX_TEXTURE_SIZE)
        {
            EditorUtility.DisplayDialog(
                "Error",
                string.Format("Vertices count of {0} exceeds the max texture size ({1})", skinnedMeshRenderer.sharedMesh.name, MAX_TEXTURE_SIZE),
                "OK");
            return;
        }

        vatAnimGenerator.SetupAnimationFrames(minSamplingRate, vatCharacterAnimator); //Go through and assign values to our frame system, which we'll then use to pull things back
        //We'll want to populate our vatCharacterAnimator here
        int frameCount = vatAnimGenerator.GetFrameCount(minSamplingRate); //Mathf.CeilToInt(clip.length * minSamplingRate);
        //Debug.Log(frameCount);
        int textureWidth = frameCount;
        if (powerOfTwo)
            textureWidth = GetNearestPowerOfTwo(textureWidth);

        if (textureWidth > MAX_TEXTURE_SIZE || textureHeight > MAX_TEXTURE_SIZE)
        {
            EditorUtility.DisplayDialog("Error", string.Format("Animation clip is too long to be sampled at {0}FPS for a max texture size of {1}!", minSamplingRate, MAX_TEXTURE_SIZE), "OK");
            return;
        }

        Vector3[][] frames = new Vector3[textureWidth][];
        Vector3[][] normalFrames = new Vector3[textureWidth][];

        Mesh bakedMesh = new Mesh(); //we need to bake the skinned mesh to a regular mesh in order to get its vertex positions on each frame
        List<Vector3> tmpVPos = new List<Vector3>(); //tmp list to store the vertex positions of the baked mesh
        List<Vector3> tmpNDir = new List<Vector3>();
        
        Vector2 bounds = new Vector2(float.PositiveInfinity, float.NegativeInfinity); //minimum and maximum x, y or z values of each vertex positions, bounds.x is min / bounds.y is max

        Undo.RegisterFullObjectHierarchyUndo(animatedGameObject, "Sample animation"); //remember the current "pose" of the gameobject to be animated, horrible but necessary

        //This mapping should be frame count...

        for (int x = 0; x < frameCount; x++)
        {
            AnimationClip clip = vatAnimGenerator.GetClip(x); //Grabs the current clip for this time index

            //float t = (x / (float)frameCount) * clip.length;
            float t = vatAnimGenerator.GetClipTime(x); //Will return the 0-1 time value for how far the current clip we are
            if (clip)
            {
                clip.SampleAnimation(animatedGameObject, t);
            }
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            bakedMesh.GetVertices(tmpVPos);
            bakedMesh.GetNormals(tmpNDir);

            for (int y = 0; y < tmpVPos.Count; y++)
            {
                tmpVPos[y] -= defaultVertexPositions[y]; //get the offset of the vertex position on THIS frame from its "default" position when the mesh is still

                bounds.x = Mathf.Min(bounds.x, tmpVPos[y].x, tmpVPos[y].y, tmpVPos[y].z);
                bounds.y = Mathf.Max(bounds.y, tmpVPos[y].x, tmpVPos[y].y, tmpVPos[y].z);
            }

            frames[x] = tmpVPos.ToArray();
            normalFrames[x] = tmpNDir.ToArray();
        }

        //Set the bounds for our different group scripts
        if (vatCharacterAnimator)
        {
            vatCharacterAnimator.animBounds = bounds;
        }
        vatAnimGenerator.bounds = bounds;

        Undo.PerformUndo(); //reset the animated pose, i hate this
        
        Texture2D texture = new Texture2D(textureWidth,
                                          textureHeight,
                                          TextureFormat.RGBA32,
                                          false);
        

        
        for (int x = 0; x < frameCount; x++)
        {
            for (int y = 0; y < frames[x].Length; y++)
            {
                Color col = new Color(
                   Mathf.InverseLerp(bounds.x, bounds.y, frames[x][y].x),
                   Mathf.InverseLerp(bounds.x, bounds.y, frames[x][y].y),
                   Mathf.InverseLerp(bounds.x, bounds.y, frames[x][y].z)
                   );

                texture.SetPixel(x, y, col);
            }
        }

        texture.Apply();

        Texture2D normalTexture = new Texture2D(textureWidth,
                                          textureHeight,
                                          TextureFormat.RGBA32,
                                          false);

        for (int x = 0; x < frameCount; x++)
        {
            for (int y = 0; y < normalFrames[x].Length; y++)
            {
                Color col = new Color(
                   Mathf.InverseLerp(-1f, 1f, normalFrames[x][y].x),
                   Mathf.InverseLerp(-1f, 1f, normalFrames[x][y].y),
                   Mathf.InverseLerp(-1f, 1f, normalFrames[x][y].z)
                   );

                normalTexture.SetPixel(x, y, col);
            }
        }

        normalTexture.Apply();

        string path = EditorUtility.SaveFilePanelInProject("Save Texture", "VATTexture_" + animatedGameObject.name, "png", "Select destination");

        string pathWithoutExtension = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));

        path = pathWithoutExtension + "_vertex.png";

        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Error", "Vertex path is invalid!", "OK");
            return;
        }

        //Write out our displacement map
        writeTextureToFile(texture, path, textureWidth, textureHeight);

        hasResults = true;
        //results_duration = clip.length;
        results_bounds = bounds;
        results_texture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(path);

        vatCharacterAnimator.vertexAnimationTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(path);   //Assign our texture to our animator

        string normalpath = pathWithoutExtension + "_normal.png";

        if (string.IsNullOrEmpty(normalpath))
        {
            EditorUtility.DisplayDialog("Error", "Normal path is invalid!", "OK");
            return;
        }

        //Write out our displacement map
        writeTextureToFile(normalTexture, normalpath, textureWidth, textureHeight);
        normals_results_texture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(normalpath);
        vatCharacterAnimator.normalsAnimationTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(normalpath);   //Assign our texture to our animator
    }

    private void writeTextureToFile(Texture2D texture, string path, int textureWidth, int textureHeight)
    {
        byte[] pngData = texture.EncodeToPNG();

        if (pngData != null)
        {
            // Ensure the path is inside the Assets folder
            System.IO.File.WriteAllBytes(path, pngData);

            // Force Unity to recognize the new asset
            AssetDatabase.ImportAsset(path);

            // Now get the TextureImporter
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                Debug.Log(importer);
                importer.textureType = TextureImporterType.Default;
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.sRGBTexture = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = Mathf.RoundToInt(Mathf.Max(GetNearestPowerOfTwo(textureWidth), GetNearestPowerOfTwo(textureHeight)));
                importer.npotScale = powerOfTwo ? TextureImporterNPOTScale.ToNearest : TextureImporterNPOTScale.None;

                // Mark importer as dirty and save changes
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                // Refresh AssetDatabase to ensure Unity recognizes changes
                AssetDatabase.Refresh();
                /*
                hasResults = true;
                //results_duration = clip.length;
                results_bounds = bounds;
                results_texture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(path);
                */
            }
            else
            {
                Debug.LogError("Failed to get TextureImporter for path: " + path);
            }
        }
    }

    private int GetNearestPowerOfTwo(int x)
    {
        if (x < 0) { return 0; }
        --x;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        return x + 1;
    }
}
