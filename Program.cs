using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using picacomic;
using picacomic.Http.Response;

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
            File.WriteAllText(@"../asset/1.txt", "111");
        }

        private static void Log(object o)
        {
            
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}]{o.ToString()}");

        }


    }
}
