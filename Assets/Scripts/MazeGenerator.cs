using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField]
    private string mazeName = "LongTightMaze"; // Name of the maze, can be set in the inspector
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
            if (tile != null && tile.tileType != TileType.Wall)
            {
                //if tile falloff is below the tileProbability, disable the tile
                if (tile.densityFalloff < tileProbability)
                {
                    tile.gameObject.SetActive(true); // Enable the tile
                }
                else
                {
                    tile.gameObject.SetActive(false); // Disable the tile
                }
            }
        }
        //enable the mazegenerators save button 
        GUI.enabled = true; // Enable the save button after adjusting the tiles
    }


    public List<Tile> tiles = new List<Tile>(); // List to hold the generated tiles

    /// <summary>
    /// Buttons to create and save the maze in the editor.
    /// </summary>

    //custom inspector button to create the maze in the editor, multi object editing not supported
    [CustomEditor(typeof(MazeGenerator)), CanEditMultipleObjects]
    public class MazeGeneratorGenerate : Editor
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

    //save button
    [CustomEditor(typeof(MazeGenerator)), CanEditMultipleObjects]
    public class MazeGeneratorSave : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MazeGenerator mazeGenerator = (MazeGenerator)target;
            if (GUILayout.Button("Save to Database"))
            {
                mazeGenerator.SaveToDatabase();
            }
        }
    }

    //Method to save to html sql database
    public void SaveToDatabase()
    {
        Debug.Log("Saving maze to database...");

        // Implement the logic to save the maze data to an HTML SQL database
        // This is a placeholder for your database saving logic
        Debug.Log("Maze saved to database (placeholder).");
    }

    // Method to create the maze
    public void CreateMaze()
    {
        DestroyAllPreviousTiles();

        // Generate the maze all tiles on the outside are walls, the est are randomly generated tiles
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                CreateNewTile(row, column);
            }
        }
    }

    private void CreateNewTile(int row, int column)
    {
        // Determine the tile type based on its position
        TileType tileType;
        if (row == 0 || row == rows - 1 || column == 0 || column == columns - 1)
        {
            // If it's on the border, create a wall tile
            tileType = TileType.Wall;
        }
        else
        {
            // Otherwise, create a regular tile with a random type
            tileType = TileType.Tile;
        }

        // Instantiate the tile prefab at the specified position
        GameObject tileObject = Instantiate(tilePrefab, new Vector3(column, 0, row), Quaternion.identity, transform);

        // Get the Tile component from the instantiated object
        Tile tile = tileObject.GetComponent<Tile>();

        // If the Tile component is found, set its properties
        if (tile != null)
        {
            //create random density between 0 and 1 for the tile
            int density = (int)Random.Range(0f, 100f);
            tile.CreateTile(row, column, tileType, density);
            tiles.Add(tile); // Add the tile to the list
                             // Adjust the tile position based on the offset and keep the middle of the maze at (0, 0)
            tileObject.transform.position = new Vector3(column * tileOffset - (columns * tileOffset / 2), 0, row * tileOffset - (rows * tileOffset / 2));
            //if the tile is not a wall and the density is below the tileProbability, disable the tile
            if (tile.tileType != TileType.Wall && tile.densityFalloff > tileProbability)
            {
                tile.gameObject.SetActive(false); // Disable the tile if it doesn't meet the probability
            }
        }
        else
        {
            Debug.LogError("Tile component not found on the tile prefab.");
        }
    }

    private void DestroyAllPreviousTiles()
    {
        // Destroy all previously generated tiles
        foreach (Tile tile in tiles)
        {
            if (tile != null)
            {
                DestroyImmediate(tile.gameObject); // Use DestroyImmediate for editor scripts
            }
        }
        tiles.Clear(); // Clear the list of tiles
    }
}
