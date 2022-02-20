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
            Console.Write("cmd: ");
            string val = Console.ReadLine() ?? "";
            if (val.Equals("help")) Console.WriteLine("RESET - Place pieces back to original places{0}MOVE - Move piece by ID to coordinates.{0}BOARD - Show game board{0}PIECES - List all pieces on board", System.Environment.NewLine);
            else if (val.Equals("reset")) m.SetupBoard();
            else if (val.Split(" ")[0].Equals("move"))
            {
                char test = Convert.ToChar(val.Split(" ")[2][0].ToString().ToLower());
                int found = Array.IndexOf(m.letters, test);
                if (!m.MovePiece(Convert.ToInt32(val.Split(" ")[1]), new Point() { X = found, Y = val.Split(" ")[2][1] - '0' }))
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
        int row = 0;
        Console.Write("  A B C D E F G H\n  ---------------\n1");
        for (int i = 1; i <= 64; i++)
        {
            Pieces p = m.pieces.Find(tmp => tmp.coords.Equals(new Point { X = i - (8 * row), Y = row + 1 }));
            Console.Write("|{0}", p.id <= 0 ? " " : m.types[p.type][0] );
            if (i % 8 == 0) { row++; Console.Write("|{0}  ---------------{0}", System.Environment.NewLine); if (i != 64) Console.Write("{0}", i / 8 + 1); }
        }
        Console.WriteLine("");
    }

    void ShowPiecesList()
    {
        foreach (Pieces p in m.pieces)
        {
            Console.WriteLine("{0}-{1}[{2}]:{3}", p.side == 0 ? "White" : "Black", m.types[p.type], p.id, String.Concat(m.letters[p.coords.X], p.coords.Y));
        }
    }

    void ShowHistory()
    {
        foreach (History h in m.history)
        {
            Pieces p = m.pieces.Find(tmp => tmp.id == h.PieceID);
            Console.WriteLine("[{5}] {0} {1}[{2}] from {3} to {4} - Duration: {6}", p.side == 0 ? "White" : "Black", m.types[p.type], p.id, String.Concat(m.letters[h.OldPosition.X], h.OldPosition.Y), String.Concat(m.letters[h.NewPosition.X], h.NewPosition.Y), h.EndTime, h.StartTime - h.EndTime);
        }
    }
}