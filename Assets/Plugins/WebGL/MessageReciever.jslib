mergeInto(LibraryManager.library, {
  GetMazeNameFromPage: function () {
    var inputValue = parent.document.getElementById("mazeName").value;
    SendMessage("MazeTileGenerator", "RecieveMazeNameFromWebPage", inputValue);

  }
});