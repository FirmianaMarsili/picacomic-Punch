using System;
using System.Collections.Generic;
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
            if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            {
                throw new Exception("请查看文档设置账号密码");
            }
            List<Account> Accounts = new List<Account>();
            foreach (var item in args[0].Split('|'))
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string[] user_pass = item.Split(',');
                    if (user_pass.Length == 2)
                    {
                        Accounts.Add(new Account
                        {
                            username = user_pass[0],
                            password = user_pass[1]
                        });
                    }
                    else
                    {
                        throw new Exception("请查看文档设置账号密码");
                    }
                }
            }
            for (int i = 0; i < Accounts.Count; i++)
            {
                await PunchAsync(Accounts[i].username, Accounts[i].password, i);
            }
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

    }
}
 
