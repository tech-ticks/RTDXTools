﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class Helpers
{
    public static readonly Color DefaultVertexColor = new Color(0.447f, 0.651f, 0.651f);
    
    #if UNITY_EDITOR
    [MenuItem("Tools/JSONClone")]
    public static void JsonCloneFromSelection()
    {
        var selection = Selection.activeGameObject;
        if (!selection)
            return;

        CreateClone(selection);
    }
    
    public static GameObject CreateClone(GameObject source)
    {
        var jsonComponents = new List<string>();
        var comps = source.GetComponents<Component>();
        var types = new List<Type>();

        foreach (var comp in comps)
        {
            Debug.Log(comp.name);
            string json = EditorJsonUtility.ToJson(comp);
            types.Add(comp.GetType());
            Debug.Log(json);
            jsonComponents.Add(json);
        }

        var clone = new GameObject {name = source.name, tag = source.tag, layer = source.layer};

        for (var i = 0; i < jsonComponents.Count; i++)
        {
            var comp = types[i] == typeof(Transform) ? clone.transform : clone.AddComponent(types[i]);
            EditorJsonUtility.FromJsonOverwrite(jsonComponents[i], comp);
        }

        foreach (Transform child in source.transform)
        {
            CreateClone(child.gameObject).transform.parent = clone.transform;
        }

        return clone;
    }

    
    public static void SetClipboard(string value)
    {
        if (value == null)
            throw new ArgumentNullException("Attempt to set clipboard with null");

        Process clipboardExecutable = new Process(); 
        clipboardExecutable.StartInfo = new ProcessStartInfo // Creates the process
        {
            RedirectStandardInput = true,
            FileName = @"clip", 
            UseShellExecute = false
        };
        clipboardExecutable.Start();

        clipboardExecutable.StandardInput.Write(value); // CLIP uses STDIN as input.
        // When we are done writing all the string, close it so clip doesn't wait and get stuck
        clipboardExecutable.StandardInput.Close(); 
    }
    
    [MenuItem("Tools/Clone Mesh")]
    public static void CloneMesh()
    {
        var selection = Selection.activeGameObject;
        if (selection == null)
            return;

        var meshFilter = selection.GetComponent<MeshFilter>();
        if (meshFilter == null)
            return;

        var src = meshFilter.sharedMesh;
        var dst = Object.Instantiate(src);
        AssetDatabase.CreateAsset(dst, "Assets/Mesh.asset");
    }

    [MenuItem("Tools/Export NavMesh")]
    public static void ExportNavMesh()
    {
        NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();

        Mesh mesh = new Mesh {vertices = triangulatedNavMesh.vertices, triangles = triangulatedNavMesh.indices};
        AssetDatabase.CreateAsset(mesh, "Assets/NavMesh.asset");
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Tools/Skinned Mesh Renderer to Mesh Renderer")]
    public static void SkinnedMeshRendererToMeshRenderer()
    {
        foreach (var go in Selection.gameObjects)
        {
            if (go.GetComponent<SkinnedMeshRenderer>() == null)
                continue;

            var skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
            var mesh = skinnedMeshRenderer.sharedMesh;
            var material = skinnedMeshRenderer.sharedMaterial;
            Object.DestroyImmediate(skinnedMeshRenderer);
            
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
    }

    [MenuItem("Tools/Print Texture Property Names")]
    public static void PrintTexturePropertyNames()
    {
        var selection = Selection.activeGameObject;
        if (selection == null)
            return;

        var renderer = selection.GetComponent<Renderer>();
        if (renderer == null)
            return;

        StringBuilder output = new StringBuilder();
        foreach (var material in renderer.sharedMaterials)
        {
            output.AppendLine(material.name + " :");
            output.Append("   TexturePropertyNames: ");
            output.AppendLine(string.Join(",", material.GetTexturePropertyNames()));
            output.Append("   Keywords: ");
            output.AppendLine(string.Join(",", material.shaderKeywords));
            output.Append("   Pass count: ");
            output.Append(material.passCount.ToString());
            for (int i = 0; i < material.passCount; i++)
            {
                output.Append(material.GetPassName(i) + ", ");
            }
            output.AppendLine();
        }
        Debug.Log(output.ToString());
    }

    [MenuItem("Tools/Unload all AssetBundles")]
    public static void UnloadAllAssetBundles()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }
    
    public static void ReplaceModelShaders(GameObject root)
    {
        var shader = Shader.Find("Custom/Preview");
        foreach (var mr in root.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in mr.sharedMaterials)
            {
                mat.shader = shader;
            }
        }
    }
    
    [MenuItem("Tools/Vertex Paint/Default in-game color (#72A6A6)")]
    public static void VertexPaintDefault() => VertexPaint(DefaultVertexColor, "black");
    
    [MenuItem("Tools/Vertex Paint/Black")]
    public static void VertexPaintBlack() => VertexPaint(Color.black, "black");
    
    [MenuItem("Tools/Vertex Paint/Gray")]
    public static void VertexPaintGray() => VertexPaint(Color.gray, "gray");
    
    [MenuItem("Tools/Vertex Paint/Dark Gray")]
    public static void VertexPaintLightGray() => VertexPaint(new Color(0.25f, 0.25f, 0.25f, 1), "darkgray");

    [MenuItem("Tools/Extract Animations")]
    public static void ExtractSelectedAnimations() => ExtractAnimations(Selection.objects, true);

    [MenuItem("Tools/Remove Animation Root")]
    public static void RemoveAnimationRoot()
    {
        foreach (var clip in Selection.objects.OfType<AnimationClip>())
        {
            // Animation paths are structured like root/PG_root/...
            // We need to get rid of the root object to make PG_root the new root
            string path = AssetDatabase.GetAssetPath(clip);
            string yaml = File.ReadAllText(path);
            File.WriteAllText(path, yaml.Replace("path: root/", "path: "));
        }
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Tools/Loop animations")]
    public static void LoopAnimations()
    {
        foreach (var clip in Selection.objects.OfType<AnimationClip>())
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }
    }
    
    [MenuItem("Tools/Copy UVs")]
    public static void CopyUVs()
    {
        if (Selection.objects.Length != 2)
            return;

        var srcMesh = Selection.objects[0] as Mesh;
        var dstMesh = Selection.objects[1] as Mesh;

        if (srcMesh == null || dstMesh == null)
            return;

        var srcMeshVertices = srcMesh.vertices;
        var dstMeshVertices = dstMesh.vertices;
        var srcMeshUvs = srcMesh.uv;
        var dstMeshUvs = dstMesh.uv;

        for (int i = 0; i < dstMesh.vertexCount; i++)
        {
            int nearestVertexIndex = 0;
            float nearestDistance = float.PositiveInfinity;
            for (int j = 0; j < srcMesh.vertexCount; j++)
            {
                float distance = Vector3.Distance(srcMeshVertices[i], dstMeshVertices[j]);
                if (distance < nearestDistance)
                {
                    nearestVertexIndex = j;
                    nearestDistance = distance;
                }
            }

            dstMeshUvs[i] = srcMeshUvs[nearestVertexIndex];
        }

        dstMesh.uv = dstMeshUvs;
    }

    [MenuItem("Tools/Fix AnimationClip Quaternion interpolation")]
    public static void FixAnimationClipQuaternionInterpolation()
    {
        foreach (var clip in Selection.objects.OfType<AnimationClip>())
        {
            clip.EnsureQuaternionContinuity();
            EditorUtility.SetDirty(clip);
        }
    }
    
    public static void VertexPaint(Color color, string colorName)
    {
        var selection = Selection.activeObject;
        if (selection == null)
            return;
        
        Mesh srcMesh = null;
        var go = selection as GameObject;
        if (go != null)
        {
            var filter = go.GetComponent<MeshFilter>();
            var skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
            if (filter == null && skinnedMeshRenderer == null)
                return;
            
            srcMesh = filter != null ? filter.sharedMesh : skinnedMeshRenderer.sharedMesh;
        }

        if (selection is Mesh mesh)
        {
            srcMesh = mesh;
        }

        if (srcMesh == null)
            return;

        CreateVertexPaintedClone(srcMesh, color, $"Assets/{srcMesh.name}_{colorName}.asset");
    }

    public static Mesh CreateVertexPaintedClone(Mesh srcMesh, Color color, string path)
    {
        var newMesh = Object.Instantiate(srcMesh);
        var colors = new Color[newMesh.vertexCount];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }

        newMesh.colors = colors;
        AssetDatabase.CreateAsset(newMesh, path);
        return newMesh;
    }
    
    public static AssetBundle GetAssetBundle(string path, string name)
    {
        var assetBundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(ab => ab.name == name);
        if (assetBundle == null)
        {
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(path, name));
        }

        return assetBundle;
    }

    public static void ExtractAnimations(Object[] assetsContainingAnimations, bool deleteOriginal)
    {
        foreach (var asset in assetsContainingAnimations)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            foreach (var animation in AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath).OfType<AnimationClip>())
            {
                var clone = Object.Instantiate(animation);
                
                // Make the name more clear since the generated name of the export contains both
                // the original file name and the name of the animation
                // TODO: change the format in SPICA
                var pathSplit = assetPath.Split('/');
                pathSplit[pathSplit.Length - 1] = asset.name
                    .Substring(asset.name.IndexOf("__", StringComparison.Ordinal) + 2) + ".asset";
                AssetDatabase.CreateAsset(clone, string.Join("/", pathSplit));
                if (deleteOriginal)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }
    }
    
    public static Transform FindDeepChild(this Transform parent, string name)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(parent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == name)
                return c;
            foreach(Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }    
    
    public static Transform FindDeepChildDepthFirst(this Transform parent, string name)
    {
        foreach(Transform child in parent)
        {
            if(child.name == name)
                return child;
            var result = child.FindDeepChildDepthFirst(name);
            if (result != null)
                return result;
        }
        return null;
    }
    #endif
}
