using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace SHOP_DATA
{
    internal class Program
    {
        private static TelegramBotClient botClient;
        private static HttpClient httpClient;

        static void Main(string[] args)
        {
            string tokenBot = ConfigurationManager.AppSettings["tokenBot"];
            if (string.IsNullOrEmpty(tokenBot))
            {
                Console.WriteLine("Bot token is missing in App.config.");
                return;
            }

            botClient = new TelegramBotClient(tokenBot);
            httpClient = new HttpClient();

            botClient.OnMessage += BotClient_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Bot started. Press ENTER to stop...");
            Console.ReadLine();
            botClient.StopReceiving();
        }

        private static async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message?.Text == null) return;

            string chatID = e.Message.Chat.Id.ToString();
            string message = e.Message.Text.Trim();

            if (message.Equals("Dk", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    int number = int.Parse(ConfigurationManager.AppSettings["number"]);
                    string userName = $"kiendev{number}@gmail.com";
                    string password = "1122334455";
                    string url = ConfigurationManager.AppSettings["url"];

                    await UpdateAppSetting("number", (number + 1).ToString());

                    var formData = new Dictionary<string, string>
                    {
                        { "email", userName },
                        { "password", password },
                        { "invite_code", "" },
                        { "email_code", "" }
                    };

                    var content = new FormUrlEncodedContent(formData);

                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    string responseBody = await response.Content.ReadAsStringAsync();

                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);

                    if (jsonObject.data != null)
                    {
                        await botClient.SendTextMessageAsync(chatID, $"<b>Register Success</b>\nEmail: {userName}",Telegram.Bot.Types.Enums.ParseMode.Html);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatID, $"{jsonObject.message}");
                    }

                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat, $"Error: {ex.Message}");
                }
            }
        }

        private static async Task UpdateAppSetting(string key, string newValue)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] != null)
            {
                config.AppSettings.Settings[key].Value = newValue;
            }
            else
            {
                config.AppSettings.Settings.Add(key, newValue);
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            await Task.CompletedTask;
        }
    }
}
