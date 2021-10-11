using iTextSharp.text.pdf.parser;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Reflection;
using Microsoft.Win32;

namespace ConsoleApp8
{
    class Program
    {
        private static string token { get; set; } = "2013688267:AAF2drgp311S6WeZIDX4Q-X_zthV0sdBx9A";
        public static string url;
        public static string save_path;
        public static string name;
        private static TelegramBotClient client;

        static void Main(string[] args)
        {
            SetAutoRunValue(true, Assembly.GetExecutingAssembly().Location);
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

        private static bool SetAutoRunValue(bool autorun, string path)
        {
            const string name = "HelloApp";
            string ExePath = path;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            try
            {
                if (autorun)
                {
                    reg.SetValue(name, ExePath);
                }
                else
                {
                    reg.DeleteValue(name);
                }
                reg.Flush();
                reg.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}