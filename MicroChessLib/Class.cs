/// <summary>
/// Created by Jacob Fliss
/// </summary>

namespace MicroChessLib;

public struct Point
{
    public int X;
    public int Y;
}

public struct Pieces
{
    public int id { get; set; }
    public int type { get; set; }
    public Point coords { get; set; }
    public int side { get; set; }
    public bool dead { get; set; }
}

public struct History
{
    public Pieces Piece { get; set; }
    public Point OldPosition { get; set; }
    public Point NewPosition { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class MCL
{
    public List<Pieces> pieces = new List<Pieces>();
    public List<History> history = new List<History>();
    public char[] letters = new char[] { ' ', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
    public string[] types = new string[] { "pawn", "rook", "knight", "bishop", "Queen", "King" };
    public int player = 0; // keep track of whos turn it is. 0 = white, bottom of screen
    public DateTime MoveStartTime = DateTime.Now;

    public Pieces AddGamePiece(int id, int type, Point coords, int side, bool dead = false)
    {
        return new Pieces() { id = id, type = type, coords = new Point() { X = coords.X, Y = coords.Y }, side = side, dead = dead };
    }

    public bool DetectCollisionPath(Point StartPosition, Point EndPosition, Point PieceToCheck)
    {
        // store differences in start->end coords
        int XDiff = Math.Abs(StartPosition.X - EndPosition.X);
        int YDiff = Math.Abs(StartPosition.Y - EndPosition.Y);
        int LargestDiff = XDiff > YDiff ? XDiff : YDiff; // get the largest to see how far we iterate
        for (int i = 0; i < LargestDiff; i++) // check if along our path we would land on another piece on both neg/pos coords
        {
            if (PieceToCheck.Y == (YDiff != 0 ? EndPosition.Y < 0 ? StartPosition.Y - i : StartPosition.Y + i : StartPosition.Y) &&
                PieceToCheck.X == (XDiff != 0 ? EndPosition.X < 0 ? StartPosition.X - i : StartPosition.X + i : StartPosition.X))
            {
                Console.WriteLine("Collision detected at {0}{1}", letters[PieceToCheck.X], PieceToCheck.Y);
                return true;
            }
        }
        return false;
    }

    // TO-DO: finish and test
    public bool PotentialForCheck() // check if either king would be in check
    {
        if (history.Count <= 0) return false; // no one moved yet
        foreach (Pieces p in pieces.FindAll(o => !o.dead)) // check each piece and every space
        {
            int row = 0;
            for (int i = 1; i <= 64; i++)
            {
                int attack = 0;
                if (MoveValidity(p, new Point { X = i - (8 * row), Y = row + 1 }, out attack)) // check if potential move is valid, would be an attack and on a king
                {
                    if (attack > 0)
                    {
                        Pieces tmp = pieces.Find(o => o.id == attack);
                        if (tmp.type == 5 && tmp.side == player)
                        { // we're about to put ourselves in check
                            Console.WriteLine("Potential for check from {2} at {0}{1}", tmp.coords.X, tmp.coords.Y, types[tmp.type]);
                            return true;
                        }
                    }
                }
                if (i % 8 == 0) { row++; }
            }
        }
        return false;
    }

    // TO-DO: test
    public bool AttackOtherPiece(Pieces MyPiece)
    {
        foreach (Pieces tmp in pieces)
        {
            if (!tmp.dead && tmp.side != player && tmp.coords.X == MyPiece.coords.X && tmp.coords.Y == MyPiece.coords.Y) // check if landing on another piece
            {
                Pieces temp = pieces.Find(tmp => tmp.id == MyPiece.id);
                temp.dead = true;
                pieces[tmp.id] = temp;
                if (temp.type == 5)
                {
                    Console.WriteLine("The king has died. {0} side won! Game over.", player == 0 ? "white" : "black");
                }
                return true; // should only land on 1 piece
            }
        }
        return false;
    }

    public bool MoveValidity(Pieces p, Point coords, out int AttackID)
    {
        bool valid = false;
        AttackID = 0;
        History NewHistory = new History() { Piece = p, StartTime = MoveStartTime, EndTime = DateTime.Now, OldPosition = p.coords, NewPosition = coords };

        if (coords.X < 1 || coords.X > 8 || coords.Y < 1 || coords.Y > 8) // dont go beyond board
            return false;

        switch (p.type) // check if move is valid before other checks
        {
            case 0: // pawn - only y axis 1 place, first move can be 2 places, attacks are diagonal
                if ((player == 0 && p.coords.Y <= coords.Y + (history.FindIndex(o => o.Piece.id == p.id) > 0 ? 1 : 2) && coords.X == p.coords.X) ||
                    (player == 1 && p.coords.Y >= coords.Y - (history.FindIndex(o => o.Piece.id == p.id) > 0 ? 1 : 2) && coords.X == p.coords.X))
                    valid = true;
                if (((coords.X == (p.coords.X - 1) && coords.Y == (p.coords.Y - 1)) && player == 0 && pieces.FindIndex(o => o.coords.Equals(coords) && o.side != player) > 0) ||
                    ((coords.X == (p.coords.X + 1) && coords.Y == (p.coords.Y + 1)) && player == 0 && pieces.FindIndex(o => o.coords.Equals(coords) && o.side != player) > 0) ||
                    ((coords.X == (p.coords.X - 1) && coords.Y == (p.coords.Y + 1)) && player == 1 && pieces.FindIndex(o => o.coords.Equals(coords) && o.side != player) > 0) ||
                    ((coords.X == (p.coords.X + 1) && coords.Y == (p.coords.Y - 1)) && player == 1 && pieces.FindIndex(o => o.coords.Equals(coords) && o.side != player) > 0))
                { // attack
                    AttackID = pieces.Find(o => o.coords.Equals(coords)).id;
                    return true;
                }
                break;
            case 1: // rooks - straight lines
                if ((coords.X == p.coords.X && coords.Y != p.coords.Y) || (coords.X != p.coords.X && coords.Y == p.coords.Y))
                    valid = true;
                break;
            case 2: // knights - L shape, ignores path collisions
                if ((coords.X - p.coords.X == 2 && coords.Y - p.coords.Y == 1) || (coords.X - p.coords.X == 1 && coords.Y - p.coords.Y == 2))
                {
                    foreach (Pieces tmp in pieces)
                    {
                        if (!tmp.dead && tmp.side == player && tmp.coords.X == p.coords.X && tmp.coords.Y == p.coords.Y) // check if landing on another piece
                            return false; // can't land on our own piece
                        else if (!tmp.dead && tmp.side != player && tmp.coords.X == p.coords.X && tmp.coords.Y == p.coords.Y)
                            AttackID = pieces.Find(o => o.coords.Equals(coords)).id;
                    }
                    return true; // ignore path collisions
                }
                break;
            case 3: // bishops - diagonals only
                if (Math.Abs(coords.X - p.coords.X) == Math.Abs(coords.Y - p.coords.Y))
                    valid = true;
                break;
            case 4: // queens - inf places, diagonals or straight lines
                if ((coords.X == p.coords.X && coords.Y != p.coords.Y) || (coords.X != p.coords.X && coords.Y == p.coords.Y) || (Math.Abs(coords.X - p.coords.X) == Math.Abs(coords.Y - p.coords.Y)))
                    valid = true;
                break;
            case 5: // kings - 1 place, diagonals or straight lines
                if (Math.Abs(coords.X - p.coords.X) > 1 || Math.Abs(coords.Y - p.coords.Y) > 1)
                    return false; // king is going beyond what he's allowed, immediately return false
                if ((coords.X == p.coords.X && coords.Y != p.coords.Y) || (coords.X != p.coords.X && coords.Y == p.coords.Y) || (Math.Abs(coords.X - p.coords.X) == Math.Abs(coords.Y - p.coords.Y)))
                    valid = true;
                break;
            default:
                break;
        }

        if (valid && p.type != 2) // check for collisions, except knights, only if piece move was valid
        {
            foreach (Pieces tmp in pieces)
            {
                if (tmp.dead || tmp.id == p.id) continue; // ignore our piece and dead pieces
                if (DetectCollisionPath(new Point() { X = p.coords.X, Y = p.coords.Y }, new Point() { X = coords.X, Y = coords.Y }, new Point() { X = tmp.coords.X, Y = tmp.coords.Y }))
                {
                    if (tmp.coords.Equals(coords) && tmp.side != player)
                    { // move was valid, technically we collided, but our final path was on top of other player's piece, attack it
                        AttackID = pieces.Find(o => o.coords.Equals(coords)).id;
                        return true;
                    }
                    return false; // we collided with a piece on our way to our destination
                }
            }
            return true;
        }
        return false;
    }

    public bool MovePiece(Pieces p, Point coords)
    {
        Console.WriteLine("Moving \"{2}\"[{0}] piece from {3} to {1}...", p.id, String.Concat(letters[coords.X], coords.Y), types[p.type], String.Concat(letters[p.coords.X], p.coords.Y));
        
        if (pieces.FindIndex(piece => piece.type == 5 && piece.dead) > 0) // king is dead
            return false;

        int IsAttack = 0; // piece ID we are attacking, if greater than 0
        if (MoveValidity(p, coords, out IsAttack)) { // validate we can move in space
            if (PotentialForCheck()) return false; // we can move there, but we would check ourselves
            if (IsAttack > 0) {
                if (AttackOtherPiece(p)) {
                    Pieces Att = pieces.Find(o => o.id == IsAttack); Console.WriteLine("Attacking {0} at {0}{1}", types[Att.type], Att.coords.X, Att.coords.Y);
                } } // attack other pieces if we happen to land on them
            history.Add(new History() { Piece = p, StartTime = MoveStartTime, EndTime = DateTime.Now, OldPosition = p.coords, NewPosition = coords }); // success, add move history
            return true;
        }
        return false;
    }

    public void SetupBoard()
    {
        pieces.Clear();
        for (int x = 1; x <= 32; x++)
        {
            if (x <= 16) pieces.Add(AddGamePiece(x, 0, new Point() { X = x > 8 ? (x - 8) : x, Y = x > 8 ? 7 : 2 }, x > 8 ? 1 : 0)); // pawns
            else if (x > 16 && x <= 20) pieces.Add(AddGamePiece(x, 1, new Point() { X = x % 2 == 0 ? 8 : 1, Y = x >= 19 ? 8 : 1 }, x >= 19 ? 1 : 0)); // rooks
            else if (x > 20 && x <= 24) pieces.Add(AddGamePiece(x, 2, new Point() { X = x % 2 == 0 ? 7 : 2, Y = x >= 23 ? 8 : 1 }, x >= 23 ? 1 : 0)); // knights
            else if (x > 24 && x <= 28) pieces.Add(AddGamePiece(x, 3, new Point() { X = x % 2 == 0 ? 6 : 3, Y = x >= 27 ? 8 : 1 }, x >= 27 ? 1 : 0)); // bishops
            else if (x > 28 && x <= 30) pieces.Add(AddGamePiece(x, 4, new Point() { X = 4, Y = x >= 30 ? 8 : 1 }, x >= 30 ? 1 : 0)); // queens
            else if (x > 30 && x <= 32) pieces.Add(AddGamePiece(x, 5, new Point() { X = 5, Y = x >= 32 ? 8 : 1 }, x >= 32 ? 1 : 0)); // kings
        }
        Console.WriteLine("Pieces are now placed on board. Game is starting at {0}", DateTime.Now);
    }
}

