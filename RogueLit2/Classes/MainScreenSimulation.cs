using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLit2.Classes {
    internal class MainScreenSimulation : IDisposable {
        private Random rnd = new();
        private int Width;
        private int Height;
        private AnimationCell[] Cells;
        private int AnimationCycleSpeed;
        private bool Shutdown = false;
        private Task Cycletask;
        private bool[] Mask;

        internal MainScreenSimulation(int width, int height, int animationCycleSpeed) {
            Width = width;
            Height = height;
            AnimationCycleSpeed = animationCycleSpeed;
            Cells = new AnimationCell[width * height];

            Image img = Image.FromFile(Directory.GetCurrentDirectory() + @"\Assets\Play.bmp");
            Bitmap b = new(img);
            Mask = new bool[(width * 2) * height];
            for (int y = 0; y < b.Height; y++) {
                for (int x = 0; x < b.Width; x++) {
                    float br = b.GetPixel(x, y).GetBrightness();
                    Mask[x + (y * Width)] = br == 1;
                }
            }
        }

        internal void Start() {
            Cells = Cells.Select(x => { x = new(rnd.Next(190, 250), rnd.Next(3, 6), "▓▓"); return x; }).ToArray();

            Cycletask = new(() => {
                while (!Shutdown) {
                    for (int i = 0; i < 50; i++) {
                        Animate();
                        GenerateNextCells();
                        Thread.Sleep(AnimationCycleSpeed);
                    }

                    // Invert rise direction
                    for (int i = 0; i < Cells.Length; i++)
                        Cells[i].Rise ^= true;
                }
                Shutdown = false;
            });
            Cycletask.Start();
        }


        private void Animate() {
            string markUp = "";
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < Cells.Length; i++) {
                string rgb = GetRGBValue(Mask[i] ? Cells[i].Value : 255 - Cells[i].Value);
                markUp += $"[#{rgb}]{Cells[i].Icon}[/]" + ((i + 1) % Width == 0 ? "\n" : "");
            }
            if (Shutdown) return;
            AnsiConsole.Markup(markUp);
        }

        private string GetRGBValue(float cellValue) =>
            ((int)cellValue).ToString("x2") +
            ((int)(cellValue / 7f)).ToString("x2") +
            ((int)(cellValue / 8f)).ToString("x2");


        private void GenerateNextCells() {
            for (int i = 0; i < Cells.Length; i++) {
                Cells[i].Value += (Cells[i].Rise ? 1 : -1) * Cells[i].AccelRate;
                Cells[i].AccelRate += ((float)rnd.NextDouble() / 2f) * (rnd.Next(2) == 0 ? 1 : -1);
            }
        }


        // Dispose of the controller
        public void Dispose() {
            Shutdown = true;

            // Wait for shutdown
            while (Shutdown)
                continue;
        }
    }
}
