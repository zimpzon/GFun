using UnityEngine;

public static class MapBuilderRandomWalkers
{
    public static void Build(int w, int h)
    {
        var param = new WalkParam(w, h);

        for (int i = 0; i < 5; ++i)
        {
            param.steps = 60;
            DoWalk(param, MapBuilder.map);
        }
    }

    struct WalkParam
    {
        public WalkParam(int w, int h)
        {
            this.w = w;
            this.h = h;
            x = w / 2;
            y = h / 2;
            steps = 50;
            minBeforeTurn = 2;
            maxBeforeTurn = 5;
        }

        public int x;
        public int y;
        public int w;
        public int h;
        public int steps;
        public int minBeforeTurn;
        public int maxBeforeTurn;
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

                if (param.x > 0 && param.x < param.w - 2)
                    param.x += dirX;
                else
                    dirX = -dirX;

                if (param.y > 0 && param.y < param.h - 2)
                    param.y += dirY;
                else
                    dirY = -dirY;

                param.steps--;
            }
        }
    }
}
