using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace picacomic
{
    struct Account
    {
        public string username;
        public string password;
    }
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Log(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Log(Environment.CurrentDirectory);
            Log(AppDomain.CurrentDomain.BaseDirectory);
            using (StreamWriter sw = new StreamWriter(@"README.md", true))
            {
                sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\r\n");
            }
           // File.WriteAllText(@"README.md", DateTime.Now.ToString("HH:mm:ss")+"\r\n");
            using (StreamReader sr = new StreamReader("README.md"))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        Log(line);
                    }
                }
        }

        private static void Log(object o)
        {
            
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}]{o.ToString()}");

        }


    }
}
