using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DataTypes : MonoBehaviour
{
    public partial class Maze
    {
        public int MazeId { get; set; }

        public string Name { get; set; } = null!;

        public DateTime CreationDate { get; set; }

        public double Density { get; set; }

        public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

        public virtual ICollection<MazeTile> MazeTiles { get; set; } = new List<MazeTile>();
    }

    [System.Serializable]
    public partial class MazeTile
    {
        [JsonProperty("tileId")]
        public int TileId { get; set; }

        [JsonProperty("rowIndex")]
        public int RowIndex { get; set; }

        [JsonProperty("columnIndex")]
        public int ColumnIndex { get; set; }

        [JsonProperty("tileType")]
        public string TileType { get; set; } = null!;

        [JsonProperty("mazeId")]
        public int MazeId { get; set; }

        [JsonProperty("densityFallOff")]
        public double DensityFallOff { get; set; }

        public virtual Maze Maze { get; set; } = null!;
    }

    public partial class GameSession
    {
        public int GameSessionId { get; set; }

        public int MazeId { get; set; }

        public int PlayerId { get; set; }

        public DateTime StartTime { get; set; }

        public virtual Maze Maze { get; set; } = null!;

        public virtual Player Player { get; set; } = null!;
    }


    //public partial class Image
    //{

    //    public int ImageId { get; set; }

    //    public string Name { get; set; } = null!;

    //    public string Link { get; set; } = null!;
    //}
    [System.Serializable]
    //IMAGE CLASS
    public partial class Image
    {
        [JsonProperty("imageId")]
        public int ImageId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = null!;
        [JsonProperty("link")]
        public string Link { get; set; } = null!;
    }

    public partial class Player
    {
        public int PlayerId { get; set; }

        public string Name { get; set; } = null!;

        public DateTime CreationDate { get; set; }

        public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    }
}