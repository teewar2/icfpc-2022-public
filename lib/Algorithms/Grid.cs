using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class Grid
{
    public Grid(List<GridRow> rows)
    {
        Rows = rows;
    }

    public List<GridRow> Rows;

    public Grid Copy()
    {
        return new Grid(Rows.Select(x => x.Copy()).ToList());
    }

    public override string ToString()
    {
        return Rows.StrJoin("\n");
    }
}

public class GridRow
{
    public GridRow(int height, List<GridCell> cells)
    {
        Height = height;
        Cells = cells;
    }

    public int Height;
    public List<GridCell> Cells;

    public GridRow Copy()
    {
        return new GridRow(Height, Cells.Select(x => x.Copy()).ToList());
    }

    public override string ToString()
    {
        return $"Height: {Height}, Cells: [{Cells.StrJoin(", ")}]";
    }
}

public class GridCell
{
    public GridCell(int width)
    {
        Width = width;
    }

    public int Width;

    public GridCell Copy()
    {
        return new GridCell(Width);
    }

    public override string ToString()
    {
        return Width.ToString();
    }
}
