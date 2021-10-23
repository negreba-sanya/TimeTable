using iTextSharp.text.pdf.parser;
using System;
using System.Windows.Input;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Telegram.Bot.Types.ReplyMarkups;

namespace TimeTable
{
    class Program
    {
        private static string token { get; set; } = "2013688267:AAF2drgp311S6WeZIDX4Q-X_zthV0sdBx9A";
        public static string url;
        public static string save_path;
        public static string name;
        private static TelegramBotClient client;
        const string name_program = "TimeTable";
        public static string mode = "none";
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

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
            SetAutorunValue(true);
            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.ReadLine();
            client.StopReceiving();
        }


        // метод вызываемый при отправке сообщения пользователем
        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            string answer;
            var msg = e.Message;

            if (msg != null)
            {
                if (mode == "none")
                    switch (msg.Text)
                    {
                        case "Расписание":
                            mode = "time";
                            await client.SendTextMessageAsync(
                                        chatId: msg.Chat.Id,
                                        text: "Введите дату в формате \"01.01.2000\"",
                                        replyMarkup: GetButtons_three()
                                        );
                            break;
                        case "Преподаватели":
                            mode = "teachers";
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                        text: "Введите фамилию преподавателя в формате \"Иванов\"",
                                        replyMarkup: GetButtons_two());
                            break;
                        default:
                            mode = "none";
                            await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                        text: "Выберите команду:",
                                        replyMarkup: GetButtons());
                            break;
                    }
                    else
                    {
                        switch (mode)
                        {
                            case "time":
                            if (msg.Text == "Назад")
                            {
                                mode = "none";
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                        text: "Выберите команду:",
                                        replyMarkup: GetButtons());
                            }
                            else
                            {
                                switch (msg.Text)
                                {                                    
                                    case "Сегодня":
                                        try
                                        { // формирование ссылки и скачивание файла с дальнейшим распознаванием и парсингом
                                            WebClient wc = new WebClient();
                                            url = "https://www.achtng.ru/atng/rasp/" + DateTime.Now.ToString("yyyy_MM") + "/" + DateTime.Now.ToString("yyyy_MM_dd") + ".pdf";
                                            save_path = @"";
                                            name = DateTime.Now.ToString("yyyy_MM_dd") + ".pdf";
                                            wc.DownloadFile(url, save_path + name);
                                            answer = DateTime.Now.ToString("dd MMMM yyyy ") + "\n" + "\n";
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
                                                string day = "";
                                                switch (Convert.ToInt32(DateTime.Now.DayOfWeek))
                                                {
                                                    case 1:
                                                        day = "понедельник";
                                                        break;
                                                    case 2:
                                                        day = "вторник";
                                                        break;
                                                    case 3:
                                                        day = "среда";
                                                        break;
                                                    case 4:
                                                        day = "четверг";
                                                        break;
                                                    case 5:
                                                        day = "пятница";
                                                        break;
                                                    case 6:
                                                        day = "суббота";
                                                        break;
                                                }


                                                if (matches.Count > 0)
                                                {
                                                    foreach (Match match in matches)
                                                    {
                                                        answer += match.Value.Replace("\nРасписание создано в 1С:Колледж с помощью обработки 'Мастер создания расписания'(Автор: Денис Буторин http://butorin.org)Расписание на " + DateTime.Now.ToString("dd MMMM yyyy") + " (" + day + ")   - продолжение стр.2", "");
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
                                        { // вызывается при отсутсвии расписания на сайте по сформированной ссылке и вносит текст в переменную answer
                                            answer = DateTime.Now.ToString("dd MMMM yyyy ") + "\n" + "\n" + "На этот день нет расписания";
                                        }
                                        // вывод переменной в чат с пользователем 
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                            text: answer,
                                            replyMarkup: GetButtons_three()
                                            );
                                        break;
                                    case "Завтра":
                                        DateTime date_2 = DateTime.Today.AddDays(+1);
                                        try
                                        { // формирование ссылки и скачивание файла с дальнейшим распознаванием и парсингом
                                            
                                            WebClient wc = new WebClient();
                                            url = "https://www.achtng.ru/atng/rasp/" + date_2.ToString("yyyy_MM") + "/" + date_2.ToString("yyyy_MM_dd") + ".pdf";
                                            save_path = @"";
                                            name = date_2.ToString("yyyy_MM_dd") + ".pdf";
                                            wc.DownloadFile(url, save_path + name);
                                            answer = date_2.ToString("dd MMMM yyyy ") + "\n" + "\n";
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

                                                string day = "";
                                                switch (Convert.ToInt32(date_2.DayOfWeek))
                                                {
                                                    case 1:
                                                        day = "понедельник";
                                                        break;
                                                    case 2:
                                                        day = "вторник";
                                                        break;
                                                    case 3:
                                                        day = "среда";
                                                        break;
                                                    case 4:
                                                        day = "четверг";
                                                        break;
                                                    case 5:
                                                        day = "пятница";
                                                        break;
                                                    case 6:
                                                        day = "суббота";
                                                        break;
                                                }

                                                if (matches.Count > 0)
                                                {
                                                    foreach (Match match in matches)
                                                    {
                                                        answer += match.Value.Replace("\nРасписание создано в 1С:Колледж с помощью обработки 'Мастер создания расписания'(Автор: Денис Буторин http://butorin.org)Расписание на " + date_2.ToString("dd MMMM yyyy") + " (" + day + ")   - продолжение стр.2", "");
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
                                        { // вызывается при отсутсвии расписания на сайте по сформированной ссылке и вносит текст в переменную answer
                                            answer = date_2.ToString("dd MMMM yyyy ") + "\n" + "\n" + "На этот день нет расписания";
                                        }
                                        // вывод переменной в чат с пользователем 
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                            text: answer,
                                            replyMarkup: GetButtons_three()
                                            );
                                        break;
                                    case "Послезавтра":
                                        DateTime date_3 = DateTime.Today.AddDays(+2);
                                        try
                                        { // формирование ссылки и скачивание файла с дальнейшим распознаванием и парсингом

                                            WebClient wc = new WebClient();
                                            url = "https://www.achtng.ru/atng/rasp/" + date_3.ToString("yyyy_MM") + "/" + date_3.ToString("yyyy_MM_dd") + ".pdf";
                                            save_path = @"";
                                            name = date_3.ToString("yyyy_MM_dd") + ".pdf";
                                            wc.DownloadFile(url, save_path + name);
                                            answer = date_3.ToString("dd MMMM yyyy ") + "\n" + "\n";
                                            try
                                            {
                                                string day = "";
                                                switch (Convert.ToInt32(date_3.DayOfWeek))
                                                {
                                                    case 1:
                                                        day = "понедельник";
                                                        break;
                                                    case 2:
                                                        day = "вторник";
                                                        break;
                                                    case 3:
                                                        day = "среда";
                                                        break;
                                                    case 4:
                                                        day = "четверг";
                                                        break;
                                                    case 5:
                                                        day = "пятница";
                                                        break;
                                                    case 6:
                                                        day = "суббота";
                                                        break;
                                                }
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
                                                        answer += match.Value.Replace("\nРасписание создано в 1С:Колледж с помощью обработки 'Мастер создания расписания'(Автор: Денис Буторин http://butorin.org)Расписание на " + date_3.ToString("dd MMMM yyyy") + " (" + day + ")   - продолжение стр.2", "");
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
                                        { // вызывается при отсутсвии расписания на сайте по сформированной ссылке и вносит текст в переменную answer
                                            answer = date_3.ToString("dd MMMM yyyy ") + "\n" + "\n" + "На этот день нет расписания";
                                        }
                                        // вывод переменной в чат с пользователем 
                                        await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                            text: answer,
                                            replyMarkup: GetButtons_three()
                                            );
                                        break;
                                    default:
                                        try
                                        {// попытка преобразовать текст сообщения в дату
                                            DateTime date_4 = Convert.ToDateTime(msg.Text);
                                            try
                                            { // формирование ссылки и скачивание файла с дальнейшим распознаванием и парсингом
                                                WebClient wc = new WebClient();
                                                url = "https://www.achtng.ru/atng/rasp/" + date_4.ToString("yyyy_MM") + "/" + date_4.ToString("yyyy_MM_dd") + ".pdf";
                                                save_path = @"";
                                                name = date_4.ToString("yyyy_MM_dd") + ".pdf";
                                                wc.DownloadFile(url, save_path + name);
                                                answer = date_4.ToString("dd MMMM yyyy ") + "\n" + "\n";
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

                                                    string day = "";
                                                    switch (Convert.ToInt32(date_4.DayOfWeek))
                                                    {
                                                        case 1:
                                                            day = "понедельник";
                                                            break;
                                                        case 2:
                                                            day = "вторник";
                                                            break;
                                                        case 3:
                                                            day = "среда";
                                                            break;
                                                        case 4:
                                                            day = "четверг";
                                                            break;
                                                        case 5:
                                                            day = "пятница";
                                                            break;
                                                        case 6:
                                                            day = "суббота";
                                                            break;
                                                    }

                                                    if (matches.Count > 0)
                                                    {
                                                        foreach (Match match in matches)
                                                        {
                                                            answer += match.Value.Replace("\nРасписание создано в 1С:Колледж с помощью обработки 'Мастер создания расписания'(Автор: Денис Буторин http://butorin.org)Расписание на " + date_4.ToString("dd MMMM yyyy") + " (" + day + ")   - продолжение стр.2", "");
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
                                            { // вызывается при отсутсвии расписания на сайте по сформированной ссылке и вносит текст в переменную answer
                                                answer = date_4.ToString("dd MMMM yyyy ") + "\n" + "\n" + "На этот день нет расписания";
                                            }
                                            // вывод переменной в чат с пользователем 
                                            await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                                text: answer,
                                                replyMarkup: GetButtons_three()
                                                );

                                        }
                                        catch
                                        {
                                            await client.SendTextMessageAsync(
                                                chatId: msg.Chat.Id,
                                                text: "Введите дату в формате \"01.01.2000\"",
                                                replyMarkup: GetButtons_two()
                                                );
                                        }
                                        break;
                                }

                            }
                                break;
                            case "teachers":
                            if (msg.Text == "Назад")
                            {
                                mode = "none";
                                await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                        text: "Выберите команду:",
                                        replyMarkup: GetButtons());
                            }
                            else
                            {
                                try
                                {// попытка сравнить текст сообщения с фамилией преподавателя из списка
                                    await client.SendTextMessageAsync(
                                        chatId: msg.Chat.Id,
                                        text: Teachers(msg.Text),
                                        replyMarkup: GetButtons_two());
                                }
                                catch
                                {// при отрицательном результате предыдущих условий
                                    await client.SendTextMessageAsync(chatId: msg.Chat.Id,
                                        text: "Введите фамилию преподавателя в формате \"Иванов\"",
                                        replyMarkup: GetButtons_two());
                                }
                            }
                                break;
                        }
                    }
            }
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = "Расписание" } },
                    new List<KeyboardButton> { new KeyboardButton { Text = "Преподаватели" } }
                }
            };
        }
        private static IReplyMarkup GetButtons_two()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = "Назад" } }
                }
            };
        }
        private static IReplyMarkup GetButtons_three()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton { Text = "Сегодня" }, new KeyboardButton { Text = "Завтра" }, new KeyboardButton { Text = "Послезавтра" } },
                    new List<KeyboardButton> { new KeyboardButton { Text = "Назад" } }
                }
            };
        }

        // метод автозагрузки
        public static bool SetAutorunValue(bool autorun)
        {
            string ExePath = AppDomain.CurrentDomain.BaseDirectory;
            
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (autorun)
                    reg.SetValue(name_program, ExePath + "TimeTable.exe");
                else
                    reg.DeleteValue(name_program);

                reg.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        // метод проверки на поля на совпадение с фамилиями преподавателей
        public static string Teachers(string msg)
        {
            string answ = "";
            List<string[]> hrefTags = new List<string[]>();
            var client = new WebClient();
            string html = client.DownloadString("https://www.achtng.ru/staff");
            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);
            foreach (IElement element in document.GetElementsByClassName("staffer-staff-title"))
            {
                string[] a = Convert.ToString(element.TextContent).Replace("\t", "").Replace("\n", "").Split(" ");
                hrefTags.Add(a);
            }
            foreach (string[] i in hrefTags)
            {
                if (i[0] == msg)
                {
                    answ = i[0] + " " + i[1] + " " + i[2];
                }
            }
            return answ;
        }
    }
}