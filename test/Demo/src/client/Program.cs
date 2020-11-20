using System;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        private static async GetNodeInfo(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var 
            }

        }
    }

    class NodeInfo
    {
        public string HOSTNAME { get; set; }
        public string APP_SERVICE_NAME { get; set; }
    }
}
