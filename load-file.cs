using System.Reflection.PortableExecutable;

class LoadProjectFiles{
    private string[] _ProjectPaths;
    private List<string> _FileTypes = new List<string>();
    private List<string> _FilterFolders = new List<string>();
    private List<FileInfo> _Files = new List<FileInfo>();
    private List<string> _Comments = new List<string>();
    private List<string> _MultiLineCommentsStart = new List<string>();
    private List<string> _MultiLineCommentsEnd = new List<string>();
    
    /// <summary>
    /// Initialize the configData, and gets the path that'll be used to go through the files.
    /// </summary>
    /// <param name="projectPaths">Either a file, or directory that'll be used for counting lines</param>
    /// <param name="configData">Initialized config object that contains the .json config data</param>
    public LoadProjectFiles(string[] projectPaths, Config.Data configData){
        _ProjectPaths = projectPaths;
        AddFileType(configData);
        AddFolderToIgnore(configData);
        AddCommentCharacters(configData);
        SetMultiLineComments(configData);
    }
    
    /// <summary>
    /// Goes through the file (usually *.extention) of the config object, and then sends it to the AddFileType overload.
    /// </summary>
    /// <param name="configData">Initialized config object that contains the .json config data</param>
    public void AddFileType(Config.Data configData){
        foreach(var file in configData.FilesToCount){
            AddFileType(file);
        }
    }
    /// <summary>
    /// Adds the file to list of files that'll be counted.
    /// </summary>
    /// <param name="fileType">Name + extention of the file to count as string (*.extention also work for all).</param>
    public void AddFileType(string fileType){
        _FileTypes.Add(fileType);
    }

    /// <summary>
    /// Goes through the folders of the config object, 
    /// and then sends it to the AddFolderToIgnore overload.
    /// </summary>
    /// <param name="configData"></param>
    public void AddFolderToIgnore (Config.Data configData){
        foreach (var folder in configData.FoldersToIgnore){
            AddFolderToIgnore(folder);
        }
    }


    /// <summary>
    /// Adds the chosen folder name (string) to the filterFolders list.
    /// </summary>
    /// <param name="folderName">Name as string to the folder to filter</param>
    public void AddFolderToIgnore(string folderName){
        _FilterFolders.Add(folderName);
    }


    /// <summary>
    /// Goes through the commentSymbols of the config object, 
    /// and then sends it to the addCommentCharacters list overload.
    /// </summary>
    /// <param name="configData"></param>
    public void AddCommentCharacters (Config.Data configData){
        foreach(var comment in configData.CommentSymbols){
            AddCommentCharacters(comment);
        }   
    }

    /// <summary>
    /// Adds the comment character to the list of comment symbols
    /// </summary>
    /// <param name="character">the symbol to be counted as 'comment' line</param>
    public void AddCommentCharacters (char character){
        _Comments.Add(character.ToString());
    }


    /// <summary>
    /// Adds the comment character(s) to the list of comment symbols
    /// </summary>
    /// <param name="character">the symbol to be counted as 'comment' line</param>
    public void AddCommentCharacters (string characters){
        _Comments.Add(characters);
    }

    /// <summary>
    /// Goes through the multilineCommentPair of the config object, 
    /// and then filter and sends it to the SetMultiLineComments overload.
    /// </summary>
    /// <param name="configData"></param>
    public void SetMultiLineComments (Config.Data configData){
        foreach(var mCommentSymbolPair in configData.multilineCommentSymbols){
            SetMultiLineComments(mCommentSymbolPair[0], mCommentSymbolPair[1]);
        }
    }


    /// <summary>
    /// Add the start-character to the start list, and end to the end-list, and also adds both to the 'comment's list.
    /// so that we don't unintentionally skip lines with code on it.
    /// </summary>
    /// <param name="start">Symbol that designate the start of an multi-line comment.</param>
    /// <param name="end">Symbol that designate the end of an multi-line comment.</param>
    public void SetMultiLineComments (string start, string end){
        _MultiLineCommentsStart.Add(start);
        _MultiLineCommentsEnd.Add(end);
        _Comments.Add(start); // Ensure we don't skip if code is on same line
        _Comments.Add(end);
    }

    /// <summary>
    /// Check if the current-line is a commented out line, or a code-line.
    /// if it's a commented out line, increment comments by 1.
    /// </summary>
    /// <param name="line">Current line that's been read up</param>
    /// <param name="comments">Counter for content-lines.</param>
    /// <returns></returns>
    private (bool, int) CountLine(string line, int comments){
        foreach(string comment in _Comments){
            if(line.IndexOf(comment) == 0){
                return (false, comments);
            }else if(line.IndexOf(comment) > 0){
                return (true, comments + 1);
            }
        }
        return (true, comments);
    }

    /// <summary>
    /// Iterate over the folder and place all the valid-files into our files list.
    /// </summary>
    /// <param name="folder">Folder to look through for files</param>
    public void AddFiles (DirectoryInfo folder){
        foreach(string fileType in _FileTypes){
            foreach (FileInfo file in folder.GetFiles(fileType)){
                _Files.Add(file);
            }
        }
    }


    /// <summary>
    /// Called to get all files set-up based on the json config.
    /// </summary>
    public void GetAllFiles(){
        foreach(string ProjectPath in _ProjectPaths){
            DirectoryInfo projectDirectory = new DirectoryInfo(ProjectPath);
            if(projectDirectory.Exists){
                AddFiles(projectDirectory);
                DirectoryInfo[] folders = projectDirectory.GetDirectories("*", SearchOption.AllDirectories
                );

                foreach(var folder in folders){
                    foreach(var filter in _FilterFolders){
                        string lowerFolderName = folder.FullName.ToLower();
                        string lowerFilterName = filter.ToLower();
                        
                        if(!lowerFolderName.Contains(lowerFilterName)){
                            AddFiles(folder);
                        }
                    }
                }
            }else{
                if(File.Exists(ProjectPath)){
                    _Files.Add(new FileInfo(ProjectPath));
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Checks if the line has a stat, or end multi-comment symbol, then we check if there's code-lines left, if not.
    /// We increment the comments counter.
    /// </summary>
    /// <param name="multilineComment">Wether or not we're looking for an 'end' symbol</param>
    /// <param name="line">Current line</param>
    /// <param name="comments">Counter of how many comment-lins have been read.</param>
    /// <returns></returns>
    private (bool, string, int) CheckForMultiComment(bool multilineComment, string line, int comments){
        if(multilineComment){
            foreach(string commentCharacters in _MultiLineCommentsEnd){
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
            foreach(string commentCharacters in _MultiLineCommentsStart){
                if(line.IndexOf(commentCharacters) >= 0){
                    return (true,line.Replace(commentCharacters, ""), ++comments);
                }
            }
            return (false, line, comments);
        }
    }

    /// <summary>
    ///  Constants for easy printing of the array.
    /// </summary>
    private  const int CODELINE = 0;
    private  const int WHITESPACE = 1;
    private  const int COMMENTED = 2;
    private  const int TOTALLINES = 3;

    /// <summary>
    /// Prints out the counted lines.
    /// </summary>
    public void PrintOut(){
        int[] lines = GetLines();
        Console.WriteLine($"WhiteSpaces:{lines[WHITESPACE]}\n" + 
                          $"Commented Lines:{lines[COMMENTED]}\n"+
                          $"Code Lines: {lines[CODELINE]}\n"+
                          "------------------------\n"+
                          $"Total Lines:{lines[TOTALLINES]}");
    }

    /// <summary>
    /// Goes through all the files, and read their individual lines, and check if they should be counted or not.
    /// </summary>
    /// <returns>Array of lines countet.</returns>
    public int[] GetLines(){
        int codeLines = 0;
        int emptyLineCount = 0;
        int comments = 0;
        bool multilineComment = false;
        foreach (FileInfo file in _Files){
            String? line = "";
            try{
                using (StreamReader sr = new StreamReader(file.FullName)) {
                    line = sr.ReadLine();
                    while (line != null){
                        line = line.Trim(' ');
                        (multilineComment, line, comments) = CheckForMultiComment(multilineComment, line, comments);
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
                }
            }catch (IOException e) {
                Console.WriteLine($"File error: {e.Message}");
            }
            catch (UnauthorizedAccessException e) {
                Console.WriteLine($"Access error: {e.Message}");
            }
        }
        int[] AllLines = [codeLines, emptyLineCount, comments, (codeLines + emptyLineCount + comments)];
        return AllLines;
    }
}