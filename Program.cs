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
            LoadProjectFiles lpf = new LoadProjectFiles(args, data);

            lpf.GetAllFiles();
            lpf.PrintOut();
        }
    }
    
}

