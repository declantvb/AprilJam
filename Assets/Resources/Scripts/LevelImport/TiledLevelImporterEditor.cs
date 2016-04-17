#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

using UnityEditor;

[CustomEditor(typeof(TiledLevelImporter))]
public class TiledLevelImporterEditor : Editor
{
    public Dictionary<int, int> zDepths;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Import Tiled Level"))
        {
            string filename = EditorUtility.OpenFilePanel("Select Tiled level JSON", ".", "json");

            var file = new StreamReader(File.OpenRead(filename));
            var fileText = file.ReadToEnd();

            var obj = JsonUtility.FromJson<TiledLevel>(fileText);

            obj.path = new FileInfo(filename).DirectoryName;

            CreateLevel(obj);


        }
    }

    private void CreateLevel(TiledLevel obj)
    {
        zDepths = new Dictionary<int, int>();

        // If any tilesets are remote references, go find those tilesets and load them
        FixTilesets(obj.tilesets, obj.path);

        var tileCount = obj.tilesets.Sum(t => t.tilecount) + 1;
        Sprite[] tileSprites = new Sprite[tileCount];

        int i = 0;

        // Load tilesets
        foreach (var tileset in obj.tilesets)
        {
            Debug.Log("Reading tileset...");
            var tilesetToUse = tileset;

            i = tilesetToUse.firstgid;
            
            var query = tilesetToUse.image.Split('\\', '/').Last().Split('.').First() + " t:sprite";
            Debug.Log(query);
            var assetPath = AssetDatabase.FindAssets(query);
            Debug.Log(assetPath.First());


            if (assetPath.Any())
            {
                var spriteSheet = AssetDatabase.GUIDToAssetPath(assetPath.First());

                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet)
                    .OfType<Sprite>().ToArray();

                foreach (var sprite in sprites)
                {
                    tileSprites[i++] = sprite;
                }
            }
            else
            {
                Debug.LogError("Couldn't find sprites - " + tilesetToUse.image);
            }

        }
        
        var importedLevel = new GameObject("Tiled Level");


        // Load the layers
        for (int layerId = obj.layers.Length - 1; layerId >= 0; layerId--)
        {
            var layer = obj.layers[layerId];

            GameObject layerObj = new GameObject(layer.name);
            layerObj.transform.parent = importedLevel.transform;


            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    var dataIndex = layer.width * y + x;
                    var spriteIndex = layer.data[dataIndex];

                    if (spriteIndex == 0)
                        continue;

                    GameObject tile = new GameObject("Tile");

                    tile.transform.parent = layerObj.transform;
                    tile.transform.position = new Vector2(x, -y);
                    tile.isStatic = true;

                    var renderer = tile.AddComponent<SpriteRenderer>();
                    renderer.sprite = tileSprites[spriteIndex];
                    renderer.sortingOrder = zDepths[spriteIndex];

                    // Anything on layer 0 should have a collider
                    if (zDepths[spriteIndex] == 0)
                    {
                        var collider = tile.AddComponent<BoxCollider2D>();
                        collider.size = new Vector2(1, 1.4f);
                        
                    }
                }
            }
        }
    }

    private void FixTilesets(Tileset[] tilesets, string path)
    {
        for (int i = 0; i < tilesets.Length; i++)
        {
            if (!string.IsNullOrEmpty(tilesets[i].source))
            {
                var firstgid = tilesets[i].firstgid;
                tilesets[i] = LoadTileset(Path.Combine(path, tilesets[i].source), firstgid);
            }
        }

    }

    private Tileset LoadTileset(string source, int firstgid)
    {
        Debug.Log(source);

        var tileset = new Tileset();

        var contents = new StreamReader(File.OpenRead(source)).ReadToEnd();

        var xdoc = new XmlDocument();
        xdoc.LoadXml(contents);
        var tsNode = xdoc.GetElementsByTagName("tileset")[0];

        tileset.tilecount = int.Parse(tsNode.Attributes.GetNamedItem("tilecount").Value);
        tileset.image = tsNode["image"].Attributes.GetNamedItem("source").Value;

        var tileNodes = xdoc.GetElementsByTagName("tile");
        foreach (XmlNode tileNode in tileNodes)
        {
            var id = int.Parse(tileNode.Attributes.GetNamedItem("id").Value) + firstgid;
            var zNode = tileNode["properties"].ChildNodes.Cast<XmlNode>().Where(x => x.Name == "property" && x.Attributes.GetNamedItem("name").Value == "z").FirstOrDefault();
            Debug.Log(zNode);
            if (zNode != null)
            {
                Debug.Log(id);
                Debug.Log(zDepths);
                
                zDepths.Add(id, int.Parse(zNode.Attributes.GetNamedItem("value").Value));
            }

        }

        tileset.firstgid = firstgid;

        return tileset;
    }
}


#endif