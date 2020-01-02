using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace vimeoLinkParser2
{
    class Program
    {
        static string category = string.Empty;
        static int maxPage = 0;
        static int totalThreads = 0;
        static Queue<string> URLs = new Queue<string>();
        static List<string> HTMLs = new List<string>();
        static object URLlocker = new object();
        static object HTMLlocker = new object();
        static void Main(string[] args)
        {
            //ввод данных
            /*Console.Write("Категория: ");
            category = Console.ReadLine();
            Console.Write("Всего страниц: ");
            maxPage = Convert.ToInt32(Console.ReadLine());
            Console.Write("Потоков: ");  
            totalThreads = Convert.ToInt32(Console.ReadLine());
            */
            if(File.Exists(@"result.txt"))
            {
                File.Delete(@"result.txt");
            }
            category = "comedy";
            maxPage = 2008;
            totalThreads = 5;

            for(int i = 1; i < maxPage; i++)
            {
                URLs.Enqueue(@"http://vimeo.com/categories/" + category + @"/videos/page:" + i + "/sort:relevant/format:thumbnail");
            }

            for (int i = 0; i < totalThreads; i++)
                (new Thread(new ThreadStart(Download))).Start();
            Console.WriteLine("END");           
                Console.ReadKey();
        }

        public static void Download()
        {            
            while (true)
            {laba44:
                try
                {
                    string URL;
                    //блокируем очередь URL и достаем оттуда один адрес
                    lock (URLlocker)
                    {
                        if (URLs.Count == 0)
                            break;//адресов больше нет, выходим из метода, завершаем поток
                        else
                            URL = URLs.Dequeue();
                    }
                    Console.WriteLine(URL + " - start downloading ...");
                    //скачиваем страницу
                    WebRequest request = WebRequest.Create(URL);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string HTML = (new StreamReader(response.GetResponseStream())).ReadToEnd();
                    //блокируем список скачанных страниц, и заносим туда свою страницу
                    lock (HTMLlocker)
                    {
                        Regex newReg = new Regex("data-position=\"[0-9]{1,5}\">\\s*<a href=\"/(?<idn>.*?)\" title=\"(?<title>.*?)\">", RegexOptions.Singleline);
                        MatchCollection matches = newReg.Matches(HTML);

                        if (matches.Count > 0)
                        {
                            foreach (Match mat in matches)
                            {
                                Console.WriteLine(mat.Groups["idn"].Value + " " + System.Net.WebUtility.HtmlDecode(mat.Groups["title"].ToString()));
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"result.txt", true))
                                {
                                    file.WriteLine(mat.Groups["idn"].Value + ";;;;;" + System.Net.WebUtility.HtmlDecode(mat.Groups["title"].ToString()));
                                }
                            }
                            //Console.WriteLine(matches.Count);
                            //Console.ReadKey();
                        }
                        //HTMLs.Add(HTML);
                    }

                    //
                    Console.WriteLine(URL + " - downloaded (" + HTML.Length + " bytes)");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(15000);
                    goto laba44;
                }
                /////
            }
        }
    }
}
