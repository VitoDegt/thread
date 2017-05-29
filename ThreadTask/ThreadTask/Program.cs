using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadTask
{
    public class StringProvider
    {
        public string Data
        {
            get;
            private set;
        }

        public StringProvider() {

            Task.Run(() => {

                Random r = new Random();
                while (true) {

                    Data = new string(Enumerable.Range(0, r.Next(4, 7)).Select(f => (char)r.Next((int)'A', (int)'Z')).ToArray());
                    Thread.Sleep(r.Next(20, 150));
                }
            });
        }

    }

    class StringProviderObserver
    {
        private StringProvider provider;

        public event Action<string> DataChanged;

        public StringProviderObserver(StringProvider provider) {

            this.provider = provider;
        }

        public void Start() {

            Thread t = new Thread(() => {
                string data = provider.Data;
                while (true) {
                    if (data != provider.Data) {
                        data = provider.Data;
                        DataChanged?.Invoke(data);
                    }
                }
            });
            t.Start();
        }
    }

    class Program
    {
        static void Main(string[] args) {

            StringProvider provider = new StringProvider();
            StringProviderObserver observer = new StringProviderObserver(provider);
            observer.DataChanged += OnDataChanged;
            observer.Start();

            while (true) {
                Console.WriteLine("\t\tHello World");
                Thread.Sleep(1000);
            }
        }

        private static void OnDataChanged(string obj) {

            Console.WriteLine(obj);
            WriteFile(obj + ".txt");
        }

        private static readonly object syncObject = new object();

        private static void WriteFile(string fileName) {

            ThreadPool.QueueUserWorkItem(o => {
                lock (syncObject){ 
                    using (StreamWriter writer = new StreamWriter(fileName)) {
                        for (int i = 0; i < 100000000; i++)
                            writer.Write("0");
                    }
                }
            });
        }
    }
}
