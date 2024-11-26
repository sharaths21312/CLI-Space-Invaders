using System.Text;

DateTime ptime;
const ConsoleColor bgcol = ConsoleColor.DarkBlue;
var Character = new Player();
List<Projectile> projectiles = [];

Console.OutputEncoding = System.Text.Encoding.Unicode;
var t = new Thread(KeyboardInput);
t.Start();
Console.BackgroundColor = bgcol;
Console.Clear();
Console.CursorVisible = false;


int w = Console.BufferWidth;
int h = Console.WindowHeight;
int pos = w/2;

// Clear the console window

while (true) {
    ptime = DateTime.Now;
    Console.CursorTop = Character.vpos;
    ClearConsole(w);
    Character.Draw();
    Thread.Sleep(Math.Max(0, 16 - DateTime.Now.Subtract(ptime).Milliseconds));
}

void KeyboardInput() {
    ConsoleKeyInfo k;
    while (true) {
        k = Console.ReadKey();
        if (k.Key == ConsoleKey.LeftArrow) {
            Character.MoveLeft();
        } else if (k.Key == ConsoleKey.RightArrow) {
            Character.MoveRight();
        }
    }

}

void ClearConsole(int w) {
    Console.SetCursorPosition(0, 0);
    Console.BackgroundColor = bgcol;
    for (int i = 0; i < h - 1; i++) {
        Console.WriteLine(new String(' ', w));
    }
    Console.SetCursorPosition(0, 0);
}

class Projectile
{
    public int vpos;
    public int hpos;
    public bool deleted;

    public Projectile(int hp, int vp) {
        vpos = vp;
        hpos = hp;
        deleted = false;
    }

    public void Move() {
        vpos--;
        if (vpos <= 0) {
            Delete();
        }
    }
    public void Draw() {
        if (deleted) return;

        Console.CursorLeft = hpos;
        Console.CursorTop = vpos;

        Console.Write("\u275a");
    }

    public void Delete() {
        deleted = true;
    }
}


class Player
{
    public readonly int vpos = Console.BufferHeight - 3;
    public readonly int ReloadMS = 250;
    public readonly ConsoleColor bgcol = ConsoleColor.DarkBlue;
    public readonly ConsoleColor fgcol = ConsoleColor.Magenta;
    public readonly ConsoleColor projcol = ConsoleColor.Yellow;
    public readonly string DrawingChars = new('\u2586', 3); // "lower three quarters block"
    public System.Timers.Timer shootTimer;
    public List<Projectile> Projectiles = [];
    public int hpos;

    public Player() {
        hpos = Console.BufferWidth/2;
        shootTimer = new(ReloadMS);
        shootTimer.Elapsed += Shoot;
        shootTimer.Start();
    }

    public void MoveLeft() {
        hpos = Math.Max(hpos - 3, 0);
    }
    public void MoveRight() {
        hpos = Math.Min(hpos + 3, Console.BufferWidth - 3);
    }

    public void Draw() {
        Console.CursorTop = vpos;
        Console.CursorLeft = hpos;
        Console.BackgroundColor = bgcol;
        Console.ForegroundColor = fgcol;
        Console.Write(DrawingChars);
        Console.ForegroundColor = projcol;
        lock (Projectiles) {
            foreach (var p in Projectiles) {
                p.Draw();
            }
        }
    }

    public void Shoot(Object? source, System.Timers.ElapsedEventArgs e) {
        lock (Projectiles) {
            foreach (var p in Projectiles) {
                p.Move();
            }
            Projectiles.RemoveAll(p => p.deleted);
            Projectiles.Add(new Projectile(hpos + 1, vpos - 1));
        }
    }
}