public static class XpCalc
{
    public static int GetLevelAtXp(int xp)
    {
        int sum = 0;
        for (int i = 1; i < 500; ++i)
        {
            if (sum > xp)
                return i - 1;

            sum += GetXpRequired(i);
        }
        return 500;
    }

    public static int GetXpRequired(int level)
    {
       return level < 1 ? 0 : 200 + (level - 1) * 50;
    }

    public static int GetTotalXpRequired(int level)
    {
        int result = 0;
        for (int i = 1; i < level; ++i)
            result += GetXpRequired(i);
        return result;
    }
}
