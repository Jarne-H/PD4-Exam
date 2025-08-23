using UnityEngine;

public class TileClicker : MonoBehaviour
{
    [SerializeField]
    private MazeGenerator _mazeGenerator; // Reference to the MazeGenerator script

    //on click, draw ray from camera to mouse position
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 0 is left mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the clicked object has a Tile component
                Tile tile = hit.collider.gameObject.GetComponent<Tile>();
                if (tile != null)
                {
                    //get next tile type in line with the TileType enum
                    TileType nextTileType = (TileType)(((int)tile.tileType + 1) % System.Enum.GetValues(typeof(TileType)).Length);
                    _mazeGenerator.AdjustTile(tile.row, tile.column, nextTileType, tile.densityFalloff); // Call the AdjustTile method from MazeGenerator with the clicked tile
                }
                else
                {
                    Debug.Log("Clicked object is not a Tile.");
                }
            }
        }
    }
}
