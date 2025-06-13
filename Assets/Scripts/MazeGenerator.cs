using System.Collections.Generic;
using System.Web;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MazeGenerator : MonoBehaviour
{
    [Header("Web Service Settings")]
    [SerializeField]
    private string webServiceUrl = "http://www.pd4-examwebservice.com/"; // URL of the web service to save maze data
    [SerializeField]
    private string getMazeEndpoint = "getMaze"; // Endpoint to retrieve maze data


    [Header("Maze Settings")]
    [SerializeField]
    private string mazeName = "LongTightMaze"; // Name of the maze, can be set in the inspector
    [Header("Maze Generation Settings")]

    //rows, columns and an array to hold the tile data
    [SerializeField]
    private MazeValueSlider _rows;
    [SerializeField]
    private MazeValueSlider _columns;
    [SerializeField]
    private MazeValueSlider _tileDensity;
    [SerializeField]
    private MazeValueSlider _tileOffset;
    public GameObject tilePrefab;

    public void AdjustTileRenderFalloff()
    {
        //go over all tiles and adjust the render falloff based on the tileProbability
        foreach (Tile tile in tiles)
        {
            if (tile != null && tile.tileType != TileType.Wall)
            {
                //if tile falloff is below the tileProbability, disable the tile
                if (tile.densityFalloff < _tileDensity.Value)
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
        for (int row = 0; row < _rows.Value; row++)
        {
            for (int column = 0; column < _columns.Value; column++)
            {
                CreateNewTile(row, column);
            }
        }
    }

    private void CreateNewTile(int row, int column)
    {
        // Determine the tile type based on its position
        TileType tileType;
        if (row == 0 || row == _rows.Value - 1 || column == 0 || column == _columns.Value - 1)
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
            int density = (int)Random.Range(1f, 100f);
            tile.CreateTile(row, column, tileType, density);
            tiles.Add(tile); // Add the tile to the list
                             // Adjust the tile position based on the offset and keep the middle of the maze at (0, 0)
            tileObject.transform.position = new Vector3(column * _tileOffset.Value - (_columns.Value * _tileOffset.Value / 2), 0, row * _tileOffset.Value - (_rows.Value * _tileOffset.Value / 2));
            //if the tile is not a wall and the density is below the tileProbability, disable the tile
            if (tile.tileType != TileType.Wall && tile.densityFalloff > _tileDensity.Value)
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
        tiles.Clear(); // Clear the list of tiles after destruction
    }


    public void LoadMaze()
    {
        //Call https://example.com/maze-data to get the maze data
        //Get the json data from the URL and parse it
        //Tile[] loadedTiles = the loaded tiles array
        //float tineDensity = the loaded tile density value
        //GenerateLoadedMaze();
        //HTTP call to http://www.pd4-examwebservice.com/ with a get request to retrieve the maze data

    }

    public void GenerateLoadedMaze(Tile[] loadedTiles, float tileDensity)
    {
        DestroyAllPreviousTiles(); // Clear existing tiles before loading new ones
        // Iterate through the loaded tiles and create them in the scene
        foreach (Tile loadedTile in loadedTiles)
        {
            if (loadedTile != null)
            {
                // Create a new tile instance
                GameObject tileObject = Instantiate(tilePrefab, loadedTile.transform.position, Quaternion.identity, transform);
                Tile tile = tileObject.GetComponent<Tile>();
                if (tile != null)
                {
                    // Set the properties of the new tile
                    tile.CreateTile(loadedTile.row, loadedTile.column, loadedTile.tileType, loadedTile.densityFalloff);
                    tiles.Add(tile); // Add the new tile to the list
                    // Adjust the position based on the offset
                    tileObject.transform.position = new Vector3(loadedTile.column * _tileOffset.Value - (_columns.Value * _tileOffset.Value / 2), 0, loadedTile.row * _tileOffset.Value - (_rows.Value * _tileOffset.Value / 2));
                    // Disable the tile if its density falloff is above the specified threshold
                    if (tile.tileType != TileType.Wall && tile.densityFalloff > tileDensity)
                    {
                        tile.gameObject.SetActive(false); // Disable the tile if it doesn't meet the probability
                    }
                }
                else
                {
                    Debug.LogError("Tile component not found on the loaded tile prefab.");
                }
            }
        }
        AdjustTileRenderFalloff(); // Adjust tiles based on the current density setting
    }
}
