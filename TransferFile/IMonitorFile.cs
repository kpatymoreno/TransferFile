using System.IO;

namespace TransferFile
{
    interface IMonitorFile
    {
        void onActionOccurOnFolderPath(object sender, FileSystemEventArgs e);
        void Run();
    }
}