using System;
using Newtonsoft.Json.Linq;
using picacg;
  
namespace picacomic
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {            
            if (args.Length != 2)
            {
                throw new Exception("请查看文档设置账号密码");
            }
            string username = args[0];
            string password = args[1];
            string msg = await HttpWeb.SendAsync(PicacomicUrl.Login(username, password));
            JObject jd = JObject.Parse(msg);
            if ((int)jd["code"] == 200 && jd["data"] != null)
            {
                Header.SetAuthorization((string)jd["data"]["token"]);
                Log("登录成功");
                Log("开始获取人物信息");
                msg = await HttpWeb.SendAsync(PicacomicUrl.Profile());
                jd = JObject.Parse(msg);
                if ((int)jd["code"] == 200)
                {
                    Log($"昵称：{(string)jd["data"]["user"]["name"]}");
                    Log($"等级：{(string)jd["data"]["user"]["level"]}");
                    Log($"当前经验：{(string)jd["data"]["user"]["exp"]}");
                    if ((bool)jd["data"]["user"]["isPunched"])
                    {
                        Log("已经签到完成");
                    }
                    else
                    {
                        Log("开始签到");
                        msg = await HttpWeb.SendAsync(PicacomicUrl.Punch());
                        jd = JObject.Parse(msg);
                        if ((int)jd["code"] == 200 && jd["data"] != null)
                        {
                            Log("签到完成");
                        }
                    }
                }
            }
            else
            {
                throw new Exception("请查看文档设置账号密码");
            }
        }

        private static void Log(object o)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}]{o.ToString()}");
        }

    }
} 
   
