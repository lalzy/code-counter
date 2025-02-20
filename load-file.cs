using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;

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
    /// Goes through the file (usually *.extension) of the config object, and then sends it to the AddFileType overload.
    /// </summary>
    /// <param name="configData">Initialized config object that contains the .json config data</param>
    public void AddFileType(Config.Data configData){
        foreach(var file in configData.FilesToCount){
            AddFileType(file);
        }
    }
    /// <summary>
    /// Adds a file type to the list of counted files.
    /// </summary>
    /// <param name="fileType">Name + extension of the file to count as string (*.extension also work for all).</param>
    public void AddFileType(string fileType){
        _FileTypes.Add(fileType);
    }

    /// <summary>
    /// Goes through the folders of the config object, 
    /// and then sends it to the AddFolderToIgnore overload.
    /// </summary>
    /// <param name="configData">Initialized config object that contains the .json config data</param>
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
    /// <param name="configData">Initialized config object that contains the .json config data</param>
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
    /// <param name="configData">Initialized config object that contains the .json config data</param>
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
        // _Comments.Add(start); // Ensure we don't skip if code is on same line
        // _Comments.Add(end);
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
    /// Checks if the line contains any start-multi-line comment symbols. If it does, adds it to the multiLineCharacter's list.
    /// If the entire line is a comment, then increments comment count.
    /// </summary>
    /// <param name="multiLineCharacters">Whether or not we're looking for an 'end' symbol</param>
    /// <param name="line">Current line</param>
    /// <param name="commentCount">Counter of how many comment-lins have been read.</param>
    /// <returns>Tuple of the params that are modified:
    /// multiLineCharacters has the character added
    // comment-count is incremented if there is no code.<string></returns>
    private (List<string>, int) StartMultiLineComment(List<string> multiLineCharacters, string line, int commentCount){
      for(int i = 0; i < _MultiLineCommentsStart.Count ; i++){
            if (line.Contains(_MultiLineCommentsStart[i])){
                multiLineCharacters.Add(_MultiLineCommentsEnd[i]);
            }
        }
        if(line.Length == 0)
            commentCount++;
        return (multiLineCharacters, commentCount);
    }

    /// <summary>
    ///  Checks if the line contains any end-multi-line comment symbols. If it does, remove it from the multiLineCharacter's list.
    /// If the entire line is a comment, then increments comment count.
    /// </summary>
    /// <param name="multiLineCharacters">Whether or not we're looking for an 'end' symbol</param>
    /// <param name="line">Current line</param>
    /// <param name="commentCount">Counter of how many comment-lins have been read.</param>
    /// <returns>Tuple of the params that are modified:
    /// multiLineCharacters has the character removed.
    // Line has the comment-symbol removed, 
    // comment-count is incremented if there is no code.<string></returns>
    private (List<string>, string, int) EndMultiLineComment(List<string> multiLineCharacters, string line, int commentCount){
        if(multiLineCharacters.Count > 0){
            for (int i = 0; i < multiLineCharacters.Count ; i++){
                if (line.Contains(multiLineCharacters[i])){
                    int index = line.IndexOf(multiLineCharacters[i]);
                    line = line.Substring(index > 0 ? index+multiLineCharacters[i].Length : index);
                    multiLineCharacters.Remove(multiLineCharacters[i]);
                }
            }
        }
        if(line.Length == 0){
            commentCount++;
        }
        
        return (multiLineCharacters, line, commentCount);
    }

    /// <summary>
    /// Checks if the line contains, or is an multiLine comment.
    /// We increment the comments counter.
    /// </summary>
    /// <param name="multiLineCharacters">Whether or not we're looking for an 'end' symbol</param>
    /// <param name="line">Current line</param>
    /// <param name="commentCount">Counter of how many comment-lins have been read.</param>
    /// <returns>Tuple of the parameters (modified by End/StartMultiLineComment, see their documentation for modification details<string></returns>
    private (List<string>, string, int) CheckForMultiComment(List<string> multiLineCharacters, string line, int commentCount){
        (multiLineCharacters, line, commentCount) = EndMultiLineComment(multiLineCharacters, line, commentCount);
        (multiLineCharacters, commentCount) = StartMultiLineComment(multiLineCharacters, line, commentCount);

        return (multiLineCharacters, line, commentCount);
    }

    /// <summary>
    /// Remove the string from a string-line. This is to ensure any commented line within the string is ignored.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="commentChar"></param>
    /// <returns>modified line where the string is removed (but the string-char is kept).</returns>
    private (char?, string) skipStringLine (string line, char stringChar, char? storedChar){
        if(line.Contains(stringChar)){
            StringBuilder newLine = new StringBuilder();
            for (int i = 0 ; i < line.Length ; i++){
                if(line[i] == stringChar){
                    // Skip escape character
                    if(i > 0 && line[i - 1] == '\\'){
                        continue;
                    }
                    storedChar = storedChar == null ? storedChar : null;
                }else if(storedChar != null){
                    newLine.Append(line[i]);
                }
            }
            line = newLine.ToString();
        }
        return (storedChar, line);
    }

    /// <summary>
    /// Check if the current-line is a commented out line (entire line is commented), or a code-line (may contain a comment, but also contains code).
    /// If it's a commented-out line, increment the comment count by 1.
    /// </summary>
    /// <param name="line">Current line that's been read up</param>
    /// <param name="commentCount">Counter for content-lines.</param>
    /// <returns>Wether or not to count the line (boolean), and the commentsCount</returns>
    private (bool, int, char?) CountLine(string line, int commentCount, char? storedChar){
        (storedChar, line) = skipStringLine(line, '"', storedChar);
        foreach(string comment in _Comments){
            if(line.IndexOf(comment) == 0){
                return (false, commentCount, storedChar);
            }else if(line.IndexOf(comment) > 0 && storedChar == null){
                return (true, commentCount, storedChar);
            }
        }
        return (true, commentCount, storedChar);
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
    /// Goes through the file counting up the lines.
    /// </summary>
    /// <param name="sr">The file to be read</param>
    /// <returns>Array of lines counted.</returns>
    private int[] ReadLines(StreamReader sr, int[] allLines){
        List<string> multiLineCharacters = new List<string>();
        char? storedChar = null;
        string? line = sr.ReadLine();
        int count = 1;
        while (line != null){
            line = line.Trim(' ');
            // Checks if we're within, or starting a multiline comment
            (multiLineCharacters, line, allLines[COMMENTED]) = CheckForMultiComment(multiLineCharacters, line, allLines[COMMENTED]);

            // Check if the current line should be counted as code (such as if multi-line comment is on same line as valid-code).
            bool countLine;
            (countLine, allLines[COMMENTED], storedChar) = CountLine(line, allLines[COMMENTED], storedChar);

            // If it's an multi-line comment, we skip this (as it's commented).
            if(multiLineCharacters.Count == 0){
                if(line.Length == 0){
                    allLines[WHITESPACE]++;
                }else if(countLine){
                    if(count == 8)
                    Console.WriteLine(line);
                    allLines[CODELINE]++;
                }else{
                    allLines[COMMENTED]++;
                }
            }
            count++;
            line = sr.ReadLine();
        }

        return allLines;
    }
    
    /// <summary>
    /// Goes through and open the files for reading, then pass it to our ReadLine function.
    /// </summary>
    /// <returns>Array of lines counted.</returns>
    public int[] GetLines(){
        int[] allLines = new int[5];
        foreach (FileInfo file in _Files){
            try{
                using (StreamReader sr = new StreamReader(file.FullName)) {
                    allLines = ReadLines(sr, allLines);
                }
            }catch (IOException e) {
                Console.WriteLine($"File error: {e.Message}");
            }
            catch (UnauthorizedAccessException e) {
                Console.WriteLine($"Access error: {e.Message}");
            }
        }
        return allLines;
    }
}