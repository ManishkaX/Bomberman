using System.Drawing;

namespace Server;
public static class Map
{
    public enum CellTypes { SolidBlock, SoftBlock, Empty }

    private const int Width = 18;

    private const int Height = 10;

    public static CellTypes[,] MapTemplate = new CellTypes[Height, Width];

    public static List<Point> SoftBlocks = new List<Point>();


    public static void GenerateMap()
    {
        Random random = new Random();

        for (var row = 0; row < Height; row++)
        {
            for (var col = 0; col < Width; col++)
            {
                MapTemplate[row, col] = CellTypes.Empty;

                if (col == 0 || row == 0 || row == Height || col == Height)
                {
                    MapTemplate[row, col] = CellTypes.SolidBlock;
                    continue;
                }

                if (row % 2 == 0 && col % 2 == 0)
                {
                    MapTemplate[row, col] = CellTypes.SolidBlock;
                    continue;
                }

                if ((row == 1 && col == 1) || (row == 2 && col == 1) || (row == 1 && col == 2) ||
                    (row == Height - 1 && col == 1) || (row == Height - 2 && col == 1) || (row == Height - 1 && col == 2) ||
                    (row == 1 && col == Width - 1) || (row == 1 && col == Width - 2) || (row == 2 && col == Width - 1) ||
                    (row == Height - 1 && col == Width - 1) || (row == Height - 2 && col == Width - 1) || (row == Height - 1 && col == Width - 2))
                {
                    MapTemplate[row, col] = CellTypes.Empty;
                    continue;
                }

                if (random.Next(1, 100) < 60)
                {
                    MapTemplate[row, col] = CellTypes.SoftBlock;
                    SoftBlocks.Add(new Point(row, col));
                }


            }
        }
    }
}