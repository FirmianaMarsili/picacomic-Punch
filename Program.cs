using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using picacomic.Http.Response;

namespace picacomic
{
    
    struct Account
    {
        public string Username;
        public string Password;

        public Account(string username, string password)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
    }
    static class Program
    {
        static async Task Main(string[] args)
        {
            var accountsString = args[0];
            
            
            if (args.Length == 0 || string.IsNullOrEmpty(accountsString))
            {
                throw new Exception("请查看文档设置账号密码");
            }
            
            var rawSplit = accountsString.Split('|');

            if (rawSplit.Length % 2 != 0)
            {
                throw new Exception("账号密码不匹配");
            }

            var accounts = rawSplit.Batch(2)
                .Select(b => b.ToArray())
                .Select(b => new Account(b[0], b[1]))
                .ToList();
            
            for (int i = 0; i < accounts.Count; i++)
            {
                await PunchAsync(accounts[i].Username, accounts[i].Password, i);
            }
        }

        private static void Log(object o)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{o}");
        }

        private static async Task PunchAsync(string username, string password, int index)
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
                Profile profilePunch = await PicacomicUrl.Profile();
                Log($"等级：{profilePunch.User.Level}");
                Log($"当前经验：{profilePunch.User.Exp}");
            }
            else
            {
                throw new Exception("签到失败");
            }
        }
        
        //credit to https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/Batch.cs
        private static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
            this IEnumerable<TSource> source, int size)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count).ToArray();
        }
    }
}

