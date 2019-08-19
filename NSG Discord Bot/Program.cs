using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Discord;
using System.IO;
using Newtonsoft.Json;
using System.Web;
using System.Net;

public class Program
{
	private DiscordSocketClient _client;

	private static Int64 last_time_stamp = 15271402510L;

	private static readonly string[] MONTHS =
	{
		"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
	};

	private static readonly string[] TAMES =
	{

	};

	private static readonly int[] PLAYER_TOTAL_XP =
	{
		0, 0, 5, 20, 40, 70, 120, 190, 270, 360, 450,
		550, 660, 780, 910, 1050, 1200, 1360, 1530, 1710,
		1900, 2100, 2310, 2530, 2760, 3000, 3250, 3510, 3780,
		4060, 4350, 4650, 4960, 5280, 5610, 5950, 6300, 6660,
		7030, 7410, 7800, 8200, 8610, 9030, 9460, 9900, 10350,
		10810, 11280, 11760, 12250, 12750, 13260, 13780, 14310, 14850,
		15400, 15960, 16530, 17110, 17700, 18850, 21078, 22448,
		23908, 25462, 27499, 30249, 34786, 40004, 46805, 54762,
		63714, 73000, 85000, 98000, 112000, 127500, 144500, 163500,
		184500, 207500, 232570, 259896, 289681, 323189, 360886,
		403318, 451484, 506186, 566358, 632547, 705354, 785442,
		873538, 971538, 1083538, 1213538, 1368538, 1558538, 1798538,
		2098538, 2468538, 2918538, 3458538, 4098538, 4208538, 4333538,
		4478538, 4648538, 4848538, 5083538, 5358538, 5678538, 6048538, 6473538, 6873538,
		7273538, 7673538, 8073538, 8473538, 8873538, 9273538, 9673538,
		10073538, 10473538, 10873538, 11273538, 11673538, 12073538,
		12473538, 13373538, 23373538, 33373538, 43373536, 53373536
	};

	private static string[] tweet_message = new string[100];
	private static long[] time_stamps = new long[100];

	private static int write_index = 0;

	public static void Insert(long id)
	{
		time_stamps[write_index] = id;
	}

	public static void Insert(string message)
	{
		tweet_message[write_index] = message;
		++write_index;
	}

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
		_client.MessageReceived += MessageRecieved;

		await _client.LoginAsync(TokenType.Bot, Config.bot.token);
		await _client.StartAsync();
		Console.Write("Guilds: " + _client.Guilds.Count + "\n");
		await Task.Delay(-1);
	}

	private void SendMessage(SocketMessage client, String message)
	{
		client.Channel.SendMessageAsync(message);
	}

	private int SecureParseInt32(ref String number)
	{
		int value = -1;

		try
		{
			value = Int32.Parse(number);
		}
		catch (Exception)
		{
			
		}
		return value;
	}

	private async Task MessageRecieved(SocketMessage message)
	{
		/*
		 * Command Initiated..
		 */
		if (message.Content.StartsWith("::"))
		{
			char[] chararray = message.Content.ToCharArray();
			int index = -1;
			while (++index < chararray.Length)
			{
				if (chararray[index] != ':')
					break;
			}
			for (int i = index; i < chararray.Length; ++i)
				chararray[i] |= (char)0x20;

			string output = new string(chararray, index, chararray.Length - index);

			string[] commands = output.Split(' ');

			if (output.Equals("help"))
			{
				output = "Hey, " + message.Author.Username + " below are the following commands.";
				output += "\n" + "::breedcryo - This will give a list of time in minutes of how long tames are knocked out when cryo sicked.";
			}
			else if (output.Equals("breed"))
			{
				output = "Gigas: 1 Minute , 16 Seconds";
			}
			else if (commands[0].Equals("gif"))
			{
				output = null;
				await message.Channel.SendFileAsync("resources/gif.gif");
			}
			else if (commands[0].StartsWith("exp") || commands[0].StartsWith("xp"))
			{
				if (commands.Length == 1)
				{
					output = "Hey, " + message.Author.Username + " to use this command please do the below.\n";
					output += "This a ranged based experience command.\n";
					output += "I.e. ::xp 100 106 will give you how much experience you need to reach 106 from 100.\n";
				}
				else if (commands.Length == 2)
				{
					int level = SecureParseInt32(ref commands[1]);
					if (level <= 0 || level >= PLAYER_TOTAL_XP.Length)
					{
						output = "[Invalid Format Provided]: Try ::exp 100 as an example to find exp needed to get to level 100.";
					}
					else
					{
						output = "The amount of experience to reach level **" + level + "** is **" + PLAYER_TOTAL_XP[level].ToString("N0") + "** experience.";
					}
				}
				else if (commands.Length == 3)
				{
					int level_start = SecureParseInt32(ref commands[1]);
					int level_end = SecureParseInt32(ref commands[2]);
					if ((level_start >= level_end) || (level_start <= 0) || (level_end >= PLAYER_TOTAL_XP.Length))
					{
						output = "[Invalid Format Provided]: Try ::exp 100 135 as an example to find exp needed from 100 to 135.\n";
						output += (level_start >= level_end) + " , " + (level_start <= 0) + " , " + (level_end > PLAYER_TOTAL_XP.Length) +
							" , " + level_end + " , " + PLAYER_TOTAL_XP.Length;
					}
					else
					{
						output = "The amount of experience to reach level **" + level_end + "** from **" + level_start + "** is **" + (PLAYER_TOTAL_XP[level_end] -  PLAYER_TOTAL_XP[level_start]).ToString("N0") + "** experience.";
					}
				}
			}
			else if (output.StartsWith("kib"))
			{
				if (output.Contains("colors"))
				{
					await message.Channel.SendFileAsync("resources/kibbletypes.png");
				}
			}
			else if (commands[0].Equals("starve"))
			{


			}
			else if (output.Equals("tw"))
			{
				WebClient client = new WebClient();
				string downloadString = client.DownloadString("https://twitter.com/complexminded?lang=en");
				string[] lines = downloadString.Split('\n');
				System.IO.File.WriteAllLines("Lines.txt", lines);
				string tweet_date = "tweet-timestamp js-permalink js-nav js-tooltip" + '"' + " title=" + '"';

				for (int line = 0; line < lines.Length; ++line)
				{
					if (lines[line].Contains(tweet_date))
					{
						int idx = lines[line].IndexOf(tweet_date);
						string date_stamp = lines[line].Substring(idx > 0 ? idx + tweet_date.Length : 0, idx > 0 ? 35 : 0);
						date_stamp = date_stamp.Substring(0, date_stamp.IndexOf("data") - 3);
						int hours = Int32.Parse(date_stamp.Substring(0, date_stamp.IndexOf(":")));
						int minutes = Int32.Parse(date_stamp.Substring(date_stamp.IndexOf(":") + 1, 2));

						if (date_stamp.Contains("PM -") && hours < 12)
							hours += 12;

						idx = date_stamp.IndexOf(" - ");
						date_stamp = date_stamp.Substring(idx + 3, date_stamp.Length - idx - 3);

						string[] dates = date_stamp.Split(' ');
						int day = Int32.Parse(dates[0]);
						int month = 0;

						for (int i = 0; i < 12; ++i)
						{
							if (MONTHS[i].Equals(dates[1]))
								month = i + 1;
						}
						int year = Int32.Parse(dates[2]);

						last_time_stamp = day | (month << 5) | (year << 10) | (hours << 23) | (minutes << 29);
						Insert(last_time_stamp);
						continue;
					}

					string tweet_message = "TweetTextSize TweetTextSize--normal js-tweet-text tweet-text";
					if (lines[line].Contains(tweet_message))
					{
						string message_substring = lines[line].Substring(lines[line].IndexOf('>'));
						int end_slice = message_substring.LastIndexOf("<") - 1;
						message_substring = message_substring.Substring(end_slice > 0 ? 1 : 0, end_slice > 0 ? end_slice : 0);
						StringBuilder sb = new StringBuilder();
						sb.Append("\n\n\n\nComposed By: @ComplexMinded");
						sb.Append("\nMessage: " + message_substring);
						long day = last_time_stamp & 31L;
						long month = (last_time_stamp >> 5) & 31L;
						long year = (last_time_stamp >> 10) & 4095L;
						long hours = (last_time_stamp >> 23) & 31L;
						long minutes = (last_time_stamp >> 29);
						sb.Append("\nDate Created: " + month + "/" + day + "/" + year + "  at " + hours + ":" + minutes);
						if (output.Length + sb.ToString().Length < 2000)
							output += sb.ToString();
						break;
					}
				}
			}
			else
			{
				output = "Hey, " + message.Author.Username + " the Command ''" + output + "'' wasn't found use ::help for more information.";
			}

			if (output != null)
				await message.Channel.SendMessageAsync(output);

			//output = "ChannelID: " + message.Channel.Id;
			//await message.Channel.SendMessageAsync(output);
			//var tweet_channel = _client.GetChannel(611227339145216010) as IMessageChannel; // 4
			//await tweet_channel.SendMessageAsync(output);
		}
	}

	//ark-twitter-feed : 611227339145216010
	//ark-patch-notes : 611227474306662516
	//rules and info : 611227474306662516
	//announcements : 611225185068253214

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