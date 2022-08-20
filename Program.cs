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

        private static async Task PunchAsync(string username,string password,int index)
        {
            Log("=============================================");
            Log($"开始运行第{index + 1}个账号");

            Login login = await PicacomicUrl.Login(username, password);
            Log("登录成功");
            Header.SetAuthorization(login.Authorization);
            Log("开始获取人物信息");
            Profile profile = await PicacomicUrl.Profile();
            Log($"昵称：{profile.User.Name}");
            Log("开始签到");
            Punch punch = await PicacomicUrl.Punch();
            if (punch.PunchSuccess)
            {
                Log("签到完成");
                Profile profile_punch = await PicacomicUrl.Profile();
                Log($"等级：{profile_punch.User.Level}");
                Log($"当前经验：{profile_punch.User.Exp}");
            }
            else
            {
                throw new Exception("签到失败");
            }
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
