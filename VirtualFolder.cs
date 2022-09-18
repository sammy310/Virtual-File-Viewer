using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VirtualFileViewer
{
    class VirtualFolder
    {
        private const char SubFolderParam1 = '├';
        private const char SubFolderParam2 = '└';
        private const char SubFolderParam3 = '─';
        private const char SubFileParam1 = '│';
        private const char SubFileParam2 = ' ';
        private const int SubFileParam2Count = 4;

        private const string SubFileFrontParam1 = "│  ";
        private const string SubFileFrontParam2 = "    ";

        public string FileName;
        public List<VirtualFolder> SubFolders = new List<VirtualFolder>();
        public int SubFolderCount => SubFolders.Count;

        public List<string> Files = new List<string>();
        public int FileCount => Files.Count;

        public VirtualFolder(string fileName)
        {
            FileName = fileName;
        }

        public void AddSubFolder(int subCount, string fileName)
        {
            if (subCount == 0)
            {
                SubFolders.Add(new VirtualFolder(fileName));
                return;
            }
            SubFolders[SubFolders.Count - 1].AddSubFolder(subCount - 1, fileName);
        }

        public void AddFile(int subCount, string fileName)
        {
            if (subCount == 0)
            {
                Files.Add(fileName);
                return;
            }
            SubFolders[SubFolders.Count - 1].AddFile(subCount - 1, fileName);
        }

        public void AddListViewItem(ListView listView)
        {
            listView.Items.Add(FileName);
            foreach (var file in Files)
            {
                listView.Items.Add(file);
            }
            foreach (var sub in SubFolders)
            {
                sub.AddListViewItem(listView);
            }
        }

        public TreeNode CreateTreeNode()
        {
            TreeNode node = new TreeNode(FileName);
            foreach (var sub in SubFolders)
            {
                node.Nodes.Add(sub.CreateTreeNode());
            }
            foreach (var file in Files)
            {
                node.Nodes.Add(file);
            }
            return node;
        }

        public static VirtualFolder CreateVirtualFile(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        VirtualFolder rootFile = new VirtualFolder(reader.ReadLine());
                        while (reader.EndOfStream == false)
                        {
                            UpdateVirtualFile(rootFile, reader.ReadLine());
                        }
                        return rootFile;
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        private static void UpdateVirtualFile(VirtualFolder rootFile, string fileData)
        {
            int fileDataStrLength = fileData.Length;

            int subCount = 0;
            int index = 0;
            bool isFind = false;
            bool isSubFolder = false;
            for (index = 0; index < fileDataStrLength;)
            {
                switch (fileData[index])
                {
                    case SubFolderParam1: // "├"
                    case SubFolderParam2: // "└"
                        // "├─", "└─"
                        if (fileData[index + 1] != SubFolderParam3)
                        {
                            isFind = true;
                            break;
                        }

                        subCount++;
                        index += 2;
                        isSubFolder = true;
                        break;
                    case SubFileParam1: // "│"
                        // "│  "
                        if (IsStringEqualParam(fileData, SubFileParam2, index + 1, 2) == false)
                        {
                            isFind = true;
                            break;
                        }

                        subCount++;
                        index += 3;
                        isSubFolder = false;
                        break;
                    case SubFileParam2: // " "
                        // "    "
                        if (IsStringEqualParam(fileData, SubFileParam2, index, SubFileParam2Count) == true)
                        {
                            subCount++;
                            index += SubFileParam2Count;
                            isSubFolder = false;
                            break;
                        }
                        break;
                    default:
                        isFind = true;
                        break;
                }

                if (isFind == true)
                {
                    break;
                }
            }

            subCount--;

            // Empty
            if (index >= fileDataStrLength)
            {
                return;
            }

            string fileName = fileData.Substring(index);
            if (isSubFolder == true)
            {
                rootFile.AddSubFolder(subCount, fileName);
            }
            else
            {
                rootFile.AddFile(subCount, fileName);
            }
        }

        public static void CreateTextFile(string filePath, string savePath)
        {
            try
            {
                if (Directory.Exists(filePath) == true)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(filePath);

                    using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        using (var writer = new StreamWriter(fileStream))
                        {
                            writer.WriteLine(directoryInfo.Name);
                            CreateTextFileLoop(writer, directoryInfo, "");
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private static void CreateTextFileLoop(StreamWriter writer, DirectoryInfo directoryInfo, string prefixString)
        {
            var dirs = directoryInfo.GetDirectories();

            // Files
            var files = directoryInfo.GetFiles();
            string filePrefix = prefixString;
            if (dirs.Length == 0)
            {
                filePrefix += SubFileFrontParam2;
            }
            else
            {
                filePrefix += SubFileFrontParam1;
            }

            for (int i = 0; i < files.Length; i++)
            {
                writer.Write(filePrefix);
                writer.WriteLine(files[i].Name);
            }

            writer.WriteLine(filePrefix);

            // Directories
            string dirPrefix = prefixString;
            dirPrefix += SubFolderParam1;
            dirPrefix += SubFolderParam3;

            int dirIndex = 0;
            for (; dirIndex < dirs.Length - 1; dirIndex++)
            {
                writer.Write(dirPrefix);
                writer.WriteLine(dirs[dirIndex].Name);
                CreateTextFileLoop(writer, dirs[dirIndex], filePrefix);
            }

            // Last directory
            if (dirs.Length > 0)
            {
                writer.Write(prefixString);
                writer.Write(SubFolderParam2);
                writer.Write(SubFolderParam3);
                writer.WriteLine(dirs[dirIndex].Name);
                CreateTextFileLoop(writer, dirs[dirIndex], prefixString + SubFileFrontParam2);
            }
        }

        private static bool IsStringEqualParam(string str, char param, int startIndex, int length)
        {
            for (int i = startIndex; i < startIndex + length; i++)
            {
                if (str[i] != param)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
