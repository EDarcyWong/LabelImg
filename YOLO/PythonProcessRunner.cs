using CommunityToolkit.Mvvm.Messaging;
using LabelImg.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;


namespace LabelImg.YOLO
{
    public class PythonProcessRunner : IDisposable
    {
        private Process pythonProcess;

        /// <summary>
        /// 启动指定目录下的 Python 脚本
        /// </summary>
        /// <param name="pythonExePath">python 可执行文件路径，默认"python"（需环境变量中存在）</param>
        /// <param name="scriptPath">Python脚本完整路径</param>
        /// <param name="workingDirectory">工作目录，一般为脚本所在目录</param>
        public void Start()
        {
            WeakReferenceMessenger.Default.Send(new MyMessage("[PY] 开始启动Python服务……"));
            KillProcessByPort(5000);

			string pythonExePath = "python";
            string appRoot = AppDomain.CurrentDomain.BaseDirectory;
            string scriptPath = System.IO.Path.Combine(appRoot, "YOLO\\yolo_server.py");
            Debug.WriteLine(scriptPath);

            if (!File.Exists(scriptPath))
                throw new FileNotFoundException("Python脚本不存在", scriptPath);

            if (pythonProcess != null && !pythonProcess.HasExited)
                throw new InvalidOperationException("Python进程已在运行");

            string workingDirectory = Path.GetDirectoryName(scriptPath);

            var psi = new ProcessStartInfo
            {
                FileName = pythonExePath,
                Arguments = $"\"{scriptPath}\"",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            pythonProcess = new Process { StartInfo = psi };
            pythonProcess.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.WriteLine("[PY] " + e.Data);
                    WeakReferenceMessenger.Default.Send(new MyMessage("[PY] " + e.Data));
                }
            };
            pythonProcess.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.WriteLine("[PY#] " + e.Data);
                    WeakReferenceMessenger.Default.Send(new MyMessage("[PY#] " + e.Data));
                }
            };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();
		}

		public void KillProcessByPort(int port)
        {
            WeakReferenceMessenger.Default.Send(new MyMessage("[PY] 关闭端口："+port));
            string cmd = $@"for /f ""tokens=5"" %a in ('netstat -aon ^| findstr :{port}') do taskkill /F /PID %a";

			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = "/c " + cmd,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();
			process.WaitForExit();
            if (!string.IsNullOrEmpty(output))
            {
                WeakReferenceMessenger.Default.Send(new MyMessage("[Kill] " + output));
            }
            if (!string.IsNullOrEmpty(error))
            {
                WeakReferenceMessenger.Default.Send(new MyMessage("[Kill] ERROR: " + error));
            }
			Debug.WriteLine("Output:\n" + output);
			Debug.WriteLine("Error:\n" + error);
		}

        /// <summary>
        /// 关闭python进程
        /// </summary>
        public void Stop()
        {
            WeakReferenceMessenger.Default.Send(new MyMessage("[PY] 关闭Python服务"));
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                try
                {
                    pythonProcess.Kill();
                    pythonProcess.WaitForExit();
                    WeakReferenceMessenger.Default.Send(new MyMessage("[PY] 已终止进程 (PID: " + pythonProcess.Id + ")"));
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new MyMessage("[PY] 终止进程失败: " + ex.Message));
                }
                finally
                {
                    pythonProcess.Dispose();
                    pythonProcess = null;
                }
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new MyMessage("[PY] 无需终止，进程为空或已退出"));
            }
        }


        public void Dispose()
        {
            Stop();
        }
    }
}
