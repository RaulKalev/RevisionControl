using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisionControl
{
    public class RevisionComboBoxItem
    {
        public Revision Revision { get; set; }
        public string DisplayText => $"{Revision.RevisionNumber} - {Revision.Description}";
    }

}
