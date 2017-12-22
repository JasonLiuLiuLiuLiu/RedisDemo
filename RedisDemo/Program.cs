
/* ==============================================================================
   * 功能描述：ReidsDemo  
   * 创 建 者：Zouqj
   * 创建日期：2016/4/14/11:40
   * 更多redis相关技术请参考我的博文：http://www.cnblogs.com/jiekzou/p/4487356.html
   ==============================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//添加如下引用
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;

namespace RedisConsoleApplication1
{    //PM> Install-Package ServiceStack.Redis  首先用这个导入
    // 更多redis相关技术请参考我的博文：http://www.cnblogs.com/jiekzou/p/4487356.html
    class Program
    {
        #region static field
        static string host = "192.168.2.154";/*访问host地址*/
        static string password = "2016@Msd.1127_kjy";/*实例id:密码*/
        static readonly RedisClient client = new RedisClient(host, 6379);
        //static readonly RedisClient client = new RedisClient("xxxxx.m.cnsza.kvstore.aliyuncs.com", 6379, "dacb71347ad0409c:xxxx"); //49正式环境
        static IRedisTypedClient<InStoreReceipt> redis = client.As<InStoreReceipt>();

        #endregion

        static void Main(string[] args)
        {
            try
            {
                RedisTestApp();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #region redis Test
        public static void RedisTestApp()
        {
            StringTest(); //字符串测试

            HashTest(); //Hash测试

            ObjectTest(); //实体对象测试

            SingleObjEnqueueTest(); //单个对象队列测试

            ListObjTest(); //对象列表测试

            QueueTest(); //队列和栈测试

            Console.ReadKey();
        }

        #region static method
        /// <summary>
        /// 队列和栈测试
        /// </summary>
        private static void QueueTest()
        {
            Console.WriteLine("*******************队列 先进先出********************");

            client.EnqueueItemOnList("test", "饶成龙");//入队。
            client.EnqueueItemOnList("test", "周文杰");
            long length = client.GetListCount("test");
            for (int i = 0; i < length; i++)
            {
                Console.WriteLine(client.DequeueItemFromList("test"));//出队.
            }
            Console.WriteLine("*********************栈 先进后出*****************");
            client.PushItemToList("name1", "邹琼俊");//入栈
            client.PushItemToList("name1", "周文杰");
            long length1 = client.GetListCount("name1");
            for (int i = 0; i < length1; i++)
            {
                Console.WriteLine(client.PopItemFromList("name1"));//出栈.
            }
            Console.ReadKey();
        }
        /// <summary>
        /// 单个对象队列测试
        /// </summary>
        private static void SingleObjEnqueueTest()
        {
            //Console.WriteLine("******************实体对象队列操作********************");
            //Student _stu = new Student { Name = "张三", Age = 21 };
            //JavaScriptSerializer json = new JavaScriptSerializer();
            //client.EnqueueItemOnList("stu", json.Serialize(_stu));
            //_stu = json.Deserialize<Student>(client.DequeueItemFromList("stu"));
            //Console.WriteLine(string.Format("姓名：{0},年龄{1}", _stu.Name, _stu.Age));
            //Console.ReadKey();
        }

        /// <summary>
        /// List对象测试
        /// </summary>
        public static void ListObjTest()
        {
            List<InStoreReceipt> list = new List<InStoreReceipt>() { new InStoreReceipt() { IdentityID = 1, ReceiptStatus = 1, ReceiptTime = DateTime.Now, ReceiptMessage = "test1" },
            new InStoreReceipt() { IdentityID = 2, ReceiptStatus = 1, ReceiptTime = DateTime.Now, ReceiptMessage = "test2" },new InStoreReceipt() { IdentityID = 3, ReceiptStatus = 1, ReceiptTime = DateTime.Now, ReceiptMessage = "test3" }};
            AddInStoreInfo(list);
            var rList = redis.GetAllItemsFromList(redis.Lists["InStoreReceiptInfoList"]);
            rList.ForEach(v => Console.WriteLine(v.IdentityID + "," + v.ReceiptTime + "," + v.ReceiptMessage));
            redis.RemoveAllFromList(redis.Lists["InStoreReceiptInfoList"]);
            Console.ReadKey();
        }

        /// <summary>
        /// 实体对象测试
        /// </summary>
        private static void ObjectTest()
        {
            Console.WriteLine("**************实体对象，单个，列表操作*****************");
            UserInfo userInfo = new UserInfo() { UserName = "zhangsan", UserPwd = "1111" };//</span>(底层使用json序列化 )  
            client.Set<UserInfo>("userInfo", userInfo);
            UserInfo user = client.Get<UserInfo>("userInfo");
            Console.WriteLine(user.UserName);

            List<UserInfo> list = new List<UserInfo>() { new UserInfo() { UserName = "lisi", UserPwd = "222" }, new UserInfo() { UserName = "wangwu", UserPwd = "123" } };
            client.Set<List<UserInfo>>("list", list);
            List<UserInfo> userInfoList = client.Get<List<UserInfo>>("list");
            userInfoList.ForEach(u => Console.WriteLine(u.UserName));
            client.Remove("list");

            Console.ReadKey();
        }

        /// <summary>
        /// Hash测试
        /// </summary>
        private static void HashTest()
        {
            Console.WriteLine("********************Hash*********************");
            client.SetEntryInHash("userInfoId", "name", "zhangsan");
            var lstKeys = client.GetHashKeys("userInfoId");
            lstKeys.ForEach(k => Console.WriteLine(k));
            var lstValues = client.GetHashValues("userInfoId");
            lstValues.ForEach(v => Console.WriteLine(v));
            client.Remove("userInfoId");
            Console.ReadKey();
        }
        /// <summary>
        /// 字符串测试
        /// </summary>
        private static void StringTest()
        {
            #region 字符串类型
            Console.WriteLine("*******************字符串类型*********************");
            client.Set<string>("name", "zouqj");
            string userName = client.Get<string>("name");
            Console.WriteLine(userName);
            Console.ReadKey();
            #endregion
        }
        /// <summary>
        /// 添加需要回执的进仓单信息到Redis
        /// </summary>
        /// <param name="lstRInStore">进仓单回执信息列表</param>
        private static void AddInStoreInfo(List<InStoreReceipt> inStoreReceipt)
        {
            IRedisList<InStoreReceipt> rlstRInStore = redis.Lists["InStoreReceiptInfoList"];
            rlstRInStore.AddRange(inStoreReceipt);
        }
        #endregion
        #endregion
    }
    /// <summary>
    /// 进仓单回执信息（对应清关系统）
    /// </summary>
    public class InStoreReceipt
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int IdentityID { get; set; }
        /// <summary>
        /// 回执状态
        /// </summary>
        public int ReceiptStatus { get; set; }
        /// <summary>
        /// 回执时间
        /// </summary>
        public DateTime ReceiptTime { get; set; }
        /// <summary>
        /// 回执信息
        /// </summary>
        public string ReceiptMessage { get; set; }
    }
    public class Student
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }
    }
    public class UserInfo
    {
        public string UserName { get; set; }
        public string UserPwd { get; set; }
    }
}