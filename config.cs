using System.Text.Json;

public class Config {

    public Data? DataObject {get;set;}
    public class Data
    {
        public List<string> FilesToCount {get;set;} = new List<string>();
        public List<string> FoldersToIgnore {get;set;} = new List<string>();
        public List<string> CommentSymbols {get;set;} = new List<string>();
        public List<List<string>> multilineCommentSymbols {get;set;} = new List<List<string>>();
    };
    public Config(){
        // Console.WriteLine(File.ReadAllText("config.json"));
        string jsonString = File.ReadAllText("config.json");

        DataObject = JsonSerializer.Deserialize<Data>(jsonString);
        if(DataObject == null){
            throw new Exception("Failed to do json.");
        }
    }
}