DateTime ptime;
Actions CurrentAction = Actions.Nothing;
Console.OutputEncoding = System.Text.Encoding.Unicode;
var t = new Thread(KeyboardInput);
t.Start();
Console.BackgroundColor = ConsoleColor.Blue;
Console.Clear();
Console.CursorVisible = false;


int w = Console.BufferWidth;
int h = Console.WindowHeight;
int pos = w/2;

// Clear the console window
Console.SetCursorPosition(0, 0);
for (int i = 0; i < h - 1; i++) {
    Console.WriteLine(new String(' ', w));
}
Console.SetCursorPosition(0, 0);

while (true) {
    ptime = DateTime.Now;

    if (CurrentAction != Actions.Nothing) {
        var cpos = Console.GetCursorPosition();
        Console.CursorTop = h - 5;
        ClearConsoleLine(w);
        Console.CursorLeft = w / 2 - 5;
        Console.Write($"""Moving {(CurrentAction == Actions.MoveLeft ? "Left" : "Right")}""");

        if (CurrentAction == Actions.MoveLeft) {
            pos = Math.Max(pos - 2, 0);
        } else if (CurrentAction == Actions.MoveRight) {
            pos = Math.Min(pos + 2, w - 2);
        }

        CurrentAction = Actions.Nothing;
    }

    Console.CursorTop = h - 3;
    ClearConsoleLine(w);
    Console.CursorLeft = pos;
    Console.ForegroundColor = ConsoleColor.DarkRed;
    Console.Write("\u2586\u2586"); // unicode 0x2586 -> "lower three quarters block"

    Console.ForegroundColor = ConsoleColor.Black;

    Thread.Sleep(Math.Max(0, 16 - DateTime.Now.Subtract(ptime).Milliseconds));
}

void KeyboardInput() {
    ConsoleKeyInfo k;
    while (true) {
        k = Console.ReadKey();
        if (k.Key == ConsoleKey.LeftArrow) {
            CurrentAction = Actions.MoveLeft;
        } else if (k.Key == ConsoleKey.RightArrow) {
            CurrentAction = Actions.MoveRight;
        }
    }

}

void ClearConsoleLine(int w) {
    Console.CursorLeft = 0;
    Console.Write(new string(' ', w));
    Console.CursorLeft = 0;
}

enum Actions
{
    Nothing = 0,
    MoveLeft = 1,
    MoveRight = 2
}