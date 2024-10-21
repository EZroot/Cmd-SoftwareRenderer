using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
namespace Console3DRenderer
{
    class Program
    {
        struct Vector3
        {
            public double X;
            public double Y;
            public double Z;
            public Vector3(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
            public Vector3 RotateX(double angle)
            {
                double rad = angle * Math.PI / 180.0;
                double cosa = Math.Cos(rad);
                double sina = Math.Sin(rad);
                double y = Y * cosa - Z * sina;
                double z = Y * sina + Z * cosa;
                return new Vector3(X, y, z);
            }
            public Vector3 RotateY(double angle)
            {
                double rad = angle * Math.PI / 180.0;
                double cosa = Math.Cos(rad);
                double sina = Math.Sin(rad);
                double z = Z * cosa - X * sina;
                double x = Z * sina + X * cosa;
                return new Vector3(x, Y, z);
            }
            public Vector3 RotateZ(double angle)
            {
                double rad = angle * Math.PI / 180.0;
                double cosa = Math.Cos(rad);
                double sina = Math.Sin(rad);
                double x = X * cosa - Y * sina;
                double y = X * sina + Y * cosa;
                return new Vector3(x, y, Z);
            }
            public (int, int) Project(double screenWidth, double screenHeight, double fov, double viewerDistance)
            {
                double factor = fov / (viewerDistance + Z);
                double x = X * factor + screenWidth / 2.0;
                double y = -Y * factor + screenHeight / 2.0;
                return ((int)x, (int)y);
            }
        }
        static int[,] edges = new int[,]
        {
            {0,1}, {1,3}, {3,2}, {2,0}, 
            {4,5}, {5,7}, {7,6}, {6,4}, 
            {0,4}, {1,5}, {2,6}, {3,7}  
        };
        static void Main(string[] args)
        {
            int desiredWidth = 140;  
            int desiredHeight = 140;  

            Console.CursorVisible = false;
            int screenWidth = Console.WindowWidth;
            int screenHeight = Console.WindowHeight;
            double aspectRatio = 0.5; 
            screenWidth = (int)(screenWidth * aspectRatio);
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-1, 1, -1),
                new Vector3(1, 1, -1),
                new Vector3(-1, -1, -1),
                new Vector3(1, -1, -1),
                new Vector3(-1, 1, 1),
                new Vector3(1, 1, 1),
                new Vector3(-1, -1, 1),
                new Vector3(1, -1, 1)
            };
            double angleX = 0;
            double angleY = 0;
            double angleZ = 0;
            double viewerDistance = 35.0;
            double fov = 256.0;
            int targetFPS = 30;
            int frameTime = 1000 / targetFPS;
            DisplayInstructions();
            while (true)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                HandleInput(ref viewerDistance);
                char[,] screen = new char[screenHeight, screenWidth];
                for (int y = 0; y < screenHeight; y++)
                    for (int x = 0; x < screenWidth; x++)
                        screen[y, x] = ' ';
                Vector3[] transformed = new Vector3[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 v = vertices[i];
                    v = v.RotateX(angleX);
                    v = v.RotateY(angleY);
                    v = v.RotateZ(angleZ);
                    transformed[i] = v;
                }
                (int, int)[] projected = new (int, int)[transformed.Length];
                for (int i = 0; i < transformed.Length; i++)
                {
                    projected[i] = transformed[i].Project(screenWidth, screenHeight, fov, viewerDistance);
                }
                for (int i = 0; i < edges.GetLength(0); i++)
                {
                    int start = edges[i, 0];
                    int end = edges[i, 1];
                    DrawLine(screen, projected[start].Item1, projected[start].Item2,
                             projected[end].Item1, projected[end].Item2, '*');
                }
                StringBuilder sb = new StringBuilder(screenHeight * (screenWidth + 1));
                for (int y = 0; y < screenHeight; y++)
                {
                    for (int x = 0; x < screenWidth; x++)
                        sb.Append(screen[y, x]);
                    sb.AppendLine();
                }
                Console.SetCursorPosition(0, 0);
                Console.Write(sb.ToString());
                DisplayZoomLevel(viewerDistance, screenHeight, screenWidth);
                angleX += 1;
                angleY += 1;
                angleZ += 1;
                stopwatch.Stop();
                int elapsed = (int)stopwatch.ElapsedMilliseconds;
                int sleep = frameTime - elapsed;
                if (sleep > 0)
                    Thread.Sleep(sleep);
            }
        }
        static void HandleInput(ref double viewerDistance)
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.OemPlus || keyInfo.Key == ConsoleKey.Add)
                {
                    viewerDistance -= 0.5;
                    if (viewerDistance < 1.0)
                        viewerDistance = 1.0; 
                }
                else if (keyInfo.Key == ConsoleKey.OemMinus || keyInfo.Key == ConsoleKey.Subtract)
                {
                    viewerDistance += 0.5;
                    if (viewerDistance > 100.0)
                        viewerDistance = 100.0; 
                }
            }
        }
        static void DisplayZoomLevel(double viewerDistance, int screenHeight, int screenWidth)
        {
            string zoomText = $"Zoom Level (Viewer Distance): {viewerDistance:F1}";
            if (zoomText.Length > screenWidth)
                zoomText = zoomText.Substring(0, screenWidth);
            Console.SetCursorPosition(0, screenHeight - 1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(zoomText.PadRight(screenWidth));
            Console.ResetColor();
        }
        static void DisplayInstructions()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("3D Console Renderer - Zoom Controls");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Press '+' to Zoom In");
            Console.WriteLine("Press '-' to Zoom Out");
            Console.WriteLine("Press 'Esc' to Exit");
            Console.ResetColor();
            Console.WriteLine("Rendering will start in 3 seconds...");
            Thread.Sleep(3000);
            Console.Clear();
        }
        static void DrawLine(char[,] screen, int x0, int y0, int x1, int y1, char ch)
        {
            int width = screen.GetLength(1);
            int height = screen.GetLength(0);
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                    screen[y0, x0] = ch;
                if (x0 == x1 && y0 == y1)
                    break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}
