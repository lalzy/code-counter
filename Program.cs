// See https://aka.ms/new-console-template for more information
// Test File
namespace CodeCounter {
    class Program{
        static void Main(string[] args){
            foreach(var arg in args){
                LoadProjectFiles lpf = new LoadProjectFiles(arg);
                lpf.addFileType("*.txt");
                // lpf.addFileType("*.lisp");
                // lpf.addFileType("*.asd");
                lpf.AddFolderToIgnore(".git");
                lpf.SetCommentCharacters(';');
                lpf.SetMultiLineComments("#||", "||#"); //Emacs liker å lage dobbel pipe
                lpf.SetMultiLineComments("#|", "|#");
                lpf.getAllFiles();

                lpf.printOut();
            }
        }
    }
}

