using Autodesk.Revit.DB;

namespace RevisionControl
{
    public class SheetInfo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected))); }
        }

        public string SheetNumber { get; set; }
        public string SheetName { get; set; }
        public string RevisionNumber { get; set; }  // <-- Must be present!
        public ElementId SheetId { get; set; }
    }
}
