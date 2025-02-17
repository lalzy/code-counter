// See https://aka.ms/new-console-template for more information

LoadProjectFiles lpf = new LoadProjectFiles("/home/skn/coding-projects/common-lisp/yggdrasil2");
lpf.addFileType("*.txt");
lpf.addFileType("*.lisp");
lpf.addFileType("*.asd");
lpf.SetCommentCharacters(';');
lpf.SetMultiLineComments("|#", "#|");
lpf.getAllFiles();
Console.WriteLine($"Line count: {lpf.getLines()}");
