public class AsyncPumpProcessingLatency
{
	public long Current;

	public long Max;

	public long Sum;

	public long Count;

	public double Avg
	{
		get
		{
			double result;
			if (Count > 0)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				result = (double)Sum / (double)Count;
			}
			else
			{
				result = 0.0;
			}
			return result;
		}
	}

	public AsyncPumpProcessingLatency()
	{
		Reset();
	}

	public void Update(long ticks)
	{
		Current = ticks;
		if (Max < ticks)
		{
			while (true)
			{
				switch (5)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			Max = ticks;
		}
		Sum += ticks;
		Count++;
	}

	public void Reset()
	{
		Max = long.MinValue;
		Sum = 0L;
		Count = 0L;
	}
}
