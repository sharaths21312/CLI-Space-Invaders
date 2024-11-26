using System.Text;
using System.Runtime.InteropServices;
using System.Reflection.PortableExecutable;
// https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences

DateTime ptime;
var Character = new Player();
List<Projectile> projectiles = [];
var enemy = new Enemies();
Buffer writebuffer = new();

Console.OutputEncoding = Encoding.Unicode;

var t = new Thread(KeyboardInput);
t.Start();
Console.CursorVisible = false;

int w = Console.BufferWidth;
int h = Console.WindowHeight;
int pos = w/2;

while (true) {
    ptime = DateTime.Now;
    Character.Shoot();
    enemy.Tick();
    enemy.Draw(writebuffer);
    Character.Draw(writebuffer);
    writebuffer.Flush();
    int n = Math.Max(0, 25 - DateTime.Now.Subtract(ptime).Milliseconds);
    Thread.Sleep(Math.Max(0, 25 - DateTime.Now.Subtract(ptime).Milliseconds));
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
    public void Draw(Buffer buffer) {
        if (deleted) return;
        buffer.SetPos(hpos, vpos);
        buffer.Write("\u275a");
    }

    public void Delete() {
        deleted = true;
    }
}


class Player
{
    public readonly int vpos = Console.BufferHeight - 1;
    public readonly int ReloadMS = 250;
    public readonly SpecialColors bgcol = SpecialColors.Blue;
    public readonly SpecialColors fgcol = SpecialColors.Magenta;
    public readonly SpecialColors projcol = SpecialColors.Yellow;
    public readonly string DrawingChars = new('\u2586', 3); // "lower three quarters block"
    public DateTime shootTimer;
    public List<Projectile> Projectiles = [];
    public int hpos;

    public Player() {
        hpos = Console.BufferWidth/2;
        shootTimer = DateTime.Now;
    }

    public void MoveLeft() {
        hpos = Math.Max(hpos - 3, 0);
    }
    public void MoveRight() {
        hpos = Math.Min(hpos + 3, Console.BufferWidth - 3);
    }

    public void Draw(Buffer buffer) {
        buffer.SetPos(hpos, vpos);
        buffer.FGCol(projcol);
        buffer.BGCol(bgcol);
        foreach (var p in Projectiles) {
            p.Draw(buffer);
        }

        buffer.Write(DrawingChars);
    }

    public void Shoot() {
        if (DateTime.Now.Subtract(shootTimer).TotalMilliseconds < ReloadMS) return;

        foreach (var p in Projectiles) {
            p.Move();
        }
        Projectiles.RemoveAll(p => p.deleted);
        Projectiles.Add(new Projectile(hpos + 1, vpos - 1));
    }
}

class Enemies
{
    public readonly string enemyChars = new('\u2588', 3); // 'full block'
    public readonly SpecialColors EnemyColor = SpecialColors.Green;
    public readonly Random rng = new((int) DateTime.Now.Ticks);
    public readonly int spawnTimeMS = 2000;
    public DateTime internalTick;

    public List<Tile> OccupiedTiles;

    public Enemies() {
        OccupiedTiles = [];
        internalTick = DateTime.Now;
    }

    public void Tick() {
        if (DateTime.Now.Subtract(internalTick).TotalMilliseconds < spawnTimeMS) { return; }
        internalTick = DateTime.Now;
        foreach (var item in OccupiedTiles) {
            item.ypos++;
            if (item.ypos > Console.BufferHeight - 5) {
                OccupiedTiles.Remove(item);
            }
        }

        for (int i = 0; i < Console.BufferWidth - 2; i+=3) {
            if (rng.NextDouble() < 0.2) {
                OccupiedTiles.Add(new Tile(i, 1));
            }
        }
    }
    
    public void Draw(Buffer buffer) {
        buffer.FGCol(EnemyColor);
        foreach (var item in OccupiedTiles) {
            buffer.SetPos(item.xpos, item.ypos);
            buffer.Write(enemyChars);
        }
    }
}

class Buffer
{
    StringBuilder sb;
    public Buffer() {
        sb = new();
    }
    
    // The coordinates on the console are 1-indexed, I convert them here
    public void SetY(int pos) => sb.Append($"\x1b[{pos+1}d");
    public void SetX(int pos) => sb.Append($"\x1b[{pos+1}G");
    public void SetPos(int x, int y) => sb.Append($"\x1b[{y+1};{x+1}H");
    public void FGCol(SpecialColors color, bool isBright = false) => sb.Append($"\x1b[{(isBright ? 9 : 3)}{(int) color}m");
    public void BGCol(SpecialColors color, bool isBright = false) => sb.Append($"\x1b[{(isBright ? 10 : 4)}{(int)color}m");
    public void Write(string str) => sb.Append(str);
    public void Write(char chr) => sb.Append(chr);
    public void Flush(){
        Console.SetCursorPosition(0, 0);
        Console.Write(this.ToString());
        sb.Clear();
        ClearConsole();
    }
    public void ClearConsole() {
        SetPos(0, 0);
        for (int i = 0; i < Console.BufferHeight - 1; i++) {
            sb.AppendLine(new string(' ', Console.BufferWidth));
        }
    }

    public override string ToString() {
        return sb.ToString();
    }
}


record Tile(int xpos, int ypos)
{
    public int xpos = xpos;
    public int ypos = ypos;
}
record struct RGBCol(int R, int G, int B);

// The virtual console escape sequences are not the same as the ConsoleColor enum
enum SpecialColors
{
    Black = 0,
    Red = 1,
    Green = 2,
    Yellow = 3,
    Blue = 4,
    Magenta = 5,
    Cyan = 6,
    White = 7,
    Default = 9
}