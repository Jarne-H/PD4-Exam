using UnityEngine;

public class TileClicker : MonoBehaviour
{
    [SerializeField]
    private MazeGenerator _mazeGenerator; // Reference to the MazeGenerator script
    public MazeGenerator mazeGenerator { get { return _mazeGenerator; } set { _mazeGenerator = value; } }

    private void Start()
    {
        if (_mazeGenerator == null)
        {
            _mazeGenerator = FindAnyObjectByType<MazeGenerator>();
        }
    }

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
                    //debug log all info of the time
                    Debug.Log($"Clicked Tile at Row: {tile.row}, Column: {tile.column}, Current Type: {tile.tileType}, Next Type: {nextTileType}, Density Falloff: {tile.densityFalloff}");
                }
                else
                {
                    Debug.Log("Clicked object is not a Tile.");
                }
            }
        }
    }
}
