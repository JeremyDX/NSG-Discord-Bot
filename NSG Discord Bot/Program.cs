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
using System.Collections;


public class Program
{

	public static readonly Task EmptyTask = Task.CompletedTask;

	private static DiscordSocketClient _client;
	private static CacheReader cache;
		int command_size;

	enum Commands
	{
		EXP = 1,
		TURRETS = 6,
		BREED = 4,
		IMPRINT = 11,
		SET = 12,
	};

	private static void NULL() { }

	public static String GetUser(long id)
	{
		try
		{
			return _client.GetUser((ulong)id).Username;
		}
		catch (Exception)
		{
			return "NULL";
		}
	}

	/*
	 * Default Required To Initialize the MainAsync() method.
	*/
	public static void Main(string[] args)
	{
		Program p = new Program();

		int length = (int)CacheReader.Open();
		cache = CacheReader.SeekCache(0, length, length + 8192);
		CacheReader.Close();

		Task task = p.MainAsync();
		System.Runtime.CompilerServices.TaskAwaiter await = task.GetAwaiter();
		try
		{
			await.GetResult();
		} catch (Exception e)
		{
			Console.WriteLine(e.ToString());
		}
	}

	public async Task MainAsync()
	{
		if (Config.bot.token == "" || Config.bot.token == null)
			return;

		_client = new DiscordSocketClient();
		_client.Log += Log;
		_client.MessageReceived += MessageReceived;

		_client.LoginAsync(TokenType.Bot, Config.bot.token);
		_client.StartAsync();
		await Task.Delay(-1);
	}

	private Task MessageReceived(SocketMessage connection)
	{
		String message = "";
		try
		{

			if (connection.Content.Length < 3 || !(connection.Content[0] == ':' && connection.Content[1] == ':'))
				return EmptyTask;

			char[] input = connection.Content.ToCharArray();

			short start = 1;
			short end = (short)input.Length;

			while (++start < input.Length)
			{
				if (!(input[start] == ':' || input[start] == ' '))
					break;
			}

			while (--end > start)
			{
				if (input[end] == ':' || input[end] == ' ')
				{
					continue;
				}
				break;
			}

			if (start > end)
				return EmptyTask;

			StringBuilder sb = new StringBuilder();

			bool space = false;

			command_size = 1;

			for (int i = start; i <= end; ++i)
			{
				if (input[i] == ' ')
				{
					if (!space)
					{
						++command_size;
						sb.Append(' ');
					}
					space = true;
				}
				else
				{
					space = false;
					sb.Append(input[i]);
				}
			}

			String[] commands = sb.ToString().ToLower().Split(' ');

			int compare_id = HashCompare(commands[0]);

			if (compare_id == -1)
			{
				return connection.Channel.SendMessageAsync("Sorry, that wasn't a valid command.\nYou can use ::help command to learn more.");
			}
			long flags = 0;
			int flagscheck = 0;

			if (commands.Length > 1 && commands[1].Length > 1 && commands[1][0] == '-')
			{
				flags |= 1; //Flags Checked.
							//FLAGS ENABLED.
				for (int i = 1; i < commands[1].Length; ++i)
				{
					flagscheck = 1;
					flags |= (1L << (commands[1][i] - 95)); //Bit 2-27 Enabled.
				}
			}

			Console.Write("Command ID: " + compare_id + ", Compared : " + commands[0] + "\n");

			if (compare_id == 1)
			{
				if (commands.Length < 2)
				{
					message += "::exp command was given invalid parameters. type ::help -f exp for more information.\n";
				}
				else if (commands.Length == 2 || (commands.Length == 3 && flags > 0))
				{
					if (flags > 0 && commands.Length == 2)
					{
						message += "::exp command was given invalid parameters. type ::help -f exp for more information.\n";
					}
					else
					{
						int level = Utility.SecureParseInt32(ref commands[1 + flagscheck]);
						if (level <= 0 || level >= Utility.PLAYER_TOTAL_XP.Length)
						{
							message += "[Invalid Format Provided]: The Levels Cannot Exceed a Range of 1 through 135.";
						}
						else
						{
							String s = Utility.ConvertSecondsToTime(Utility.PLAYER_TOTAL_XP[level] / 5);
							int stone = 1 + (Utility.PLAYER_TOTAL_XP[level] / 3);

							message += "The amount of experience to reach level **" + level + "** is **" + Utility.PLAYER_TOTAL_XP[level].ToString("N0") + "** experience.\n";

							if ((flags & (1L << ((byte)'t') - 95)) != 0)
							{
								message += "You'll have to sleep in a Tek Pod for roughly: **" + s + "** amount of time!\n";
							}
							if ((flags & (1L << ((byte)'s') - 95)) != 0)
							{
								message += "This would also require **" + stone.ToString("N0") + "** worth of stone grinded.\n";
							}
						}
					}
				}
				else if (commands.Length == 3 || (commands.Length == 4 && flags > 0))
				{
					int level_start = Utility.SecureParseInt32(ref commands[1 + flagscheck]);
					int level_end = Utility.SecureParseInt32(ref commands[2 + flagscheck]);

					if ((level_start >= level_end) || (level_start <= 0) || (level_end >= Utility.PLAYER_TOTAL_XP.Length))
					{
						message += "[Invalid Format Provided]: The Levels Cannot Exceed a Range of 1 through 135.";
					}
					else
					{
						String s = Utility.ConvertSecondsToTime((Utility.PLAYER_TOTAL_XP[level_end] - Utility.PLAYER_TOTAL_XP[level_start]) / 5);
						int stone = 1 + ((Utility.PLAYER_TOTAL_XP[level_end] - Utility.PLAYER_TOTAL_XP[level_start]) / 3);

						message += "The amount of experience to reach level **" + level_end + "** from **" + level_start + "** is **" +
							(Utility.PLAYER_TOTAL_XP[level_end] - Utility.PLAYER_TOTAL_XP[level_start]).ToString("N0") + "** experience.\n";
						if ((flags & (1L << ((byte)'t') - 95)) != 0)
						{
							message += "You'll have to sleep in a Tek Pod for roughly: **" + s + "** amount of time!\n";
						}
						if ((flags & (1L << ((byte)'s') - 95)) != 0)
						{
							message += "This would also require **" + stone.ToString("N0") + "** worth of stone grinded.\n";
						}
					}
				}
			}
			else if (compare_id == 2)
			{
				if (flags == 0)
				{
					if (commands.Length == 1)
					{
						message += "Currently there are " + 6 + " commands available.\n";
						message += "::help -l , This will list all the commands available.\n";
						message += "::help COMMAND_NAME , this will give assistance on a specific command.\n";
					}
					else
					{
						int hash = HashCompare(commands[1]);
						if (hash == (int)Commands.BREED)
						{
							message += "N/A Yet\n";
						}
						else if (hash == (int)Commands.BREED)
						{
							message += "N/A Yet\n";
						}
						else if (hash == (int)Commands.BREED)
						{
							message += "N/A Yet\n";
						}
					}
				}
				else if ((flags & (1L << ((byte)'l') - 95)) != 0)
				{
					//L Flag Enabled.
					message += "::exp\n";
					message += "::turrets\n";
					message += "::breed\n";
				}
			}
			else if (compare_id == 3)
			{
				bool entermode = commands.Length > 1 && (connection.Author.Id == 424171497683288073 || connection.Author.Id == 340226013378248715);


				if (!entermode)
				{
					bool success = cache.WriteLongNotContained(connection.Author.Id);
					if (false)
					{
						if (success)
						{
							message += "Hello, **" + connection.Author.Username + "** your submission to the contest has been noted.";
							message += "The entry dates will end on Sunday September 8th 2019 at 10AM EST.";
							message += "We will then declare 5 winners to recieve a Managarmr with a saddle.";
							message += "You'll have until Tuesday September 10th 2019 at 10AM EST to CLAIM this reward.";
						}
						else
						{
							message += "Hello, **" + connection.Author.Username + "** you have already submitted into this contest.";
						}
					}
					else
					{
						message += "Sorry, the entry for this contest has ended as of September 8th 2019 10:00am EST.";
						message += "Stayed tuned in our discord for more information on the next give away.";
					}
				}
				else
				{
					for (int i = 1; i < commands.Length; ++i)
					{
						ulong value = 0;
						try
						{
							value = ulong.Parse(commands[i]);
							bool success = cache.WriteLongNotContained(value);
							if (success)
								message += GetUser((long)value) + ", Sucessfully Added.";
							else
								message += GetUser((long)value) + ", Already Exists.";
						}
						catch (Exception e)
						{

						}
					}
				}
			}
			else if (compare_id == 5)
			{
				if (connection.Author.Id == 424171497683288073 || connection.Author.Id == 340226013378248715)
				{
					if (commands.Length == 1)
					{
						long size = cache.WriterIndex() / 8;
						message += "There are currently " + size + " submissions to this contest.";
						cache.Seek(0);
						int skip = 0;
						message += '\n';
						for (int i = 0; i < size; ++i)
						{
							message += GetUser(cache.ReadLong()) + "   ";
							if (++skip == 4)
							{
								skip = 0;
								message += '\n';
							}
						}
					}
					else
					{
						if ((flags & (1L << ((byte)'c') - 95)) != 0)
						{
							Utility.polled_members = 0;
							message += "You have cleared the polling list.";
						}
						else if ((flags & (1L << ((byte)'p') - 95)) != 0)
						{
							int winner_size = 1;
							if (commands.Length == 3)
								winner_size = int.Parse(commands[2]);

							int count = 0;


							Utility.polled_members |= 1L << 39;
							Utility.polled_members |= 1L << 36;
							Utility.polled_members |= 1L << 33;
							Utility.polled_members |= 1L << 28;
							Utility.polled_members |= 1L << 20;
							Utility.polled_members |= 1L << 7;

							count += 7;

							Random random = new Random();

							for (int i = 0; i < winner_size; ++i)
							{
								long size = cache.WriterIndex() / 8;
								int rand = random.Next(1, (int)(size - 1));

								bool searching = true;
								while (searching && count <= size - 1)
								{
									cache.Seek(8 * rand);
									long id = cache.ReadLong();

									try
									{
										if ((Utility.polled_members & (long)(1L << rand)) == 0)
										{
											Utility.polled_members |= (long)(1L << rand);
											message += "Polling Member: " + GetUser(id) + "\n";
											searching = false;
											++count;
										}
										else
										{
											if (++rand >= size)
												rand = 1;
										}
									}
									catch (Exception e) { Console.WriteLine(e.ToString()); }
								}
							}
						}
					}
				}
				else
				{
					message += "You must be an adminstrator to perform this command.";
				}
			}
			else if (compare_id == 4)
			{
				int breed_id = -1;
				if (commands.Length > 1)
				{
					breed_id = GetTameId(commands[1]);
					if (breed_id != -1 && commands.Length > 2)
					{
						//12 = 1:12:00 Hours.
						double weight = Utility.SecureParseDouble(ref commands[2]);
						if (weight >= 0)
						{
							bool herb = (Utility.HERBIVORE & (1 << (breed_id + 1))) != 0;
							if (Utility.MAX_MEAT_TO_JUVY[breed_id] != -1 && ((weight * 10UL) >= Utility.MAX_MEAT_TO_JUVY[breed_id]))
							{
								message += "Your " + Utility.TAMES[breed_id] + " will reach Juvenile. You only need " +
									Utility.FormatCharIntToDecimal(Utility.MAX_MEAT_TO_JUVY[breed_id], 1) + "kg of " + (herb ? "berries" : "meat") + ".";
							}
							else
							{

								String time = Utility.ConvertSecondsToTime((int)(weight * Utility.STARVE_VALUES[breed_id]));
								int percent = (int)(Utility.GROWTH_PER_SECOND[breed_id] * weight * Utility.STARVE_VALUES[breed_id] * 10);
								string growth = Utility.FormatCharIntToDecimal(percent, 1);
								message += "With " + weight + "kg of " + (herb ? "berries" : "meat") + " your " + Utility.TAMES[breed_id] + " will last " + time + "(+" + growth + "%) of time unattended before refill is required.\n";
							}
						}
					}
				}
			}
			else if (compare_id == (int)Commands.IMPRINT)
			{
				if (commands.Length == 3)
				{
					double imprint = double.Parse(commands[1]);
					double imp_stat = double.Parse(commands[2]);
					double base_stat = imp_stat;
					imp_stat /= 500.0;
					base_stat /= (1 + imp_stat);
					base_stat *= 10;
					string s_base = Utility.FormatCharIntToDecimal((long)base_stat, 1);
					message += "With a " + imprint + " imprint your tames stat will be " + s_base + " when unleveled from " + imp_stat;
				}
			}
			
			else if (compare_id == 6)
			{
				bool autos = (flags & (1L << ((byte)'a') - 95)) != 0;
				bool heavies = (flags & (1L << ((byte)'h') - 95)) != 0;
				bool tek = (flags & (1L << ((byte)'t') - 95)) != 0;

				/*
				 * 
				 Auto/Heavy = 1 Shot = 0.4250 Seconds or 0.170 In Game Minutes.
				 Tek		= 1 Shot = 0.3375 Seconds or 0.135 In Game Minutes.
				 * 
				 */

				int message_idx = 1;

				if (flags != 0)
					message_idx = 2;

				if (commands.Length > message_idx)
				{
					if (commands[message_idx].Contains(':'))
					{
						String[] time = commands[message_idx].Split(':');
						int minutes = int.Parse(time[0]);
						int seconds = int.Parse(time[1]) + (minutes * 60);
						int bullets = (int)(seconds / 0.4250);
						int shards = (int)(seconds / 0.3375);
						//Provided Time Stamp.
					}
					else
					{
						//Provided Bullets Amount.
						int bullets = 0;
						if (commands[message_idx].Equals("max"))
						{
							bullets = int.MaxValue;
						}
						else
						{
							bullets = int.Parse(commands[message_idx]);
						}
						int seconds_bullets = (int)((bullets > 1400 ? 1400 : bullets) * 0.3525);
						int minutes_bullets = (int)((bullets > 1400 ? 1400 : bullets) * 0.1285);

						int seconds_quad_bullets = (int)(((((bullets > 5800 ? 5800 : bullets) / 4) * 4) / 4) * 0.3525);
						int minutes_quad_bullets = (int)(((((bullets > 5800 ? 5800 : bullets) / 4) * 4) / 4) * 0.1285);

						int seconds_shards = (int)((bullets > 5000 ? 5000 : bullets) * 0.3525);
						int minutes_shards = (int)((bullets > 5000 ? 5000 : bullets) * 0.1285);

						if (autos)
						{
							message += "**Auto Turrets -** - " + (bullets > 1400 ? 1400 : bullets) + " bullets will take **" + Utility.FormatSecondsToTime(seconds_bullets) +
								"** to drain.\nThis is **" + Utility.FormatSecondsToArkTime(minutes_bullets) + "** Converted To Game Time.\n\n";
						}
						if (heavies)
						{
							message += "**Heavy Turrets - ** - " + ((((bullets > 5800 ? 5800 : bullets) / 4) * 4)) + " bullets will take **" + Utility.FormatSecondsToTime(seconds_quad_bullets) +
								"** to drain.\nThis is **" + Utility.FormatSecondsToArkTime(minutes_quad_bullets) + "** Converted To Game Time.\n\n";
						}
						if (tek)
						{
							message += "**Tek Turrets -** - " + (bullets > 5000 ? 5000 : bullets) + " shards will take **" + Utility.FormatSecondsToTime(seconds_shards) +
								"** to drain.\nThis is **" + Utility.FormatSecondsToArkTime(minutes_shards) + "** Converted To Game Time.\n\n";
						}
					}
				}
			}
			else if (compare_id >= 7 && compare_id <= 9)
			{
				/*bool wood = (flags & (1L << ((byte)'w') - 95)) != 0;
				bool stone = (flags & (1L << ((byte)'s') - 95)) != 0;
				bool metal = (flags & (1L << ((byte)'m') - 95)) != 0;
				bool tek = (flags & (1L << ((byte)'t') - 95)) != 0;
				bool cave = (flags & (1L << ((byte)'c') - 95)) != 0;

				int damage = 
				if (compare_id == 7)*/
			}
			else if (compare_id == 10)
			{
				IReadOnlyCollection<SocketRole> roles = _client.GetGuild(610431252914503700).Roles;
				for (int i = 0; i < roles.Count; ++i)
				{
					SocketRole role = roles.ElementAt<SocketRole>(i);
				}
			}
			if (message.Length > 2000)
				message = message.Substring(0, 2000);

			return connection.Channel.SendMessageAsync(message);
		}
		catch (Exception e) {
			Console.Write("Command Failed: " + e.ToString());
		}

		return EmptyTask;
	}

	private Task Log(LogMessage msg)
	{
		Console.WriteLine(msg.ToString());
		return EmptyTask;
	}


	/*
	 * @Objective - Create a REAL hash compare method.
	 * -1 = Non Located Command.
	 *  1 = Experience Command.
	 */
	private int HashCompare(String check)
	{
		if (check.Length < 1)
			return -1;


		if (check.Contains("xp"))
			return 1;
		else if (check[0] == 'h')
			return 2;
		else if (check[0] == 'e' && (check[1] == 'n' || check[2] == 't' || check[1] == 't') && (check[3] == 'r' || check[4] == 'r'))
			return 3;
		else if (((check[0] == 'b' && check[1] == 'e') || (check[0] == 'b' && check[1] == 'r')))
			return 4;
		else if (check[0] == 'l')
			return 5;
		else if (check[0] == 't' && check[1] == 'u')
			return 6;
		else if (check[0] == 'c' && check[1] == '4')
			return 7;
		else if (check[0] == 'g' && check[1] == 'r' && check[2] == 'e')
			return 8;
		else if (check[1] == 'r' && check[1] == 'o' && check[3] == 'c')
			return 9;
		else if (check[0] == 'r' && check[2] == 'l')
			return 10;
		else if (check[0] == 'i' && (check[2] == 'p' || check[1] == 'p' || check[check.Length - 1] == 't'))
			return 11;
		else if (check[0] == 's' && (check[1] == 't' || check[2] == 't'))
			return 12;
		return -1;
	}

	/*
	 * Special Filter To Compare Messy Spelling To Actual Tame's.
	 */
	private int GetTameId(String check)
	{
		check += "     ";
		if (check[0] == 'g' && check[2] == 'g')
			return 0; //Giga
		if ((check[0] == 'o' && check[2] == 'l') || (check[0] == 's' && (check[1] == 'n' || check[3] == 'w')))
			return 1; //Owl.
		if (check[0] == 'm' && (check[2] == 'n' || check[1] == 'a'))
			return 2; //Mana.
		if (check[0] == 'p' && (check[4] == 'a' || check[1] == 't' || check[2] == 't'))
			return 3; //Ptera
		if (check[0] == 't' && (check[1] == 'h' || check[1] == 'r' || check[check.Length - 1] == 'i'))
			return 4; //Theri
		return -1;
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