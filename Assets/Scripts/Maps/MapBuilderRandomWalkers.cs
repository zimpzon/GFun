using UnityEngine;

public static class MapBuilderRandomWalkers
{
    public static void Build(int w, int h)
    {
        var param = new WalkParam(MapBuilder.MapMaxWidth / 2, MapBuilder.MapMaxHeight / 2, w, h);

        for (int i = 0; i < 5; ++i)
        {
            param.steps = 60;
            DoWalk(param, MapBuilder.MapSource);
        }
    }

    struct WalkParam
    {
        public WalkParam(int x, int y, int w, int h)
        {
            this.w = w;
            this.h = h;
            this.x = x;
            this.y = y;
            steps = 50;
            minBeforeTurn = 2;
            maxBeforeTurn = 5;
            xMin = x - w / 2;
            xMax = x + w / 2;
            yMin = y - h / 2;
            yMax = y + h / 2;
        }

        public int x;
        public int y;
        public int w;
        public int h;
        public int steps;
        public int minBeforeTurn;
        public int maxBeforeTurn;
        public int xMin;
        public int xMax;
        public int yMin;
        public int yMax;
    }

    static (int dirX, int dirY) GetDirection()
    {
        int val = Random.Range(0, 4);
        if (val == 0)
            return (0, -1);
        if (val == 1)
            return (1, 0);
        if (val == 2)
            return (0, 1);
        if (val == 3)
            return (-1, 0);

        throw new System.Exception("Out of range");
    }

    static void DoWalk(WalkParam param, byte[,] map)
    {
        while (param.steps > 0)
        {
            (int dirX, int dirY) = GetDirection();
            int len = Random.Range(param.minBeforeTurn, param.maxBeforeTurn);
            for (int i = 0; i < len; ++i)
            {
                map[param.x + 0, param.y + 0] = 1;
                map[param.x + 1, param.y + 0] = 1;
                map[param.x + 0, param.y + 1] = 1;
                map[param.x + 1, param.y + 1] = 1;

                if (param.x > param.xMin && param.x < param.xMax - 2)
                    param.x += dirX;
                else
                    dirX = -dirX;

                if (param.y > param.yMin && param.y < param.yMax - 2)
                    param.y += dirY;
                else
                    dirY = -dirY;

                param.steps--;
            }
        }
    }
}
