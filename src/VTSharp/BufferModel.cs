using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTSharp
{
    internal class BufferModel
    {
        public BufferModel()
        {
            this.Lines = new ObservableCollection<Line>();
        }

        public ObservableCollection<Line> Lines
        {
            get;
            set;
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
    }

    internal class Line
    {
        public List<Sequence> Sequences
        {
            get;
            set;
        }
    }
}
