using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

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

            obj.path = new FileInfo(filename).Directory.FullName;

            CreateLevel(obj);


        }
    }

    private void CreateLevel(TiledLevel obj)
    {
        FixTilesets(obj.tilesets, obj.path);

        var tileCount = obj.tilesets.Sum(t => t.tilecount) + 1;
        Sprite[] tileSprites = new Sprite[tileCount];
        zDepths = new Dictionary<int, int>();

        int i = 0;

        foreach (var tileset in obj.tilesets)
        {
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

        }
        
        var importedLevel = new GameObject("Tiled Level");

        foreach (var layer in obj.layers)
        {
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

                    var renderer = tile.AddComponent<SpriteRenderer>();
                    renderer.sprite = tileSprites[spriteIndex];
                    renderer.sortingOrder = zDepths[spriteIndex];
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
                tilesets[i] = LoadTileset(Path.Combine(path, tilesets[i].source));
                tilesets[i].firstgid = firstgid;
            }
        }

    }

    private Tileset LoadTileset(string source)
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
            var id = int.Parse(tileNode.Attributes.GetNamedItem("id").Value);
            var zNode = tileNode["properties"].ChildNodes.Cast<XmlNode>().Where(x => x.Name == "property" && x.Attributes.GetNamedItem("name").Value == "z").FirstOrDefault();
            Debug.Log(zNode);
            if (zNode != null)
            {
                Debug.Log(id);
                zDepths.Add(id, int.Parse(zNode.Attributes.GetNamedItem("value").Value));
            }

        }

        return tileset;
    }
}
