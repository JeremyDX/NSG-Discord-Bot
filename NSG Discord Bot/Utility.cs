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


public class Utility
{

	public static Int64 last_time_stamp = 15271402510L;

	public static long polled_members = 0; //Supports 64 bools.

	public static readonly string[] MONTHS =
	{
		"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
	};

	public static readonly short[] CRYOSICK_FOOD_LOSS =
	{
		//Gig   //Owl	//Mana	//Pt	//Theri
		-1,		3000,	-1,		-1,		-1
	};

	public static readonly long HERBIVORE = 1 << 5; //Theri

	public static readonly short[] STARVE_VALUES =
	{
		//1		//2		//3		//4		//5
		//Gig   //Owl	//Mana	//Pt	//Theri
		175,	480,	448,	475,	163
	};

	public static readonly short[] MAX_MEAT_TO_JUVY =
	{
		//Gig   //Owl	//Mana	//Pt	//Theri
		-1,		-1,		221,	78,		425
	};

	public static readonly double[] GROWTH_PER_SECOND =
	{
		//Gig		//Owl			//Mana		//Pt		//Theri
		0,          0.001025,		0,          0.00150,		0.00048
	};
	


	public static readonly string[] TAMES =
	{
		"Giganotosaurus", "Snow Owl", "Managarmr", "Pteranodon", "Therizinosaur"
	};

	public static readonly int[] PLAYER_TOTAL_XP =
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

	public static int SecureParseInt32(ref String number)
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

	public static long SecureParseInt64(ref String number)
	{
		long value = -1;

		try
		{
			value = Int64.Parse(number);
		}
		catch (Exception)
		{

		}
		return value;
	}

	public static double SecureParseDouble(ref String number)
	{
		double value = -1;

		try
		{
			value = Double.Parse(number);
		}
		catch (Exception)
		{

		}
		return value;
	}

	public static String FormatSecondsToTime(int time)
	{
		int minutes = time / 60;
		int seconds = time - (minutes * 60);
		if (minutes > 0)
		{
			return minutes + "mins " + seconds + "secs";
		}
		else
		{
			return seconds + "secs";
		}
	}

	public static String FormatSecondsToArkTime(int time)
	{
		int minutes = time / 60;
		int seconds = time - (minutes * 60);
		if (minutes > 0)
		{
			return minutes + "hours " + seconds + "mins";
		}
		else
		{
			return seconds + "mins";
		}
	}

	private static ulong[] REDUCTION = new ulong[]
	{
		1,
		1,
		10,
		100,
		1000,
		10000,
		100000,
		1000000,
		10000000, //8
		100000000,
		1000000000,
		10000000000,
		100000000000,
		1000000000000,
		10000000000000,
		100000000000000,
		1000000000000000,
		10000000000000000, //17
		100000000000000000, //18
		1000000000000000000, //19
	};

	public static char[] Integer64ToCharArray(long num)
	{
		char[] buffer = new char[22];
		ulong value = (ulong)num;
		bool neg = num < 0;
		if (neg)
			value = (ulong)-num;
		int len = 0;
		if (value >= 1000000000) //10 Digit.
		{
			if (value >= 100000000000000) //15 Digit.
			{
				if (value >= 10000000000000000) //17 Digits.
				{
					if (value >= 1000000000000000000)
						len = 19;
					else
					{
						if (value >= 100000000000000000)
							len = 18;
						else
							len = 17;
					}
				}
				else
				{
					if (value >= 1000000000000000)
						len = 16;
					else
					{
						len = 15;
					}
				}
			}
			else
			{
				if (value >= 100000000000) // 12 Digits.
				{
					if (value >= 10000000000000) //14 Digits.
						len = 14;
					else
					{
						if (value >= 1000000000000) //13 Digits.
							len = 13;
						else
							len = 12; //12 Digits.
					}
				}
				else
				{
					if (value >= 10000000000)
						len = 11; //11 Digits.
					else
					{
						len = 10; //10 Digits.
					}
				}
			}
		}
		else
		{
			if (value >= 100000) //6 Digit.
			{
				if (value >= 10000000)
				{
					if (value >= 100000000)
						len = 9;
					else
						len = 8;
				}
				else
				{
					if (value >= 1000000)
						len = 7;
					else
						len = 6;
				}
			}
			else
			{
				if (value >= 1000)
				{
					if (value >= 10000)
						len = 5;
					else
						len = 4;
				}
				else
				{
					if (value >= 100)
						len = 3;
					else
					{
						if (value >= 10)
							len = 2;
						else
							len = 1;
					}
				}
			}
		}

		int offset = -1;

		if (neg)
			buffer[++offset] = '-';

		ulong reduction = REDUCTION[len];

		for (; --len >= 0;)
		{
			int c_value = (int)(value / reduction);
			value -= (reduction * (ulong)c_value);
			buffer[++offset] = (char)(48 + c_value);
			reduction = REDUCTION[len];
		}

		buffer[++offset] = (char) 0;

		return buffer;
	}

	public static string FormatCharIntToDecimal(long var, int size)
	{
		char[] value = Integer64ToCharArray(var);
		int trim = -1;
		bool check = false;
		for (int i = value.Length - 1; i >= 0; --i)
		{
			if (value[i] == 0 && i > 0)
			{
				if (value[i - 1] != 0)
				{
					check = true;
				}
			}
			if (check && size >= 0 && (i + 1) < value.Length)
			{
				value[i + 1] = value[i];
				if (--size == -1)
				{
					trim = i + 2;
					value[i] = '.';
					break;
				}
			}
		}
		return new string(value, 0, trim >= 0 ? trim : 0);
	}

	public static String ConvertSecondsToTime(long time)
	{
		StringBuilder sb = new StringBuilder(8);
		long hours = time / 3600;
		long minutes = (time - (hours * 3600)) / 60;
		long seconds = (int)(time - (hours * 3600) - (minutes * 60));
		long days = 0;
		if (hours > 24)
		{
			days = hours / 24;
			hours -= days * 24;
			sb.Append(days);
			sb.Append(':');
		}
		if (hours < 10)
			sb.Append('0');
		sb.Append(hours);
		sb.Append(':');
		if (minutes < 10)
			sb.Append('0');
		sb.Append(minutes);
		sb.Append(':');
		if (seconds < 10)
			sb.Append('0');
		sb.Append(seconds);
		return sb.ToString();
	}
}

