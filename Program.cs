using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Utilities;
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

            Log(posts());
        }

        private static void Log(object o)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}]{o.ToString()}");
        }


        static string posts()
        {
            HttpHelper hh = new HttpHelper();
            HttpItem hi = new HttpItem();
            hi.URL = "https://126.com";
            hi.Allowautoredirect = true;
            string html = hh.GetHtml(hi);
            return html;

        }

    }
    
}
