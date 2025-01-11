using System;
using System.Collections.Generic;
public static class Utilities
{
	 public static float GetMedian (List<float> data)
	{
		// data.Sort();
		// DATA SHOULD ALREADY BE SORTED (DESC or ASC)
		int len = data.Count;
        if (len % 2 == 0) 
		{ 
			return (data[(len / 2) - 1] + data[len / 2]) / 2; 
		} 
		else 
		{ 
			return  data[(len - 1) / 2]; 
		}
    }

	public static float GetE1RMax(float reps, float weight)
	{
		if (1 <= reps && reps < 7.614) 
		{ 
			return weight * 36 / (37 - reps); 
		}
		else 
		{ 
			return weight * Convert.ToSingle(Math.Pow(reps, 0.1)); 
		}
		// 1RM = w * (36/(37-r)) is the Brzyki Formula. It is more accurate for 1 <= r < 7.614
		// 1RM = w * r^0.1 is the Lombardi Formula. It is more accurate for r < 1 U r >= 7.614
		// 7.614 and 1 are the intersections between the graphs, chosen as these ranges match my research.
	}

	public static DateTime GetLastMonday()
	{
		int dayOfWeek = Convert.ToInt16(DateTime.Today.DayOfWeek);
		if (dayOfWeek == 0) // Sun (0) to Sat (6)
		{
			return DateTime.Today.AddDays(-6);
		}
		return DateTime.Today.AddDays(1 - dayOfWeek);
	}

}
