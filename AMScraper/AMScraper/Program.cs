using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace AMScraper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool isProcessFound = false;
            string processPath;
            List<String> PlayButtonName = new List<string>();
            List<String> PauseButtonName = new List<string>();

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr LoadLibraryEx(
                string lpFileName,
                IntPtr hFile,
                uint dwFlags
            );

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            static extern int LoadString(
                IntPtr hInstance,
                int uID,
                StringBuilder lpBuffer,
                int cchBufferMax
            );

            const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

            while (!isProcessFound)
            {
                Process[] appleMusicProcess = Process.GetProcessesByName("AppleMusic");
                if (appleMusicProcess.Length > 0)
                {
                    isProcessFound = true;
                    processPath = appleMusicProcess[0].MainModule.FileName
                        .Split(new string[] {"AppleMusic.exe"}, StringSplitOptions.None)[0];
                    try
                    {
                       IEnumerable<string> directories = Directory.EnumerateDirectories(processPath, "*");
                       foreach (var directory in directories)
                       {
                           IEnumerable<string> muiFiles =
                               Directory.EnumerateFiles(directory, "Microsoft.ui.xaml.dll.mui");
                           foreach (var file in muiFiles)
                           {
                               IntPtr hModule = LoadLibraryEx(file, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                               StringBuilder playSB = new StringBuilder(1024);
                               LoadString(hModule, 5252, playSB, playSB.Capacity);
                               StringBuilder pauseSb = new StringBuilder(1024);
                               LoadString(hModule, 5253, pauseSb, pauseSb.Capacity);
                               PlayButtonName.Add(playSB.ToString());
                               PauseButtonName.Add(pauseSb.ToString());
                           }
                        }
                       
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("Report this error to developer. This shouldn't be happen in first place. Are you sure legitimate process is running?");
                    }
                }
                else
                {
                    Console.WriteLine("Apple Music Process Not Found.");
                }
                Thread.Sleep(250);
            }
            PlayPauseButtons Buttons = new PlayPauseButtons(PlayButtonName, PauseButtonName);
            FileStream createStream = File.Create("ButtonValues.json");
            await JsonSerializer.SerializeAsync(createStream, Buttons);
        }

        public class PlayPauseButtons
        {
           public List<String> playButtons { get; set; }
           public List<String> pauseButtons { get; set; }

           public PlayPauseButtons(List<String> PlayButtonName, List<String> PauseButtonName)
           {
               playButtons = PlayButtonName;
               pauseButtons = PauseButtonName;
           }
        }
    }
}
