// See https://aka.ms/new-console-template for more information
// Test File
namespace CodeCounter {
    class Program{
        static void Main(string[] args){
            Config config = new Config();
            Config.Data? data = config.DataObject;
            if(data == null){
                throw new Exception("No config!");
            }
            // Move 'pathdir' to be a folder instead later.
            LoadProjectFiles lpf = new LoadProjectFiles(args);
            foreach(var file in data.FilesToCount){
                lpf.addFileType(file);
            }
            foreach(var folder in data.FoldersToIgnore){
                lpf.AddFolderToIgnore(folder);
            }

            foreach(var comment in data.CommentSymbols){
                lpf.AddCommentCharacters(comment);
            }

            foreach(var MultiLineComments in data.multilineCommentSymbols){
                lpf.SetMultiLineComments(MultiLineComments[0], MultiLineComments[1]);
            }

            lpf.getAllFiles();
            lpf.printOut();
        }
    }
    
}

