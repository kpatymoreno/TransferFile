using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using WinSCP;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SessionOptions = WinSCP.SessionOptions;

namespace TransferFile
{
    class MonitorFile : IMonitorFile
    {

        private readonly ILogger<MonitorFile> _log;
        private readonly IConfiguration _config;

        public MonitorFile(ILogger<MonitorFile> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public void Run()
        {
            string direccion = _config.GetValue<string>("DirMonitor");
            var fileSystemWatcher = new FileSystemWatcher(direccion)
            {
                Filter = _config.GetValue<string>("Filter"),
                NotifyFilter = NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            fileSystemWatcher.Created += onActionOccurOnFolderPath;
            Console.ReadLine();
            
        }
        public void onActionOccurOnFolderPath(object sender, FileSystemEventArgs e)
        {
            _log.LogInformation("Se agrego nuevo archivo {nombre}", e.Name.ToString());
            //Thread.Sleep(120000);
             enviarArchivo(e.Name.ToString());
            _log.LogInformation("Se realizo envio de archivo {nombre}", e.Name.ToString());
        }

        public void enviarArchivo(string filename)
        {
            string lp = _config.GetValue<string>("DirOrigen");
            lp = lp.Replace(";",@"\");
            try
            {
               
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = _config.GetValue<string>("HostName"),
                    PortNumber = _config.GetValue<int>("Puerto"),
                    UserName = _config.GetValue<string>("Usuario"),
                    Name = _config.GetValue<string>("Name"),
                    Password = _config.GetValue<string>("Password"),
                    SshHostKeyFingerprint = _config.GetValue<string>("SshHostKeyFingerprint")
                };
                 //string LocalPath = @"D:\Backup\test\"+ filename;
                string LocalPath = @_config.GetValue<string>("DirOrigen");
                LocalPath = @LocalPath + filename;
                string RemotePath = _config.GetValue<string>("DirEnvio");
                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.FilePermissions = null; // This is default
                    transferOptions.PreserveTimestamp = false;
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult = session.PutFiles(LocalPath + "*", RemotePath, false, transferOptions);

                    // Throw on any error
                    transferResult.Check();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }


}
