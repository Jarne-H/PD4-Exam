using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MazeGenerator : MonoBehaviour
{

    [Header("Maze Generation Settings")]

    //rows, columns and an array to hold the tile data
    public int rows = 10;
    public int columns = 10;
    public GameObject tilePrefab;

    public float tileOffset = 2.0f; // Offset for the tile position, can be adjusted if needed

    //adjustable parameters for the maze generation
    [Range(0, 100)]
    public int tileProbability = 80; // Probability of a regular tile

    //if the slider is changed, adjust which tiles are rendered
    private void OnValidate()
    {
        // Ensure the tile probability is within the valid range
        tileProbability = Mathf.Clamp(tileProbability, 0, 100);
        AdjustTileRenderFalloff(); // Adjust the tiles based on the new probability
    }

    public void AdjustTileRenderFalloff()
    {
        //go over all tiles and adjust the render falloff based on the tileProbability
        foreach (Tile tile in tiles)
        {
            if (tile != null)
            {
                //if tile falloff is below the tileProbability, disable the tile
                if (tile.densityFalloff < tileProbability / 100f)
                {
                    tile.gameObject.SetActive(true); // Enable the tile
                }
                else
                {
                    tile.gameObject.SetActive(false); // Disable the tile
                }
            }
        }
    }


    public List<Tile> tiles = new List<Tile>(); // List to hold the generated tiles


    //custom inspector button to create the maze in the editor, multi object editing not supported
    [CustomEditor(typeof(MazeGenerator)), CanEditMultipleObjects]
    public class MazeGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MazeGenerator mazeGenerator = (MazeGenerator)target;
            if (GUILayout.Button("Create Maze"))
            {
                mazeGenerator.CreateMaze();
            }
        }
    }

    // Method to create the maze
    public void CreateMaze()
    {
        // Clear existing tiles
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
            Debug.Log("Destroyed another child!");
        }
        // Generate the maze all tiles on the outside are walls, the est are randomly generated tiles
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                TileType tileType = (row == 0 || row == rows - 1 || column == 0 || column == columns - 1) ? TileType.Wall : 
                                    (Random.Range(0, 100) < tileProbability) ? TileType.Tile : TileType.Wall;
                GameObject tileObject = Instantiate(tilePrefab, new Vector3(column, 0, row), Quaternion.identity, transform);
                Tile tile = tileObject.GetComponent<Tile>();
                if (tile != null)
                {
                    //create random density between 0 and 1 for the tile
                    float density = Random.Range(0f, 100f);
                    tile.CreateTile(row, column, tileType, density);
                    tiles.Add(tile); // Add the tile to the list
                    // Adjust the tile position based on the offset and keep the middle of the maze at (0, 0)
                    tileObject.transform.position = new Vector3(column * tileOffset - (columns * tileOffset / 2), 0, row * tileOffset - (rows * tileOffset / 2));
                }
                else
                {
                    Debug.LogError("Tile component not found on the tile prefab.");
                }
            }
        }
    }
}
