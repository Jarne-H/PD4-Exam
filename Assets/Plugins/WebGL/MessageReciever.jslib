mergeInto(LibraryManager.library, {
  SetupPostMessageListener: function () {
    window.addEventListener("message", function (event) {
      // Optional: Check event.origin here for security

      if (event.data && event.data.type === "FromParent") {
        var str = event.data.data;

        // Call Unity method
        // Replace 'GameObjectName' and 'MethodName' with actual names
        SendMessage("MazeInitialiser", "GetMazeNameFromWebPage", str);
      }
    });
  }
});