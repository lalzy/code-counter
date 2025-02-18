// See https://aka.ms/new-console-template for more information

LoadProjectFiles lpf = new LoadProjectFiles("/home/skn/coding-projects/common-lisp/yggdrasil2");
// lpf.addFileType("*.lisp");
// lpf.addFileType("*.asd");
lpf.addFileType("*.tat");
lpf.AddFolderToIgnore(".git");
lpf.SetCommentCharacters(';');
lpf.SetMultiLineComments("#||", "||#");
lpf.SetMultiLineComments("#|", "|#");
lpf.getAllFiles();
// Console.WriteLine($"Line count: {lpf.getLines()}");
lpf.printOut();