 var MyPlugin = {
     checkIfMobile: function()
     {
         return UnityLoader.SystemInfo.mobile;
     }
 };
 
 mergeInto(LibraryManager.library, MyPlugin);
 