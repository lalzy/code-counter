class LoadProjectFiles{
    public string ProjectPath;
    List<string> FileTypes;
    private List<FileInfo> Files = new List<FileInfo>();
    private List<string> Comments = new List<string>();
    private List<string> MultiLineCommentsStart = new List<string>();
    private List<string> MultiLineCommentsEnd = new List<string>();
    
    
    public LoadProjectFiles(string projectPath){
        ProjectPath = projectPath;
        FileTypes = new List<string>();
        
    }
    public void addFileType(string fileType){
        FileTypes.Add(fileType);
    }

    public void SetCommentCharacters (char character){
        Comments.Add(character.ToString());
    }

    public void SetCommentCharacters (string characters){
        Comments.Add(characters);
    }

    public void SetMultiLineComments (string start, string end){
        MultiLineCommentsStart.Add(start);
        MultiLineCommentsEnd.Add(end);
        Comments.Add(start); // Ensure we don't skip if code is on same line
        Comments.Add(end);
    }

    private bool CountLine(string line){
        foreach(string comment in Comments){
            if(line.IndexOf(comment) == 0){
                return false;
            }
        }

        return true;
    }

    public void getAllFiles(){
        DirectoryInfo projectDirectory = new DirectoryInfo(ProjectPath);
        foreach(string fileType in FileTypes){
            FileInfo[] files = projectDirectory.GetFiles(fileType);
            if(files.Length > 0){
                foreach(FileInfo file in files){
                    Files.Add(file);
                }
            } 
        }
    }
    private bool checkForMultiComment(bool multilineComment, string line){
        if(multilineComment){
            foreach(string commentCharacters in MultiLineCommentsEnd){
                if(line.IndexOf(commentCharacters) > 0){
                    return false;
                }
            }
            return true;
        }else{
            foreach(string commentCharacters in MultiLineCommentsEnd){
                if(line.IndexOf(commentCharacters) > 0){
                    return true;
                }
            }
            return false;
        }
    }
    public int getLines(){
        int lines = 0;
        int emptyLineCount = 0; // continue caused my visualCode to freeze.
        bool multilineComment = false;
        foreach (FileInfo file in Files){
            String? line = "";
            try{
                StreamReader sr = new StreamReader(file.ToString());
                line = sr.ReadLine();
                while (line != null){
                    line = line.Trim(' ');
                    if(line.Length == 0){
                        emptyLineCount++;
                    }
                    //multilineComment = checkForMultiComment(multilineComment, line);
                    if(!multilineComment){ 
                        if(CountLine(line)){
                            lines++;
                        }
                    }
                    line = sr.ReadLine();
                }
                sr.Close();

            }catch(Exception e){Console.WriteLine(e);}
        }
        return lines - emptyLineCount;
    }
}