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
                typeface = typeface,
                FontSize = 12,
                FontUri = typeface.FontUri.AbsolutePath,
                OriginY = typeface.Baseline * 12,
                Buffer = new BufferModel(100, 0)
            };
            for (int i = 0; i < 100; i++)
            {
                CompositionTarget_Rendering(null, null);
            }
            this.scrollBar.Minimum = 0;
            this.scrollBar.Maximum = 99;
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
                model.Buffer.AddLine(new Line {
                    Sequences = new List<Sequence> {
                        new Sequence { Color = "Black", Text = one},
                        new Sequence { Color = "Red", Text = two},
                        new Sequence { BackgroundColor = "Purple", Color = "Green", Text = three},
                        new Sequence { BackgroundColor = "Black", Color = "Yellow", Text = four},
                        new Sequence { BackgroundColor = "Red", Color = "Cyan", Text = five}
                    }
                });
                model.Buffer.AddLine(new Line
                {
                    Sequences = new List<Sequence> {
                        new Sequence { Color = "Black", Text = $"{one}{two}{three}{four}{five}"},
                    }
                });
            }
        }

        private string GenerateString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz1234567890";
            return new string(Enumerable.Repeat(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.terminalLines.DataContext is RenderModel renderModel)
            {
                var charHeight = renderModel.typeface.Height * 12;
                var maxLines = (int)(this.terminalLines.ActualHeight / charHeight);
                if (renderModel.typeface.CharacterToGlyphMap.TryGetValue('a', out var index))
                {
                    var charWidth = renderModel.typeface.AdvanceWidths[index] * 12;
                    var maxCols = (int)(this.viewPortCol.ActualWidth / charWidth);
                    renderModel.Buffer.SetDimensions(maxCols, maxLines);

                    this.scrollBar.ViewportSize = maxLines;

                };
            }
        }

        private void scrollBar_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            if (this.terminalLines.DataContext is RenderModel renderModel)
            {
                renderModel.Buffer.Scroll((int)e.NewValue);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.terminalLines.DataContext is RenderModel renderModel)
            {
                this.scrollBar.Value -= e.Delta / 10;
                renderModel.Buffer.Scroll((int)this.scrollBar.Value);
            }
            
        }
    }

    internal class RenderModel
    {
        public GlyphTypeface typeface
        {
            get;
            set;
        }

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
