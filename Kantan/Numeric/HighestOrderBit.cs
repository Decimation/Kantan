static internal class HighestOrderBit
{
	public static int hob(int num)
	{
		if (!(num > 0))
			return 0;

		int ret = 1;

		while ((num >>= 1) > 0)
			ret <<= 1;

		return ret;
	}
}