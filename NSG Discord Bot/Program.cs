using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.IO;
using Newtonsoft.Json;

public class Program
{
	private DiscordSocketClient _client;

	/*
	 * Default Required To Initialize the MainAsync() method.
	*/
	public static void Main(string[] args)
	{
		new Program().MainAsync().GetAwaiter().GetResult();
	}

	public async Task MainAsync()
	{
		if (Config.bot.token == "" || Config.bot.token == null)
			return;

		_client = new DiscordSocketClient();
		_client.Log += Log;
		_client.MessageReceived += MessageReceived;

		await _client.LoginAsync(TokenType.Bot, Config.bot.token);
		await _client.StartAsync();
		await Task.Delay(-1);
	}

	/*
	 * Network Message Socket Handler Basic SocketMessage system.
	 */
	private async Task MessageReceived(SocketMessage message)
	{
		/*
		 * Command Initiated..
		 */
		if (message.Content.StartsWith("::"))
		{
			await message.Channel.SendMessageAsync("Hey, " + message.Author.Username + " the Command ''" + message.Content + "'' wasn't found use ::help for more information.");
		}
	}

	/*
	 * Allows DiscordSocketClient to output messages to command prompt. 
	 */
	private Task Log(LogMessage msg)
	{
		Console.WriteLine(msg.ToString());
		return Task.CompletedTask;
	}


	class Config
	{
		private const string configFolder = "Resources";
		private const string configFile = "config.json";

		public static BotConfig bot;

		static Config()
		{
			if (!Directory.Exists(configFolder))
				Directory.CreateDirectory(configFolder);

			if (!File.Exists(configFolder + "/" + configFile))
			{
				bot = new BotConfig();
				string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
				File.WriteAllText(configFolder + "/" + configFile, json);
			}
			else
			{
				string json = File.ReadAllText(configFolder + "/" + configFile);
				bot = JsonConvert.DeserializeObject<BotConfig>(json);
			}
		}
	}

	public struct BotConfig
	{
		public string token;
		public string cmdPrefix;
	}
}