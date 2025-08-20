mergeInto(LibraryManager.library, {
  GetMazeNameFromPage: function () {
    var inputValue = parent.document.getElementById("mazeName").value;
    SendMessage("MazeTileGenerator", "RecieveMazeNameFromWebPage", inputValue);

  },
  GetNewMazeNameFromPage: function () {
    var inputValue = parent.document.getElementById("newMazeName").value;
    SendMessage("MazeTileGenerator", "RecieveNewMazeNameFromWebPage", inputValue);

  }
});