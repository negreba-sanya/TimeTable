using iTextSharp.text.pdf.parser;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ConsoleApp8
{
    class Program
    {
        private static string token { get; set; } = "2013688267:AAF2drgp311S6WeZIDX4Q-X_zthV0sdBx9A";
        public static string url;
        public static string save_path;
        public static string name;
        private static TelegramBotClient client;

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int SW_Min = 2;
        const int SW_Max = 3;
        const int SW_Norm = 4;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [STAThread]

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            //скрыть консоль
            ShowWindow(handle, SW_HIDE);
            /*//отобразить консоль
            ShowWindow(handle, SW_SHOW);
            //свернуть консоль
            ShowWindow(handle, SW_Min);
            //развернуть консоль
            ShowWindow(handle, SW_Max);
            //нормальный размер консоли
            ShowWindow(handle, SW_Norm);*/

            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.ReadLine();
            client.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            string answer = "";
            var msg = e.Message;
            try
            {
                DateTime date = Convert.ToDateTime(msg.Text);
                if (msg.Text != null)
                {
                    try
                    {
                        WebClient wc = new WebClient();
                        url = "https://www.achtng.ru/atng/rasp/" + date.ToString("yyyy_MM") + "/" + date.ToString("yyyy_MM_dd") + ".pdf";
                        save_path = @"";
                        name = date.ToString("yyyy_MM_dd") + ".pdf";
                        wc.DownloadFile(url, save_path + name);
                        answer = date.ToString("dd MMMM yyyy ") + "\n" + "\n";
                        try
                        {
                            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(save_path + name);
                            StringBuilder stringBuil = new StringBuilder();
                            for (int i = 1; i <= reader.NumberOfPages; i++)
                            {
                                stringBuil.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                            }
                            string text_lessons = stringBuil.ToString();

                            Regex regex_1 = new Regex(@"ИСП-19/9(.+\n)+№ ИСП-20/11 Ауд.");
                            MatchCollection matches = regex_1.Matches(text_lessons);

                            if (matches.Count > 0)
                            {
                                foreach (Match match in matches)
                                {
                                    answer += match.Value;
                                }

                            }
                            else
                            {
                                answer = "Совпадений не найдено";
                            }

                            Regex regex_2 = new Regex(@"№ ИСП-20/11 Ауд.");
                            answer = regex_2.Replace(answer, "");
                            reader.Close();
                            System.IO.File.Delete(name);
                        }
                        catch
                        {
                        }
                    }
                    catch
                    {
                        answer = date.ToString("dd MMMM yyyy ") + "\n" + "\n" + "На этот день нет расписания";
                    }
                    await client.SendTextMessageAsync(msg.Chat.Id, answer);
                }
            }
            catch
            {
                await client.SendTextMessageAsync(msg.Chat.Id, "Введите дату в формате \"01.01.2000\"");
            }

        }
    }
}