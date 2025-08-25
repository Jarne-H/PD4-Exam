using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using static DataTypes;

public class MazeGenerator : MonoBehaviour
{
    [Header("Web Service Settings")]
    [SerializeField]
    private string _webServiceUrl = "http://www.pd4-examwebservice.com/api"; // URL of the web service to save maze data
    [SerializeField]
    private string _imageURL = "http://www.pd4-examsite.com"; // URL of the web service to save maze images
    [SerializeField]
    private bool _useSwagger = true;
    [SerializeField]
    private string _swaggerURL = "http://localhost:5216/api";

    //list of both a tiletype and a string, using tupleto hold both values
    [SerializeField]
    private List<(TileType, string, Texture2D)> _tileTypesWithTheirTexture = new List<(TileType, string, Texture2D)>
    {
        (TileType.W, "bricks4.png", null),
        (TileType.T, "bricks2.png", null),
        (TileType.H, "bricks1.png", null) // Add a hole tile type with its texture name
    };

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
    [SerializeField]
    private GameObject _wallPrefab;
    [SerializeField]
    private GameObject _tilePrefab;
    [SerializeField]
    private GameObject _holePrefab;
    public List<Tile> tiles = new List<Tile>(); // List to hold the generated tiles
    //all the edited tiles will be stored in this list, so they can be saved to the web service
    public List<Tile> editedTiles = new List<Tile>(); // List to hold the edited tiles

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void GetMazeNameFromPage();
    private static extern void GetNewMazeNameFromPage();
#endif

    private void Start()
    {
        if(_useSwagger)
        {
            _webServiceUrl = _swaggerURL; // Use the Swagger URL if specified
        }
        string interop = CharSet.Auto.ToString(); // Ensure the interop is set to Unicode for compatibility
        Debug.Log($"MazeGenerator started with interop: {interop}"); // Log the interop setting
        // Correctly start the coroutines to download textures
        StartCoroutine(GetTexture(TileType.W).GetEnumerator()); // Load the texture for walls
        StartCoroutine(GetTexture(TileType.T).GetEnumerator()); // Load the texture for regular tiles
    }

    private IEnumerable GetTexture(TileType tileType)
    {
        //if the tiletype already has a texture in it's tuple, exit the function
        if (_tileTypesWithTheirTexture.Any(t => t.Item1 == tileType && t.Item3 != null))
        {
            yield break; // Exit if the texture is already loaded
        }
        //if the tiletype does not have a texture, get the texture from the web service
        string imageData = _tileTypesWithTheirTexture.FirstOrDefault(t => t.Item1 == tileType).Item2; // Get the texture name from the tuple
        if (string.IsNullOrEmpty(imageData))
        {
            Debug.LogError($"No texture name found for tile type {tileType}. Please check the _tileTypesWithTheirTexture list.");
            yield break; // Exit if no texture name is found
        }
        
        // Construct the URL to get the texture
        string textureUrl = $"{_imageURL}/images/{imageData}"; // Construct the URL to get the texture by name
        //Debug.Log($"Downloading texture for tile type {tileType} from URL: {textureUrl}"); // Log the URL from which the texture will be downloaded

        // Create a new UnityWebRequest to download the texture

        Debug.Log($"Starting texture download for tile type {tileType} from URL: {textureUrl}"); // Log the start of the texture download
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(textureUrl); // Create a UnityWebRequest to download the texture from the URL
        uwr.timeout = 10; // Set a timeout for the request to avoid hanging indefinitely
        Debug.Log($"Sending request to download texture for tile type {tileType} from URL: {textureUrl}"); // Log the URL being requested
        yield return uwr.SendWebRequest(); // Send the request and wait for it to complete
        Debug.Log($"Texture download completed for tile type {tileType} from URL: {textureUrl}"); // Log the completion of the texture download
        // Check for errors in the request
        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error downloading texture for tile type {tileType}: {uwr.error} at {textureUrl}"); // Log the error if the request fails
            yield break; // Exit if there is an error in the request
        }

        Debug.Log($"Texture downloaded successfully for tile type {tileType} from URL: {textureUrl}"); // Log the successful download of the texture


        // If the request is successful, get the texture from the download handler
        Texture2D texture = DownloadHandlerTexture.GetContent(uwr); // Get the texture from the download handler
        if (texture == null)
        {
            Debug.LogError($"Failed to load texture from URL: {textureUrl}");
            yield break; // Exit if the texture is null
        }
        // Store the texture in the tuple for future use
        _tileTypesWithTheirTexture = _tileTypesWithTheirTexture.Select(t => t.Item1 == tileType ? (t.Item1, t.Item2, texture) : t).ToList(); // Update the tuple with the loaded texture
        Debug.Log($"Texture loaded successfully for tile type {tileType} from URL: {textureUrl}"); // Log the successful loading of the texture
        yield return texture; // Return the loaded texture
    }


    //public void AdjustTileRenderFalloff()
    //{
    //    //go over all tiles and adjust the render falloff based on the tileProbability
    //    foreach (Tile tile in tiles)
    //    {
    //        if (tile != null && tile.tileType != TileType.W)
    //        {
    //            //if tile falloff is below the tileProbability, disable the tile
    //            if (tile.densityFalloff < _tileDensity.Value)
    //            {
    //                tile.gameObject.SetActive(true); // Enable the tile
    //            }
    //            else
    //            {
    //                tile.gameObject.SetActive(false); // Disable the tile
    //            }
    //        }
    //    }
    //    //enable the mazegenerators save button 
    //    GUI.enabled = true; // Enable the save button after adjusting the tiles
    //}

    // Method to save the maze to the web service

    public IEnumerator SaveMaze()
    {

#if UNITY_WEBGL && !UNITY_EDITOR
        GetMazeNameFromPage();
        GetNewMazeNameFromPage();
#endif

        //if newmazename is null or empty, use the maze name from the web page
        if (string.IsNullOrEmpty(newMazeNameJSValue))
        {
            newMazeNameJSValue = $"maze{_rows.Value}x{_columns.Value}-secondary"; // Use the maze name from the web page if available
        }

        //save the maze to the web service
        string mazename = "maze" + _rows.Value + "x"+ _columns.Value;
        //use mazename from the web page if it is not null or empty
        if (!string.IsNullOrEmpty(mazeNameJSValue))
        {
            mazename = mazeNameJSValue; // Use the maze name from the web page if available
        }

        string url = "";

        //get maze by mazename from the web service
        string originalMazeResponse = null; // Initialize the response variable
        yield return StartCoroutine(HTTPGetRequest($"{_webServiceUrl}/maze/get/by-name/{mazename}", r => originalMazeResponse = r)); // Use a coroutine to send the HTTP request asynchronously
        // Check if the response is empty or null
        if (string.IsNullOrEmpty(originalMazeResponse))
        {
            //if there is not a maze with the specified name, create a new maze
            ///CREATE A NEW ORIGINAL MAZE:
            //unity web request to post the maze data
            //string jsonData = JsonUtility.ToJson(new { name = mazename, rows = _rows.Value, columns = _columns.Value, tileDensity = _tileDensity.Value, tileOffset = _tileOffset.Value });
            var mazeData = new
            {
                name = mazename,
                rows = _rows.Value,
                columns = _columns.Value,
                tileDensity = _tileDensity.Value,
                tileOffset = _tileOffset.Value

            };
            string jsonData = JsonConvert.SerializeObject(mazeData); // Serialize the maze data to JSON using Newtonsoft.Json
            //Debug.Log($"Serialized maze data to JSON: {jsonData}"); // Log the serialized JSON data name = mazename, rows = _rows.Value, columns = _columns.Value, tileDensity = _tileDensity.Value, tileOffset = _tileOffset.Value
            Debug.Log($"Data to be sent: name = {mazename}, rows = {_rows.Value}, columns = {_columns.Value}, tileDensity = {_tileDensity.Value}, tileOffset = {_tileOffset.Value}");
            Debug.Log($"Posting new maze with name {mazename} to URL: {_webServiceUrl}/maze/post/maze{_rows.Value}x{_columns.Value},{_rows.Value},{_columns.Value} with data: {jsonData}");
            string response = null;
            //save original maze to the web service
            url = $"{_webServiceUrl}/maze/post/maze{_rows.Value}x{_columns.Value},{_rows.Value},{_columns.Value}"; // Construct the URL to post the maze data
            yield return StartCoroutine(HTTPPutRequest(url, jsonData, r => response = r)); // Use a coroutine to send the HTTP request asynchronously
            if (response.Contains("error"))
            {
                Debug.LogError($"Failed to create maze. Error response: {response}"); // Log the error response from the server
                yield break; // Exit the coroutine if there is an error in the response
            }

            Debug.Log($"Maze with name {mazename} does not exist. Creating a new maze."); // Log that a maze with the specified name does not exist
            url = $"{_webServiceUrl}/maze/post/"; // Construct the URL to create a new maze
        }
        else
        {
            ///if there already is a maze with that name, remove the modified tiles from the original maze and create a new maze with the edited tiles

            //get the mazeid of the original maze from the response
            Maze originalMaze = JsonConvert.DeserializeObject<Maze>(originalMazeResponse); // Deserialize the JSON response into a Maze object
            if (originalMaze == null || originalMaze.MazeId <= 0)
            {
                Debug.LogError($"Failed to parse original maze from response: {originalMazeResponse}"); // Log an error if the maze is null or has an invalid ID
                yield break; // Exit the coroutine if the maze is null or has an invalid ID
            }
            int mazeID = originalMaze.MazeId; // Get the maze ID from the parsed maze object
            //delete the tiles that were edited in the original maze
            foreach (Tile tile1 in editedTiles)
            {
                if (tile1 != null)
                {
                    //delete the tile from the web service/maze-tile/delete/{mazeID}/{x},{y}
                    string deleteTileUrl = $"{_webServiceUrl}/maze-tile/delete/{mazeID}/{tile1.row},{tile1.column}"; // Construct the URL to delete the tile by its row and column
                    yield return StartCoroutine(HTTPDeleteRequest(deleteTileUrl, r => { })); // Use a coroutine to send the HTTP request asynchronously
                    Debug.Log($"Tile at ({tile1.row}, {tile1.column}) deleted successfully from maze ID {mazeID}."); // Log the successful deletion of the tile
                }
            }

            //check if there is a maze with the new maze name, if so, delete it
            if (!string.IsNullOrEmpty(newMazeNameJSValue))
            {
                string newMazeUrl = $"{_webServiceUrl}/maze/get/by-name/{newMazeNameJSValue}"; // Construct the URL to get the maze by the new name
                string newMazeResponse = null; // Initialize the response variable
                yield return StartCoroutine(HTTPGetRequest(newMazeUrl, r => newMazeResponse = r)); // Use a coroutine to send the HTTP request asynchronously
                if (!string.IsNullOrEmpty(newMazeResponse))
                {
                    Maze newlyMadeMaze = JsonConvert.DeserializeObject<Maze>(newMazeResponse); // Deserialize the JSON response into a Maze object
                    if (newlyMadeMaze != null && newlyMadeMaze.MazeId > 0)
                    {
                        //delete the maze with the new maze name
                        string deleteNewMazeUrl = $"{_webServiceUrl}/maze/delete/by-id/{newlyMadeMaze.MazeId}"; // Construct the URL to delete the maze by its ID
                        yield return StartCoroutine(HTTPDeleteRequest(deleteNewMazeUrl, r => { })); // Use a coroutine to send the HTTP request asynchronously
                        Debug.Log($"Old maze with name {newMazeNameJSValue} deleted successfully."); // Log the successful deletion of the old maze
                    }

                    //delete the tiles of the old maze
                    url = $"{_webServiceUrl}/maze-tile/delete/all/{newlyMadeMaze.MazeId}"; // Construct the URL to delete all tiles of the old maze
                    string deleteTilesResponsewww = null; // Initialize the response variable
                    yield return StartCoroutine(HTTPDeleteRequest(url, r => deleteTilesResponsewww = r)); // Use a coroutine to send the HTTP request asynchronously
                }
            }

            //make new maze with new maze name, the edited tiles as their tiles and the original maze ID as the OriginalMazeId
            //maze/post/secondary-maze/{name},{width},{height},{originalMazeID}
            url = $"{_webServiceUrl}/maze/post/secondary-maze/{newMazeNameJSValue},{originalMaze.MazeId}";
            Debug.Log($"Posting new maze with name {newMazeNameJSValue} and original maze ID {originalMaze.MazeId} to URL: {url}");
            //post the maze to the web service
            string makeSecondaryMazeResponse = null;
            yield return StartCoroutine(HTTPPutRequest(url, null, r => makeSecondaryMazeResponse = r)); 
            //get the id of the new maze
            string newMazeIdUrl = $"{_webServiceUrl}/maze/get/by-name/{newMazeNameJSValue}"; 
            string newMazeIdResponse = null; // Initialize the response variable
            yield return StartCoroutine(HTTPGetRequest(newMazeIdUrl, r => newMazeIdResponse = r)); 

            if (string.IsNullOrEmpty(newMazeIdResponse))
            {
                Debug.LogError($"Failed to get new maze ID for maze {newMazeNameJSValue}. No response from the server. At: {newMazeIdUrl}"); // Log an error if the response is empty
                yield break; // Exit the coroutine if there is no response
            }

            // Parse the new maze ID from the response
            Maze newMaze = JsonConvert.DeserializeObject<Maze>(newMazeIdResponse); // Deserialize the JSON response into a Maze object

            //string tileUrl = $"{_webServiceUrl}/maze-tile/post/{mazeId}/{tile.row},{tile.column},{tile.tileType},{tile.densityFalloff}"; 
            foreach (Tile tile in editedTiles)
            {
                url = $"{_webServiceUrl}/maze-tile/post/{newMaze.MazeId}/{tile.row},{tile.column},{tile.tileType},{tile.densityFalloff}"; // Construct the URL to post the tile to the original maze
                string tileResponse = null; // Initialize the response variable
                yield return StartCoroutine(HTTPPutRequest(url, null, r => tileResponse = r)); // Use a coroutine to send the HTTP request asynchronously
                if(string.IsNullOrEmpty(tileResponse))
                {
                    Debug.LogError($"Failed to post tile at ({tile.row}, {tile.column}) to maze ID {newMaze.MazeId}. No response from the server. At: {url}"); // Log an error if the response is empty
                    yield break; // Exit the coroutine if there is no response
                }
            }


            //post a new maze with newmazename and use the id of the original maze as OriginalMazeId
            if (!string.IsNullOrEmpty(newMazeNameJSValue))
            {
                mazename = newMazeNameJSValue; // Use the new maze name from the web page if available
            }

            //delete all tiles of the original maze before saving the new maze
            DeleteAllTilesByMazeID(originalMaze.MazeId); // Call the method to delete all tiles of the original maze

            //save all the tiles of the original maze
            foreach (Tile tile in tiles)
            {
                if (tile != null)
                {
                    // Construct the URL to post the tile to the original maze
                    url = $"{_webServiceUrl}/maze-tile/post/{originalMaze.MazeId}/{tile.row},{tile.column},{tile.tileType},{tile.densityFalloff}"; 
                    string tileResponse = null; // Initialize the response variable
                    yield return StartCoroutine(HTTPPutRequest(url, null, r => tileResponse = r)); // Use a coroutine to send the HTTP request asynchronously
                    if (string.IsNullOrEmpty(tileResponse))
                    {
                        Debug.LogError($"Failed to post tile at ({tile.row}, {tile.column}) to maze ID {originalMaze.MazeId}. No response from the server. At: {url}"); // Log an error if the response is empty
                        yield break; // Exit the coroutine if there is no response
                    }
                }
            }

            //delete all tiles of the secondary maze
            DeleteAllTilesByMazeID(newMaze.MazeId); // Call the method to delete all tiles of the secondary maze

            //save all the tiles of the secondary maze
            foreach (Tile tile in editedTiles)
            {
                if (tile != null)
                {
                    // Construct the URL to post the tile to the secondary maze
                    url = $"{_webServiceUrl}/maze-tile/post/{newMaze.MazeId}/{tile.row},{tile.column},{tile.tileType},{tile.densityFalloff}"; 
                    string tileResponse = null; // Initialize the response variable
                    yield return StartCoroutine(HTTPPutRequest(url, null, r => tileResponse = r)); // Use a coroutine to send the HTTP request asynchronously
                    if (string.IsNullOrEmpty(tileResponse))
                    {
                        Debug.LogError($"Failed to post tile at ({tile.row}, {tile.column}) to maze ID {newMaze.MazeId}. No response from the server. At: {url}"); // Log an error if the response is empty
                        yield break; // Exit the coroutine if there is no response
                    }
                }
            }

        }
        //save all the tiles to their respective mazes
    }

    private IEnumerable DeleteAllTilesByMazeID(int mazeId)
    {
        //get all tiles with the same maze ID
        string tilesUrl = $"{_webServiceUrl}/maze-tile/delete/all/{mazeId}"; // Construct the URL to get the tiles for the maze
        string tilesResponse = null; // Initialize the response variable
        // Use a coroutine to send the HTTP request asynchronously
        yield return StartCoroutine(HTTPDeleteRequest(tilesUrl, r => tilesResponse = r));
        if (string.IsNullOrEmpty(tilesResponse))
        {
            Debug.LogError($"Failed to delete tiles for maze ID {mazeId}. No response from the server. At: {tilesUrl}");
            yield break; // Exit the coroutine if the response is empty
        }
        Debug.Log($"Tiles deleted successfully for maze ID {mazeId}. Response: {tilesResponse}"); // Log the successful deletion of tiles
    }

    private IEnumerator HTTPPutRequest(string url, string jsonresult, System.Action<string> callback)
    {
        UnityWebRequest request;
        if (jsonresult == null)
        {
            request = UnityWebRequest.PostWwwForm(url, (string)null);
        }
        else
        {
            request = UnityWebRequest.Put(url, jsonresult);
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonresult));
            request.SetRequestHeader("Content-Type", "application/json");
            request.method = UnityWebRequest.kHttpVerbPOST;
        }
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = 10;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {request.error} at {url}");
            callback?.Invoke(null);
        }
        else
        {
            callback?.Invoke(request.downloadHandler.text);
        }
    }

    //get request to get the maze ID from the database
    private IEnumerator HTTPGetRequest(string url, System.Action<string> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = 10;
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {request.error} at {url}");
            callback?.Invoke(null);
        }
        else
        {
            callback?.Invoke(request.downloadHandler.text);
        }
    }

    //delete request to delete a tile by its ID
    private IEnumerator HTTPDeleteRequest(string url, System.Action<string> callback)
    {
        UnityWebRequest request = UnityWebRequest.Delete(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = 10;
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {request.error} at {url}");
            callback?.Invoke(null);
        }
        else
        {
            Debug.Log($"Response: {request.downloadHandler.text} at {url}");
            callback?.Invoke(request.downloadHandler.text);
        }
    }

    // Method to create the maze
    public void CreateMaze()
    {
        DestroyAllPreviousTiles();


        //http call to maze/post/maze
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
            tileType = TileType.W;
        }
        else
        {
            // Otherwise, create a regular tile with a random type
            tileType = TileType.T;
        }

        GameObject tileObject = null; // Initialize the GameObject to null
        // Instantiate the tile prefab at the specified position
        if (tileType == TileType.W)
        {
            tileObject = Instantiate(_wallPrefab, new Vector3(column, 0, row), Quaternion.identity, transform);
        }
        else if (tileType == TileType.T)
        {
            tileObject = Instantiate(_tilePrefab, new Vector3(column, 0, row), Quaternion.identity, transform);
        }
        else
        {
            tileObject = Instantiate(_holePrefab, new Vector3(column, 0, row), Quaternion.identity, transform);
        }

        // Get the Tile component from the instantiated object
        Tile tile = tileObject.GetComponent<Tile>();

        // If the Tile component is found, set its properties
        if (tile != null)
        {
            //create random density between 0 and 1 for the tile
            int density = (int)UnityEngine.Random.Range(1f, 100f);
            tile.CreateTile(row, column, tileType, density);

            //set the texture of the tile based on its type
            tile.GetComponentInChildren<Renderer>().material.mainTexture = _tileTypesWithTheirTexture.FirstOrDefault(t => t.Item1 == tile.tileType).Item3; // Set the texture of the tile based on its type

            tiles.Add(tile); // Add the tile to the list
                             // Adjust the tile position based on the offset and keep the middle of the maze at (0, 0)
            tileObject.transform.position = new Vector3(column * _tileOffset.Value - (_columns.Value * _tileOffset.Value / 2), 0, row * _tileOffset.Value - (_rows.Value * _tileOffset.Value / 2));
            //if the tile is not a wall and the density is below the tileProbability, disable the tile
            //if (tile.tileType != TileType.W && tile.densityFalloff > _tileDensity.Value)
            //{
            //    tile.gameObject.SetActive(false); // Disable the tile if it doesn't meet the probability
            //}
        }
        else
        {
            Debug.LogError("Tile component not found on the tile prefab.");
        }

        TileClicker tileClicker = tileObject.GetComponent<TileClicker>();

        if(tileClicker != null)
        {
            tileClicker = tileObject.AddComponent<TileClicker>(); // Add the TileClicker component if not already present
            tileClicker.mazeGenerator = this; // Set the reference to the MazeGenerator
        }
        else
        {
            Debug.LogError("TileClicker component not found on the tile prefab.");
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

    private string mazeNameJSValue;
    private string newMazeNameJSValue;

    public void ReceiveMazeNameFromWebPage(string value)
    {
        mazeNameJSValue = value; // Store the value received from the web page
    }

    public void ReceiveNewMazeNameFromWebPage(string value)
    {
        newMazeNameJSValue = value; // Store the value received from the web page
    }

    public IEnumerator LoadMaze()
    {
        ///FIXME: the maze name should be read from the text input field in the website
        ///                                  /api/maze/get/by-name/{name}
        ///                                  
#if UNITY_WEBGL && !UNITY_EDITOR
        GetMazeNameFromPage();
        GetNewMazeNameFromPage(); // Call the JavaScript function to get the maze name from the web page
#endif

        string url = $"{_webServiceUrl}/maze/get/by-name/maze{_rows.Value}x{_columns.Value}";
        //combination of the above code and the new code
        //if jsvalue is not null or empty, use it to get the maze by name
        if (!string.IsNullOrEmpty(mazeNameJSValue))
        {
            url = $"{_webServiceUrl}/maze/get/by-name/{mazeNameJSValue}"; // Use the jsvalue to get the maze by name
        }

        string mazeData = null;
        yield return StartCoroutine(HTTPGetRequest(url, r => mazeData = r)); // Use a coroutine to send the HTTP request asynchronously

        if (string.IsNullOrEmpty(mazeData))
        {
            Debug.LogError("Failed to load maze data. No response from the server. At: " + url);
            Debug.Log($"recieved data: {mazeData}");
            yield break;
        }
        Debug.Log($"Maze data loaded successfully: {mazeData}");
        //get the maze from the maze data
        Maze maze = JsonConvert.DeserializeObject<Maze>(mazeData); // Deserialize the JSON response into a Maze object

        // Generate the loaded maze with the tiles and tile density
        GenerateLoadedMaze(maze.MazeTiles, maze.Density); // Use the density from the loaded maze data
        // Set the rows and columns values based on the loaded maze data
        _rows.Value = maze.MazeTiles.Max(mt => mt.RowIndex) + 1; // Add 1 because row and column indices are zero-based
        _columns.Value = maze.MazeTiles.Max(mt => mt.ColumnIndex) + 1; // Add 1 because row and column indices are zero-based
        _tileDensity.Value = maze.Density; // Set the tile density value from the maze data


    }

    public void GenerateLoadedMaze(MazeTile[] loadedMazeTiles, float tileDensity)
    {
        DestroyAllPreviousTiles(); // Clear existing tiles before loading new ones
        // Iterate through the loaded tiles and create them in the scene
        foreach (MazeTile loadedTile in loadedMazeTiles)
        {
            if (loadedTile != null)
            {
                GameObject tileObject = null; // Initialize the GameObject to null
                // Determine the tile type based on the loaded data
                if(loadedTile.TileType == "W")
                {
                    tileObject = Instantiate(_wallPrefab, new Vector3(loadedTile.ColumnIndex, 0, loadedTile.RowIndex), Quaternion.identity, transform);
                }
                else if (loadedTile.TileType == "T")
                {
                    tileObject = Instantiate(_tilePrefab, new Vector3(loadedTile.ColumnIndex, 0, loadedTile.RowIndex), Quaternion.identity, transform);
                }
                else
                {
                    tileObject = Instantiate(_holePrefab, new Vector3(loadedTile.ColumnIndex, 0, loadedTile.RowIndex), Quaternion.identity, transform);
                }

                // Get the Tile component from the instantiated object
                Tile tile = tileObject.GetComponent<Tile>();
                // If the Tile component is found, set its properties
                if (tile != null)
                {
                    tile.CreateTile(loadedTile.RowIndex, loadedTile.ColumnIndex, (TileType)Enum.Parse(typeof(TileType), loadedTile.TileType), loadedTile.DensityFallOff);
                    //use texture from the tuple
                    tile.GetComponentInChildren<Renderer>().material.mainTexture = _tileTypesWithTheirTexture.FirstOrDefault(t => t.Item1 == tile.tileType).Item3; // Set the texture of the tile based on its type

                    tiles.Add(tile); // Add the tile to the list
                    // Adjust the tile position based on the offset and keep the middle of the maze at (0, 0)
                    tileObject.transform.position = new Vector3(loadedTile.ColumnIndex * _tileOffset.Value - (_columns.Value * _tileOffset.Value / 2), 0, loadedTile.RowIndex * _tileOffset.Value - (_rows.Value * _tileOffset.Value / 2));
                }
                else
                {
                    Debug.LogError("Tile component not found on the tile prefab.");
                }
            }
        }
        //AdjustTileRenderFalloff(); // Adjust tiles based on the current density setting
    }

    public void OnSaveMazeButtonClicked()
    {
        StartCoroutine(SaveMaze());
    }

    public void OnLoadMazeButtonClicked()
    {
        StartCoroutine(LoadMaze());
    }

    public void AdjustTile(int row, int column, TileType newType, int densityFallOff)
    {
        //get the tile at the specified row and column
        Tile tile = tiles.FirstOrDefault(t => t.row == row && t.column == column);
        if (tile != null)
        {
            //delete the old object and create a new one with the new type and density falloff
            Destroy(tile.gameObject); // Destroy the old tile GameObject
            //remove it from the list
            tiles.Remove(tile); // Remove the old tile from the list
            // Create a new tile with the specified type and density falloff
            GameObject newTileObject = null; // Initialize the new GameObject to null
            if (newType == TileType.W)
            {
                newTileObject = Instantiate(_wallPrefab, new Vector3(column, 0, row), Quaternion.identity, transform);
            }
            else if (newType == TileType.T)
            {
                newTileObject = Instantiate(_tilePrefab, new Vector3(column, 0, row), Quaternion.identity, transform);
            }
            else
            {
                newTileObject = Instantiate(_holePrefab, new Vector3(column, 0, row), Quaternion.identity, transform);
            }
            // Get the Tile component from the new instantiated object
            Tile newTile = newTileObject.GetComponent<Tile>();
            // If the Tile component is found, set its properties
            if(newTile != null)
            {
                newTile.CreateTile(row, column, newType, densityFallOff); // Create the new tile with the specified properties
                // Set the texture of the new tile based on its type
                newTile.GetComponentInChildren<Renderer>().material.mainTexture = _tileTypesWithTheirTexture.FirstOrDefault(t => t.Item1 == newTile.tileType).Item3;
                tiles.Add(newTile); // Add the new tile to the list
                // Adjust the tile position based on the offset and keep the middle of the maze at (0, 0)
                newTileObject.transform.position = new Vector3(column * _tileOffset.Value - (_columns.Value * _tileOffset.Value / 2), 0, row * _tileOffset.Value - (_rows.Value * _tileOffset.Value / 2));
            }
            else
            {
                Debug.LogError("Tile component not found on the new tile prefab.");
            }
        }
        else
        {
            Debug.LogWarning($"No tile found at ({row}, {column}) to adjust.");
        }
    }
}



[System.Serializable]
public class Maze
{
    [JsonProperty("mazeId")]
    public int MazeId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("creationDate")]
    public string CreationDate { get; set; }

    [JsonProperty("density")]
    public int Density { get; set; }

    public int? OriginalMazeId { get; set; }

    [JsonProperty("gameSessions")]
    public GameSession[] GameSessions { get; set; }

    [JsonProperty("mazeTiles")]
    public MazeTile[] MazeTiles { get; set; }
}
//-----------------------------------

[Serializable]
public class MazeData
{
    public int mazeId;
    public string name;
    public string creationDate;
    public List<TileData> mazeTiles;
    public int density; // Density of the maze, can be used for rendering or other purposes
}

[Serializable]
public class TileData
{
    public int tileId;
    public int rowIndex;
    public int columnIndex;
    public TileType tileType;
    public int mazeId;
    public int falloff;
}
//-----------------------------------

[Serializable]
public class MazeTile
{
    [JsonProperty("tileId")]
    public int TileId { get; set; }

    [JsonProperty("rowIndex")]
    public int RowIndex { get; set; }

    [JsonProperty("columnIndex")]
    public int ColumnIndex { get; set; }

    [JsonProperty("tileType")]
    public string TileType { get; set; }

    [JsonProperty("densityFallOff")]
    public int DensityFallOff { get; set; }
}
