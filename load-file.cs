using System.Reflection.PortableExecutable;

class LoadProjectFiles{
    public string ProjectPath;
    List<string> FileTypes;
    List<string> FilterFolders = new List<string>();
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

    public void AddFolderToIgnore(string folderName){
        FilterFolders.Add(folderName);
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

    private (bool, int) CountLine(string line, int comments){
        foreach(string comment in Comments){
            if(line.IndexOf(comment) == 0){
                return (false, comments);
            }else if(line.IndexOf(comment) > 0){
                return (true, comments + 1);
            }
        }
        return (true, comments);
    }

    public void AddFiles (DirectoryInfo folder){
        foreach(string fileType in FileTypes){
            FileInfo[] files = folder.GetFiles(fileType);
            if(files.Length > 0){
                foreach(FileInfo file in files){
                    Files.Add(file);
                }
            }
        }
    }



    public void getAllFiles(){
        DirectoryInfo projectDirectory = new DirectoryInfo(ProjectPath);
        AddFiles(projectDirectory);
        DirectoryInfo[] folders = projectDirectory.GetDirectories("*", SearchOption.AllDirectories
        );

        foreach(var folder in folders){
            foreach(var filter in FilterFolders){
                if(!folder.FullName.ToLower().Contains(filter.ToLower())){
                    AddFiles(folder);
                }
            }
        }
    }

    private (bool, string, int) checkForMultiComment(bool multilineComment, string line, int comments){
        if(multilineComment){
            foreach(string commentCharacters in MultiLineCommentsEnd){
                if(line.IndexOf(commentCharacters) == 0){
                    string subString = line.Replace(commentCharacters, "");
                    return (false,subString, comments);
                }else{
                    string subString = line.Substring(line.IndexOf(commentCharacters));
                    return (false,subString, comments);
                }
            }
            return (true, line, ++comments);
        }else{
            foreach(string commentCharacters in MultiLineCommentsStart){
                if(line.IndexOf(commentCharacters) >= 0){
                    return (true,line.Replace(commentCharacters, ""), ++comments);
                }
            }
            return (false, line, comments);
        }
    }

    private  const int CODELINE = 0;
    private  const int WHITESPACE = 1;
    private  const int COMMENTED = 2;
    private  const int TOTALLINES = 3;

    public void printOut(){
        int[] lines = getLines();
        Console.WriteLine($"WhiteSpaces:{lines[WHITESPACE]}\n" + 
        $"Commented Lines:{lines[COMMENTED]}\nCode Lines: {lines[CODELINE]}\n"+
        "------------------------\n"+
        $"Total Lines:{lines[TOTALLINES]}");
    }

    public int[] getLines(){
        int codeLines = 0;
        int emptyLineCount = 0;
        int comments = 0;
        bool multilineComment = false;
        foreach (FileInfo file in Files){
            String? line = "";
            try{
                StreamReader sr = new StreamReader(file.ToString());
                line = sr.ReadLine();
                while (line != null){
                    line = line.Trim(' ');
                    (multilineComment, line, comments) = checkForMultiComment(multilineComment, line, comments);
                    bool countLine;
                    (countLine, comments) = CountLine(line, comments);
                    if(!multilineComment){
                        if(line.Length == 0){
                            emptyLineCount++;
                        }else if(countLine){
                            codeLines++;
                        }else{
                            comments++;
                        }
                    }
                    line = sr.ReadLine();
                }
                sr.Close();

            }catch(Exception e){Console.WriteLine(e);}
        }
        int[] AllLines = [codeLines, emptyLineCount, comments, (codeLines + emptyLineCount + comments)];
        return AllLines;
    }
}