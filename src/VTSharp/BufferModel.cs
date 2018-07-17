using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTSharp
{
    internal class BufferModel
    {
        private int width;
        private int height;
        private int viewStartLine = 0;
        private string BackgroundColor = "Transparent";
        private string Color = "Black";

        public BufferModel(int width, int height)
        {
            this.Lines = new List<Line>();
            this.Viewable = new ObservableCollection<Line>();
        }


        private List<Line> Lines
        {
            get;
            set;
        }

        public ObservableCollection<Line> Viewable
        {
            get;
            set;
        }

        public void AddLine(Line line)
        {
            this.Lines.Add(line);
            RecalculateViewable();
        }

        public void Scroll(int startLine)
        {
            if (this.viewStartLine != startLine)
            {
                this.viewStartLine = startLine;
                RecalculateViewable();
            }
        }

        public void SetDimensions(int width, int height)
        {
            if (this.width != width
                || this.height != height)
            {
                this.width = width;
                this.height = height;
                RecalculateLines();
                RecalculateViewable();
            }
        }

        private void RecalculateLines()
        {
            var watch = new Stopwatch();
            watch.Start();
            var newLines = new List<Line>();

            Line lineAppend = null;
            foreach(var line in this.Lines)
            {
                if (lineAppend != null)
                {
                    line.Prepend(lineAppend);
                    lineAppend = null;
                }
                if (line.Length > this.width)
                {
                    var lines = line.SplitAt(this.width - 1);
                    newLines.Add(lines[0]);
                    if (lines[1].BreakType == BreakType.Wrapped)
                    {
                        lineAppend = lines[1];
                    }
                    else
                    {
                        newLines.Add(lines[1]);
                    }
                }
                else if (line.Length < this.width && line.BreakType == BreakType.Wrapped)
                {
                    lineAppend = line;
                }
                else
                {
                    newLines.Add(line);
                }
            }

            this.Lines = newLines;
            Trace.WriteLine(watch.Elapsed);
        }

        private void RecalculateViewable()
        {
            var watch = new Stopwatch();
            watch.Start();

            var newView = new ObservableCollection<Line>();
            for (int i = 0; i < this.height && i < this.Lines.Count; i++)
            {
                newView.Add(this.Lines[i + this.viewStartLine]);
            }
            
            if (!newView.SequenceEqual(this.Viewable))
            {
                this.Viewable.Clear();
                foreach(var line in newView)
                {
                    this.Viewable.Add(line);
                }
            }

            Trace.WriteLine(watch.Elapsed);
        }
    }

    internal class Sequence
    {
        public string BackgroundColor
        {
            get;
            set;
        } = "Transparent";

        public string Color
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            var sequence = obj as Sequence;
            return sequence != null &&
                   BackgroundColor == sequence.BackgroundColor &&
                   Color == sequence.Color &&
                   Text == sequence.Text;
        }

        public override int GetHashCode()
        {
            var hashCode = -49602234;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BackgroundColor);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Color);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            return hashCode;
        }

        public void Prepend(Sequence sequence)
        {
            this.Text = string.Concat(sequence.Text, this.Text);
        }

        public Sequence[] SplitAt(int location)
        {
            var before = new Sequence
            {
                BackgroundColor = this.BackgroundColor,
                Color = this.Color,
                Text = this.Text.Substring(0, location + 1),
            };
            var after = new Sequence
            {
                BackgroundColor = this.BackgroundColor,
                Color = this.Color,
                Text = this.Text.Substring(location + 1),
            };
            return new[] { before, after };
        }
    }

    internal class Line
    {
        public List<Sequence> Sequences
        {
            get;
            set;
        }

        public BreakType BreakType
        {
            get;
            set;
        }

        public int Length => this.Sequences.Select(sequence => sequence.Text.Length).Sum();

        public override bool Equals(object obj)
        {
            var line = obj as Line;
            return line != null &&
                   EqualityComparer<List<Sequence>>.Default.Equals(Sequences, line.Sequences) &&
                   BreakType == line.BreakType;
        }

        public override int GetHashCode()
        {
            var hashCode = -571092039;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Sequence>>.Default.GetHashCode(Sequences);
            hashCode = hashCode * -1521134295 + BreakType.GetHashCode();
            return hashCode;
        }

        public void Prepend(Line line)
        {
            if (line.Sequences.Last().BackgroundColor == this.Sequences.First().BackgroundColor
                && line.Sequences.Last().Color == this.Sequences.First().Color)
            {
                this.Sequences.First().Prepend(line.Sequences.Last());
                this.Sequences = line.Sequences.Take(line.Sequences.Count - 1).Concat(this.Sequences).ToList();
            }
            else
            {
                this.Sequences = line.Sequences.Concat(this.Sequences).ToList();
            }
        }

        public Line[] SplitAt(int location)
        {
            int i;
            for(i = 0; i < this.Sequences.Count; i++)
            {
                if (this.Sequences[i].Text.Length > location)
                {
                    break;
                }
                location -= this.Sequences[i].Text.Length;
            }

            if (i > this.Sequences.Count)
            {
                return null;
            }

            var sequences = this.Sequences[i].SplitAt(location);
            var before = new Line
            {
                BreakType = BreakType.Wrapped,
                Sequences = this.Sequences.Take(i).Concat(new[] { sequences[0] }).Where(sequence => sequence.Text.Length > 0).ToList()
            };
            var after = new Line
            {
                BreakType = this.BreakType,
                Sequences = new[] { sequences[1] }.Concat(this.Sequences.Skip(i + 1)).Where(sequence => sequence.Text.Length > 0).ToList()
            };

            return new[] { before, after };
        }
    }

    internal enum BreakType
    {
        Real,
        Wrapped
    }
}
