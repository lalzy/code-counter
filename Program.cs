// See https://aka.ms/new-console-template for more information
// Test File
namespace CodeCounter {
    class Program{
        static void Main(string[] args){
            Config config = new Config();
            foreach(var arg in args){
                // Move 'pathdir' to be a folder instead later.
                LoadProjectFiles lpf = new LoadProjectFiles(arg);
                foreach(var file in config.DataObject.FilesToCount){
                    lpf.addFileType(file);
                }
                foreach(var folder in config.DataObject.FoldersToIgnore){
                    lpf.AddFolderToIgnore(folder);
                }

                foreach(var comment in config.DataObject.CommentSymbols){
                    lpf.AddCommentCharacters(comment);
                }

                foreach(var MultiLineComments in config.DataObject.multilineCommentSymbols){
                    lpf.SetMultiLineComments(MultiLineComments[0], MultiLineComments[1]);
                }
                // lpf.addFileType("*.txt");

                // // lpf.addFileType("*.lisp");
                // // lpf.addFileType("*.asd");
                // lpf.AddFolderToIgnore(".git");
                // lpf.SetCommentCharacters(';');
                // lpf.SetMultiLineComments("#||", "||#"); //Emacs liker å lage dobbel pipe
                // lpf.SetMultiLineComments("#|", "|#");
                lpf.getAllFiles();

                lpf.printOut();
            }
        }
    }
}

