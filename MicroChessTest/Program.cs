using MicroChessLib;

class Chess
{
    public MCL m = new MCL();

    static void Main(string[] args)
    {
        Chess c = new Chess();
        c.CommandProcessing();
    }

    void CommandProcessing()
    {
        m.SetupBoard();
        while (true)
        {
            Console.Write("[{0}] cmd: ", m.player == 0 ? "white" : "black");
            string val = Console.ReadLine() ?? "";
            if (val.Equals("help")) Console.WriteLine("RESET - Place pieces back to original places{0}MOVE - Move piece by ID to coordinates.{0}BOARD - Show game board{0}PIECES - List all pieces on board", System.Environment.NewLine);
            else if (val.Equals("reset")) m.SetupBoard();
            else if (val.Split(" ")[0].Equals("move"))
            {
                char test = Convert.ToChar(val.Split(" ")[2][0].ToString().ToLower());
                int found = Array.IndexOf(m.letters, test);
                if (!m.MovePiece(m.pieces.Find(o => o.id == Convert.ToInt32(val.Split(" ")[1])), new Point() { X = found, Y = val.Split(" ")[2][1] - '0' }))
                    Console.WriteLine("move was invalid");
                else { m.player = (m.player + 1) % 2; Console.WriteLine("Move was successful at {1}. {0} side's turn.", m.player == 0 ? "white" : "black", DateTime.Now); }
            }
            else if (val.Equals("pieces")) ShowPiecesList();
            else if (val.Equals("board")) ShowBoard();
            else if (val.Equals("history")) ShowHistory();
        }
    }

    void ShowBoard()
    {
        Console.Write("   A   B   C   D   E   F   G   H\n  -------------------------------\n");
        for (int r = 8; r >= 1; r--)
        {
            string BoardLine = r.ToString();
            for (int s = (r * 8); s > ((r * 8) - 8); s--)
            {
                Pieces p = m.pieces.Find(tmp => tmp.coords.Equals(new Point { X = ((8 * r) - s) + 1, Y = r }));
                if (p.id > 0) BoardLine += ("|" + String.Concat(m.types[p.type][0], p.id.ToString("D2"))); else BoardLine += "|   ";
            }

            Console.Write("{0}|{1}\n  -------------------------------\n", BoardLine, r);
        }
        Console.WriteLine("   A   B   C   D   E   F   G   H\n");
    }

    void ShowPiecesList()
    {
        foreach (Pieces p in m.pieces)
        {
            Console.WriteLine("[{0}] {1}[{2}]:{3}", p.side == 0 ? "W" : "B", m.types[p.type], p.id, String.Concat(m.letters[p.coords.X], p.coords.Y));
        }
    }

    void ShowHistory()
    {
        foreach (History h in m.history)
        {
            Console.WriteLine("[{5}] {0} {1}[{2}] from {3} to {4} - Duration: {6}", h.Piece.side == 0 ? "White" : "Black", m.types[h.Piece.type], h.Piece.id, String.Concat(m.letters[h.OldPosition.X], h.OldPosition.Y), String.Concat(m.letters[h.NewPosition.X], h.NewPosition.Y), h.EndTime, h.StartTime - h.EndTime);
        }
    }
}