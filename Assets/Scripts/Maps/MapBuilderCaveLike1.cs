﻿using UnityEngine;

public static class MapBuilderCaveLike1
{
    public static void Build(int w, int h)
    {
        var mapHandler = new CaveBuilder1(w, h, 0.45f);
        mapHandler.MakeCaverns();
        mapHandler.InvertMap();
        mapHandler.CopyTo(MapBuilder.MapSource, MapBuilder.MapMaxWidth / 2, MapBuilder.MapMaxHeight / 2);
    }

    // PWE: Copied from somewhere, I didn't write it.
    public class CaveBuilder1
    {
        public byte[,] Map;
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public float WallPct { get; set; }

        public CaveBuilder1(int w, int h, float wallPct)
        {
            Map = new byte[w, h];

            MapWidth = w;
            MapHeight = h;
            WallPct = wallPct;

            RandomFillMap();
        }

        public void CopyTo(byte[,] dst, int centerX, int centerY)
        {
            int baseX = centerX - MapWidth / 2;
            int baseY = centerY - MapHeight / 2;
            for (int y = 0; y < MapHeight; ++y)
            {
                for (int x = 0; x < MapWidth; ++x)
                {
                    byte val = Map[x, y];
                    dst[baseX + x, baseY + y] = Map[x, y];
                }
            }
        }

        public void InvertMap()
        {
            for (int y = 0; y < MapHeight; ++y)
            {
                for (int x = 0; x < MapWidth; ++x)
                {
                    byte val = Map[x, y];
                    Map[x, y] = (byte)(val == 0 ? 1 : 0);
                }
            }
        }

        public void MakeCaverns()
        {
            // By initilizing column in the outter loop, its only created ONCE
            for (int column = 0, row = 0; row <= MapHeight - 1; row++)
            {
                for (column = 0; column <= MapWidth - 1; column++)
                {
                    Map[column, row] = PlaceWallLogic(column, row);
                }
            }
        }

        public byte PlaceWallLogic(int x, int y)
        {
            int numWalls = GetAdjacentWalls(x, y, 1, 1);

            if (Map[x, y] == 1)
            {
                if (numWalls >= 4)
                {
                    return 1;
                }
                if (numWalls < 2)
                {
                    return 0;
                }
            }
            else
            {
                if (numWalls >= 5)
                {
                    return 1;
                }
            }
            return 0;
        }

        public int GetAdjacentWalls(int x, int y, int scopeX, int scopeY)
        {
            int startX = x - scopeX;
            int startY = y - scopeY;
            int endX = x + scopeX;
            int endY = y + scopeY;

            int iX = startX;
            int iY = startY;

            int wallCounter = 0;

            for (iY = startY; iY <= endY; iY++)
            {
                for (iX = startX; iX <= endX; iX++)
                {
                    if (!(iX == x && iY == y))
                    {
                        if (IsWall(iX, iY))
                        {
                            wallCounter += 1;
                        }
                    }
                }
            }
            return wallCounter;
        }

        bool IsWall(int x, int y)
        {
            // Consider out-of-bound a wall
            if (IsOutOfBounds(x, y))
            {
                return true;
            }

            if (Map[x, y] == 1)
            {
                return true;
            }

            if (Map[x, y] == 0)
            {
                return false;
            }
            return false;
        }

        bool IsOutOfBounds(int x, int y)
        {
            if (x < 0 || y < 0)
            {
                return true;
            }
            else if (x > MapWidth - 1 || y > MapHeight - 1)
            {
                return true;
            }
            return false;
        }

        public void BlankMap()
        {
            for (int column = 0, row = 0; row < MapHeight; row++)
            {
                for (column = 0; column < MapWidth; column++)
                {
                    Map[column, row] = 0;
                }
            }
        }

        public void RandomFillMap()
        {
            int mapMiddle = 0; // Temp variable
            for (int column = 0, row = 0; row < MapHeight; row++)
            {
                for (column = 0; column < MapWidth; column++)
                {
                    // If coordinants lie on the the edge of the map (creates a border)
                    if (column == 0)
                    {
                        Map[column, row] = 1;
                    }
                    else if (row == 0)
                    {
                        Map[column, row] = 1;
                    }
                    else if (column == MapWidth - 1)
                    {
                        Map[column, row] = 1;
                    }
                    else if (row == MapHeight - 1)
                    {
                        Map[column, row] = 1;
                    }
                    // Else, fill with a wall a random percent of the time
                    else
                    {
                        mapMiddle = (MapHeight / 2);

                        if (row == mapMiddle)
                        {
                            Map[column, row] = 0;
                        }
                        else
                        {
                            Map[column, row] = RandomPercent(WallPct);
                        }
                    }
                }
            }
        }

        byte RandomPercent(float pct)
            => (byte)(Random.value < pct ? 1 : 0);
    }
}
