using Newtonsoft.Json;
using ParallelWsTester.SoapFruitLoops;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ParallelWsTester
{
    class Program
    {
        private static Dictionary<int, string> addresses = new Dictionary<int, string>
        {
            {1, "http://localhost/FruitLoops.asmx" },
            {2, "http://10.0.0.10/FruitLoops.asmx" },
            {3, "http://10.4.4.5/FruitLoops.asmx" },
            {4, "http://10.9.100.200/FruitLoops.asmx" },
        };

        static void Main(string[] args)
        {
            string htmlBase64 = longHtml2;

            AllInParallel(htmlBase64);
            //PickOneServer(htmlBase64);

            Console.WriteLine("\nPress any key to exit...");
            Console.Read();
        }

        private static void AllInParallel(string htmlBase64)
        {
            Task.Run(async () =>
            {
                var taskList = new List<Task>();

                foreach (var addr in addresses)
                {
                    Task t = new Task(() =>
                    {
                        ConvertHtmlToPdf(htmlBase64, addr.Value);
                    });

                    taskList.Add(t);
                    t.Start();
                }

                await Task.WhenAll(taskList);

            }).GetAwaiter().GetResult();
        }

        private static void PickOneServer(string htmlBase64)
        {
            Console.WriteLine($"Which server: {JsonConvert.SerializeObject(addresses, Formatting.Indented)}");
            string wsAddress = null;
            string input = Console.ReadLine();

            bool isOk = addresses.TryGetValue(Convert.ToInt32(input), out wsAddress);
            if (!isOk) { Console.WriteLine($"Bad input = '{input}'"); Console.Read(); return; }

            ConvertHtmlToPdf(htmlBase64, wsAddress);
        }

        private static void ConvertHtmlToPdf(string html, string addr)
        {
            using (var client = new FruitLoopsSoapClient())
            {
                client.Endpoint.Address = new EndpointAddress(addr);
                Console.WriteLine($"{DateTime.Now} -> Start for '{addr}'");
                Stopwatch stopwatch = Stopwatch.StartNew();
                var result = client.ConvertHTML(html);

                if (result.isOK)
                {
                    byte[] fileInBytes = Convert.FromBase64String(result.PdfContent);
                    File.WriteAllBytes($"{client.Endpoint.Address.Uri.Host}.pdf", fileInBytes);
                }

                result.PdfContent = $"ContentLength: {result.PdfContent?.Length}";
                string resultJson = JsonConvert.SerializeObject(result);

                Console.WriteLine($"{DateTime.Now} -> End for '{addr}': {resultJson}  in '{stopwatch.Elapsed:hh\\:mm\\:ss}'");
            }
        }

        private const string longHtml1 = "77u/yj4WST1ejgwJreTzAUw2yj4WST1ejgwJreTzAUw2yj4WST1ejgwJreTzAUw2yj4WST1ejgwJreTzAUw2";
        private const string longHtml2 = "77u/gc89p564IHrMRvQZivtlgc89p564IHrMRvQZivtlgc89p564IHrMRvQZivtlgc89p564IHrMRvQZivtl";
    }
}
