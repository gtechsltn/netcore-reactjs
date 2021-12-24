using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;

namespace ASPNETCoreReactJSReduxDeploy
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            string solutionFile = @"G:\Froala\VS2022\ASPNETCoreReactJSRedux\ASPNETCoreReactJSRedux.sln";
            string solutionName = Path.GetFileNameWithoutExtension(solutionFile);
            string solutionFolder = Path.GetDirectoryName(solutionFile);
            Print($"Solution Name: { solutionName }");
            Print($"Solution Folder: { solutionFolder }");
            string projectFile = @"G:\Froala\VS2022\ASPNETCoreReactJSRedux\ASPNETCoreReactJSRedux\ASPNETCoreReactJSRedux.csproj";
            string projectName = Path.GetFileNameWithoutExtension(projectFile);
            string projectFolder = Path.GetDirectoryName(projectFile);
            Print($"Project Name: { projectName }");
            Print($"Project Folder: { projectFolder }");
            GitUtil.Pull(solutionFolder);
            string hostName = projectName.PrettyName();
            Print($"Host Name: { hostName }");
            string siteName = hostName;
            Print($"Site Name: { siteName }");
            string physicalPath = $@"{Environment.ExpandEnvironmentVariables("%SystemDrive%")}\inetpub\wwwroot\{siteName}";
            Print($"Physical Path: { physicalPath }");
            Directory.CreateDirectory(physicalPath);
            IISManagerUtil.StopSite(siteName);
            string publishProfile = $@"{projectFolder}\Properties\PublishProfiles\FolderProfile.pubxml";
            DotNetCoreUtil.Publish(projectFile, publishProfile, physicalPath);
            HostsFileUtil.SaveHosting(hostName);
            string appPool = siteName;
            Print($"Application Pool: { appPool }");
            IISManagerUtil.AddAppPool(appPool, true);
            IISManagerUtil.AddSite(siteName, appPool, physicalPath, true, 443);
            IISManagerUtil.StartSite(siteName);
            IISManagerUtil.RecycleAppPool(appPool);
            IISManagerUtil.OpenSite(siteName, true, 443);
            Print("DONE");
        }

        public static void Print(object obj, string prefix = null)
        {
            if (prefix != null) Debug.Write(obj);
            Debug.WriteLine(obj);

            if (prefix != null) Console.Write(obj);
            Console.WriteLine(obj);
        }
    }

    /// <summary>
    /// IIS Site + Application Pool + ASP.NET + SQL Server
    /// </summary>
    public static class PowerShellUtil
    {
        /// <summary>
        /// IIS and ASP.NET: The Application Pool
        /// https://www.developer.com/microsoft/asp/iis-and-asp-net-the-application-pool/
        /// Easily setup an IIS Site (end to end: Build proj, setup IIS and AppPool, grant SQL permissions)
        /// https://gist.github.com/litodam/3125213
        /// </summary>
        public static void GetIISAppPool()
        {
            //Get-IISAppPool
            //IIS APPPOOL\ASPNETCoreReactJSRedux.vn
            //IIS APPPOOL\DefaultAppPool
        }
    }

    public static class GitUtil
    {
        /*
         *   Remember, a pull is a fetch and a merge.
         *   git pull origin master fetches commits from the master branch of the origin remote (into the local origin/master branch), and then it merges origin/master into the branch you currently have checked out.
         *   git pull only works if the branch you have checked out is tracking an upstream branch.
         *   For example, if the branch you have checked out tracks origin/master, git pull is equivalent to git pull origin master
         */

        public static void Pull(string solutionFolder)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.WorkingDirectory = solutionFolder;
            startInfo.FileName = "CMD";
            startInfo.Arguments = $@"/c git pull origin master";
            Program.Print(startInfo.Arguments, "CMD");
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            Program.Print(output);
            process.WaitForExit();

            //string gitCommand = "git";
            //string gitPullArgument = " pull origin master";
            //ProcessUtil.RunCommand(gitCommand, gitPullArgument, solutionFolder);
        }
    }

    /// <summary>
    /// Get publish settings from IIS and import into Visual Studio
    /// https://docs.microsoft.com/en-us/visualstudio/deployment/tutorial-import-publish-settings-iis?view=vs-2022
    /// ASP.NET Web Deployment + Publish to IIS
    /// https://docs.microsoft.com/en-us/aspnet/web-forms/overview/deployment/visual-studio-web-deployment/deploying-to-iis
    /// Deploy ASP.NET Core to IIS
    /// https://stackify.com/how-to-deploy-asp-net-core-to-iis/
    /// Publishing to IIS
    /// https://jakeydocs.readthedocs.io/en/latest/publishing/iis.html
    /// + AspNetCoreModule
    /// + Download .NET Core Windows Server Hosting
    /// </summary>
    public static class DotNetCoreUtil
    {
        public static void Publish(string projectFile, string publishProfile, string outputFolder)
        {
            string programFiles = IISManagerUtil.ProgramFilesx86().Replace(" (x86)", "");
            string dotNet = $@"{programFiles}\dotnet\dotnet";
            string args = $" publish \"{projectFile}\" -c Release /p:PublishProfile=\"{publishProfile}\" -o \"{outputFolder}\"";
            Program.Print(args, "dotnet");
            ProcessUtil.RunCommand(dotNet, args);
        }
    }

    /// <summary>
    /// The class utility extend IIS Manager
    /// %windir%            C:\WINDOWS\
    /// %SystemDrive%       C:\
    /// %SystemDrive%\inetpub\wwwroot\                                  ~ C:\inetpub\wwwroot\
    /// %windir%\System32\                                              ~ C:\Windows\System32\
    /// %windir%\System32\drivers\etc\                                  ~ C:\Windows\System32\drivers\etc\
    /// C:\inetpub\wwwroot\
    /// C:\Windows\System32\inetsrv\config\administration.config
    /// C:\Windows\System32\inetsrv\appcmd.exe
    /// C:\Windows\System32\drivers\etc\hosts
    /// C:\Program Files (x86)\IIS Express\appcmd.exe
    /// C:\Program Files (x86)\IIS Express\IisExpressAdminCmd.exe
    /// ---------------------------------------------------------------------------------------
    /// https://docs.microsoft.com/en-us/iis/manage/provisioning-and-managing-iis/appcmdexe
    /// https://docs.microsoft.com/en-us/iis/publish/using-webdav/how-to-configure-webdav-settings-using-appcmd
    /// using System.Security;
    /// using Microsoft.Web.Management.Server;
    /// https://docs.microsoft.com/en-us/iis/configuration/system.applicationhost/sites/site/application/
    /// https://docs.microsoft.com/en-us/iis/develop/runtime-extensibility/an-end-to-end-extensibility-example-for-iis-developers
    /// Internet Information Services (IIS) chính là các dịch vụ dành cho máy chủ
    ///  chạy trên nền hệ điều hành Window nhằm cung cấp và phân tán các thông tin lên mạng,
    ///  nó bao gồm nhiều dịch vụ khác nhau như Web Server, FTP Server,...
    /// </summary>
    public static class IISManagerUtil
    {
        public static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                //Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            //Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        public static void AddAppPool(string siteName, bool isNetCore)
        {
            string adminCmd = $@"{Environment.ExpandEnvironmentVariables("%WinDir%")}\system32\inetsrv\APPCMD";
            string appPool = siteName;
            string runtimeVersion = isNetCore ? string.Empty : "v4.0";
            string args = $" add apppool /name:\"{appPool}\" /managedRuntimeVersion:\"{runtimeVersion}\" /managedPipelineMode:\"Integrated\"";
            Program.Print(args, "AppCmd");
            ProcessUtil.RunCommand(adminCmd, args);
        }

        public static void AddSite(string siteName, string appPool, string physicalPath, bool isUseSsl, int port)
        {
            string adminCmd = $@"{Environment.ExpandEnvironmentVariables("%WinDir%")}\system32\inetsrv\APPCMD";
            string programFilesx86 = ProgramFilesx86();

            // Assign Site ID
            int siteId = GetMaxSiteId() + 1;

            // Assign Bindings
            string bindings = $"http/*:{port}:{appPool}";
            if (isUseSsl)
            {
                bindings = $"https/*:{port}:{appPool}";
            }

            // Add Site
            string args = $" add site /name:\"{siteName}\" /id:{siteId} /physicalPath:\"{physicalPath}\" /bindings:{bindings}";
            Program.Print(args, "AppCmd");
            ProcessUtil.RunCommand(adminCmd, args);

            // Edit site binding ssl certificate
            if (isUseSsl)
            {
                string iisExpressAdminCmd = $@"{programFilesx86}\IIS Express\IISExpressAdminCmd";
                args = $" setupsslUrl -url:https://{siteName}:{port} -UseSelfSigned";
                Program.Print(args, "AppCmd");
                ProcessUtil.RunCommand(iisExpressAdminCmd, args);
            }

            // Set full permission to folders during WebDeploy
            SetFullPermission(physicalPath);

            // Set Application Pool to App
            args = $" set app \"{siteName}/\" /applicationPool:\"{appPool}\"";
            Program.Print(args, "AppCmd");
            ProcessUtil.RunCommand(adminCmd, args);
        }

        public static void RecycleAppPool(string appPool)
        {
            string adminCmd = $@"{Environment.ExpandEnvironmentVariables("%WinDir%")}\system32\inetsrv\APPCMD";
            string args = $" recycle apppool \"{appPool}\"";
            Program.Print(args, "AppCmd");
            ProcessUtil.RunCommand(adminCmd, args);
        }

        public static void ResetIIS()
        {
            string iisreset = $@"{Environment.ExpandEnvironmentVariables("%WinDir%")}\System32\iisreset";
            ProcessUtil.RunCommand(iisreset, string.Empty);
        }

        public static void OpenSite(string siteName, bool isUseSsl, int port)
        {
            string programFilesx86 = ProgramFilesx86();
            string msedge = $@"{programFilesx86}\Microsoft\Edge\Application\msedge";
            string swaggerIndex = $@"http://{siteName}:{port}/swagger/index.html";
            if (isUseSsl)
            {
                swaggerIndex = $@"https://{siteName}:{port}/swagger/index.html";
            }
            ProcessUtil.RunCommand(msedge, swaggerIndex);
        }

        public static string PrettyName(this string projectName)
        {
            string hostName = projectName;
            if (!projectName.EndsWith(".vn", StringComparison.OrdinalIgnoreCase))
            {
                hostName = $"{projectName}.vn";
            }
            return hostName;
        }

        private static void SetFullPermission(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

            var groupNames = new[]
            {
                @".\IIS_IUSRS",
            };

            foreach (var groupName in groupNames)
            {
                directorySecurity.AddAccessRule(
                    new FileSystemAccessRule(groupName,
                        FileSystemRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow));
                directoryInfo.SetAccessControl(directorySecurity);
            }
        }

        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = string.Empty;
            var i = 0;
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Program.Print($"IP Address: {ip}");
                    i++; if (i == 2) ipAddress = ip.ToString();
                }
            }
            return ipAddress;
        }

        /// <summary>
        /// Get Max Id of Site in IIS Manager
        /// </summary>
        /// <param name="workingDir"></param>
        /// <returns></returns>
        private static int GetMaxSiteId()
        {
            int maxId = 0;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = " list sites",
                    CreateNoWindow = true,
                    FileName = $@"{Environment.ExpandEnvironmentVariables("%WinDir%")}\system32\inetsrv\APPCMD",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string output = proc.StandardOutput.ReadLine();
                if (!string.IsNullOrWhiteSpace(output))
                {
                    string str = output.Split(new[] { "id:" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    str = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (int.TryParse(str, out int id) && id > maxId)
                    {
                        maxId = id;
                    }
                }
            }
            return maxId;
        }

        public static void StartSite(string siteName)
        {
            string adminCmd = $@"{Environment.ExpandEnvironmentVariables("%WinDir%")}\system32\inetsrv\APPCMD";
            string args = $" start sites \"{ siteName }\"";
            Program.Print(args, "AppCmd");
            ProcessUtil.RunCommand(adminCmd, args);
        }

        public static void StopSite(string siteName)
        {
            string adminCmd = $@"{Environment.ExpandEnvironmentVariables("%WinDir%")}\system32\inetsrv\APPCMD";
            string args = $" stop sites \"{ siteName }\"";
            Program.Print(args, "AppCmd");
            ProcessUtil.RunCommand(adminCmd, args);
        }
    }

    /// <summary>
    /// How to get the output of Process.Start?
    /// https://pretagteam.com/question/how-to-execute-command-prompt-and-get-the-output-from-it
    /// https://pretagteam.com/question/execute-multiple-command-lines-with-the-same-process-using-net
    /// https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
    /// https://docs.microsoft.com/en-us/answers/questions/67390/c-run-a-long-cmd-process-with-live-putput-on-form.html
    /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline
    /// https://docs.microsoft.com/en-us/answers/questions/204058/how-to-execute-this-command-in-cmd-prompt-using-c.html
    /// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.start?view=net-6.0
    /// </summary>
    public static class ProcessUtil
    {
        /// <summary>
        /// Run a specific program from command prompt
        /// Cannot mix synchronous and asynchronous operation on process stream.
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="args"></param>
        public static string RunCommand(string filePath, string args, string workingDirectory = null)
        {
            StringBuilder sb = new StringBuilder();
            args = !string.IsNullOrEmpty(args) ? $" {args}" : string.Empty;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = args,
                    CreateNoWindow = true,
                    FileName = filePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            if (workingDirectory != null)
            {
                proc.StartInfo.WorkingDirectory = workingDirectory;
            }
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                sb.AppendLine(proc.StandardOutput.ReadLine());
            }
            string output = sb.ToString();
            Program.Print(output, Path.GetFileNameWithoutExtension(filePath));
            return output;
        }
    }

    /// <summary>
    /// The class utility work with file C:\Windows\System32\drivers\etc\hosts
    /// </summary>
    public static class HostsFileUtil
    {
        /// <summary>
        /// Create new host name in C:\Windows\System32\drivers\etc\hosts
        /// --------------------------------------------------------------
        /// Open the Command Prompt with Administrative Privileges
        /// notepad C:\Windows\System32\drivers\etc\hosts
        /// 127.0.0.1       mysite.vn
        /// ::1             mysite.vn
        /// </summary>
        /// <param name="hostName"></param>
        public static void SaveHosting(string hostName)
        {
            string[] lines = new[]
            {
                "127.0.0.1       {0}",
                "::1             {0}"
            };
            string hostsFilePath = $@"{Environment.ExpandEnvironmentVariables("%WinDir%")}\System32\drivers\etc\hosts";
            string hostsFileContent = File.ReadAllText(hostsFilePath, Encoding.UTF8);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Environment.NewLine);
            foreach (var line in lines)
            {
                string s = string.Format(line, hostName);
                if (!hostsFileContent.Contains(s))
                {
                    sb.AppendLine(s);
                }
            }
            string strAddress = $"{IISManagerUtil.GetLocalIpAddress()}             {hostName}";
            if (!hostsFileContent.Contains(strAddress))
            {
                sb.AppendLine(strAddress);
            }
            var str = sb.ToString();
            if (!string.IsNullOrWhiteSpace(str))
            {
                FileUtil.AppendText(hostsFilePath, str);
            }
        }
    }

    /// <summary>
    /// The class utility extend of System.IO.File
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Append text to an existing file
        /// </summary>
        /// <param name="filePath"></param>
        public static void AppendText(string filePath, string line)
        {
            using (StreamWriter file = new StreamWriter(filePath, append: true))
            {
                file.WriteLine(line);
            }
        }
    }
}