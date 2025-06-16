using UnityEngine;
//all values a tile can have
public enum TileType
{
    W,
    T
}

//base template for a tile
public class Tile : MonoBehaviour
{
    public TileType tileType = TileType.T; //default tile type
    public int row; //row position in the maze
    public int column; //column position in the maze
    public int densityFalloff; //the number at which this tile will not be shown

    // Method to set the tile type
    public void SetTileType(TileType type)
    {
        tileType = type;
        // Update the visual representation based on the tile type if needed
    }

    public void CreateTile(int row, int column, TileType type, int densityFalloff)
    {
        this.row = row;
        this.column = column;
        SetTileType(type);
        this.densityFalloff = densityFalloff; // Set the density falloff value
    }

    public void AdjustTile()
    {
        // Optionally, initialize the tile here if needed

    }
}