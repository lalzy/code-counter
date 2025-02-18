// See https://aka.ms/new-console-template for more information

LoadProjectFiles lpf = new LoadProjectFiles("/home/skn/coding-projects/common-lisp/yggdrasil");
lpf.addFileType("*.lisp");
lpf.addFileType("*.asd");
lpf.AddFolderToIgnore(".git");
lpf.SetCommentCharacters(';');
lpf.SetMultiLineComments("#||", "||#");
lpf.SetMultiLineComments("#|", "|#");
lpf.getAllFiles();

lpf.printOut();