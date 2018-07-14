using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VTSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random;
        Stopwatch stopwatch;
        long frameCounter = 0;

        public MainWindow()
        {
            InitializeComponent();

            this.random = new Random();

            new Typeface("Consolas").TryGetGlyphTypeface(out var typeface);
            this.terminalLines.DataContext = new RenderModel
            {
                FontSize = 12,
                FontUri = typeface.FontUri.AbsolutePath,
                OriginY = typeface.Baseline * 12,
                Buffer = new BufferModel()
            };

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (this.stopwatch == null)
                this.stopwatch = Stopwatch.StartNew();

            ++this.frameCounter;

            if (this.terminalLines.DataContext is RenderModel model)
            {
                var one = GenerateString(30);
                var two = GenerateString(30);
                var three = GenerateString(1);
                var four = GenerateString(1);
                var five = GenerateString(30);
                model.Buffer.Lines.Add(new Line {
                    Sequences = new List<Sequence> {
                        new Sequence { Color = "Black", Text = one},
                        new Sequence { Color = "Red", Text = two},
                        new Sequence { BackgroundColor = "Purple", Color = "Green", Text = three},
                        new Sequence { BackgroundColor = "Black", Color = "Yellow", Text = four},
                        new Sequence { BackgroundColor = "Red", Color = "Cyan", Text = five}
                    }
                });
                model.Buffer.Lines.Add(new Line
                {
                    Sequences = new List<Sequence> {
                        new Sequence { Color = "Black", Text = $"{one}{two}{three}{four}{five}"},
                    }
                });
            }

            this.terminalScrollback.ScrollToBottom();
        }

        private string GenerateString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz1234567890";
            return new string(Enumerable.Repeat(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }

    internal class RenderModel
    {
        public string FontUri
        {
            get;
            set;
        }

        public int FontSize
        {
            get;
            set;
        }

        public double OriginY
        {
            get;
            set;
        }

        public BufferModel Buffer
        {
            get;
            set;
        }
    }
}
