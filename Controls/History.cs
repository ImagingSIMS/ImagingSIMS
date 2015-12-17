using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Controls
{
    public interface IHistory
    {
        bool CanUndo();
        bool CanRedo();

        void Undo();
        void Redo();
    }
}
