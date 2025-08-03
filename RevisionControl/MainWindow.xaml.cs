using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MaterialDesignThemes.Wpf;
using RevisionControl;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel; // <-- Add this using directive
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevisionControl
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<SheetInfo> Sheets { get; set; }
        public ObservableCollection<RevisionComboBoxItem> AllRevisionItems { get; set; }

        private RevisionComboBoxItem _selectedRevisionItem;
        public RevisionComboBoxItem SelectedRevisionItem
        {
            get => _selectedRevisionItem;
            set
            {
                if (_selectedRevisionItem != value)
                {
                    _selectedRevisionItem = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(SelectedRevisionItem)));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private UIDocument _uiDoc;
        private Document _doc;
        private View _currentView;
        private System.Windows.Point _startPoint;
        private readonly WindowResizer _windowResizer;
        private ThemeManager _themeManager;
        private ApplyRevisionExternalEventHandler _applyRevisionHandler;
        private ExternalEvent _applyRevisionExternalEvent;

        // Add this field to your MainWindow class
        private bool _isClosing = false;

        public MainWindow(UIDocument uiDoc, Document doc, View currentView)
        {
            _themeManager = new ThemeManager(this);
            InitializeComponent();

            // Set window size and position after config is loaded
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Width = _themeManager.WindowWidth;
            this.Height = _themeManager.WindowHeight;
            this.Left = Math.Max(0, Math.Min(SystemParameters.VirtualScreenWidth - this.Width, _themeManager.WindowLeft));
            this.Top = Math.Max(0, Math.Min(SystemParameters.VirtualScreenHeight - this.Height, _themeManager.WindowTop));

            _applyRevisionHandler = new ApplyRevisionExternalEventHandler();
            this.Closed += (s, e) => _isClosing = true;

            // Assign ShowInfoAction inside the constructor body
            _applyRevisionHandler.ShowInfoAction = msg =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    RefreshSheets();
                    // Optionally, show a message:
                    // MessageBox.Show(this, msg, "Revision Control", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            };

            _applyRevisionExternalEvent = ExternalEvent.Create(_applyRevisionHandler);
            _uiDoc = uiDoc;
            _doc = doc;
            _currentView = currentView;

            // Register UI lifecycle events
            Loaded += MainWindow_Loaded; // Hook up the Loaded event
            this.Closed += MainWindow_Closed;
            this.PreviewMouseDown += MainWindow_PreviewMouseDown;
            _windowResizer = new WindowResizer(this);
            // Set the IgnoreElement after it's created
            _windowResizer.IgnoreElement = TitleBarControl;

            this.MouseLeftButtonUp += Window_MouseLeftButtonUp;
            this.MouseMove += Window_MouseMove;

            // Load saved theme and apply it
            ThemeToggleButton.IsChecked = _themeManager.IsDarkMode;

            AllRevisionItems = new ObservableCollection<RevisionComboBoxItem>(
                new FilteredElementCollector(doc)
                    .OfClass(typeof(Revision))
                    .Cast<Revision>()
                    .Select(r => new RevisionComboBoxItem { Revision = r })
            );


            Sheets = new ObservableCollection<SheetInfo>(
                new FilteredElementCollector(_doc)
                    .OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .Where(s => !s.IsPlaceholder)
                    .Select(s =>
                    {
                        // Get current revision number (parameter built-in: SHEET_CURRENT_REVISION)
                        string revNum = "";
                        var param = s.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION);
                        if (param != null && param.HasValue)
                            revNum = param.AsString();

                        return new SheetInfo
                        {
                            IsSelected = false,
                            SheetNumber = s.SheetNumber,
                            SheetName = s.Name,
                            SheetId = s.Id,
                            RevisionNumber = revNum
                        };
                    })
            );


            DataContext = this;
        }

        private void ApplyRevisionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRevisionItem == null)
            {
                MessageBox.Show("Palun vali versioon.", "Revision Control", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedSheets = Sheets.Where(s => s.IsSelected).ToList();
            if (!selectedSheets.Any())
            {
                MessageBox.Show("Palun vali vähemalt üks leht.", "Revision Control", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Set handler data
            _applyRevisionHandler.Document = _doc;
            _applyRevisionHandler.SheetIds = selectedSheets.Select(s => s.SheetId).ToList();
            _applyRevisionHandler.RevisionId = SelectedRevisionItem.Revision.Id;
            _applyRevisionHandler.RevisionDescription = SelectedRevisionItem.Revision.Description;

            // Raise the external event
            _applyRevisionExternalEvent.Raise();
            // Refresh SheetInfo after apply (simplest approach: re-populate collection)
        }

        private void RemoveRevisionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRevisionItem == null)
                return;
            var selectedSheets = Sheets.Where(s => s.IsSelected).ToList();
            if (!selectedSheets.Any())
                return;

            _applyRevisionHandler.Document = _doc;
            _applyRevisionHandler.SheetIds = selectedSheets.Select(s => s.SheetId).ToList();
            _applyRevisionHandler.RevisionId = SelectedRevisionItem.Revision.Id;
            _applyRevisionHandler.RevisionDescription = SelectedRevisionItem.Revision.Description;
            _applyRevisionHandler.Action = RevisionAction.Remove; // Set to remove

            _applyRevisionExternalEvent.Raise();
        }

        // Handles checkbox clicks, sets all selected rows to the new value
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox chk)) return;
            var row = SheetsDataGrid.SelectedItems;
            if (row == null || row.Count == 0) return;

            bool? value = chk.IsChecked;
            foreach (var item in SheetsDataGrid.SelectedItems.OfType<SheetInfo>())
                item.IsSelected = value == true;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            _themeManager.ApplyTheme(); // ✅ Apply the saved theme when the window loads

        }
        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            // ✅ Save size to config
            _themeManager.SaveConfig();
        }
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _themeManager.ToggleTheme();
            _themeManager.SaveConfig();
            ThemeToggleButton.IsChecked = _themeManager.IsDarkMode;
        }
        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject clickedElement = e.OriginalSource as DependencyObject;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = System.Windows.WindowState.Minimized;
        private void Window_MouseMove(object sender, MouseEventArgs e) => _windowResizer.ResizeWindow(e); // No need to skip here anymore
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => _windowResizer.StopResizing();
        private void LeftEdge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _windowResizer.StartResizing(e, ResizeDirection.Left);
        private void RightEdge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _windowResizer.StartResizing(e, ResizeDirection.Right);
        private void BottomEdge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _windowResizer.StartResizing(e, ResizeDirection.Bottom);
        private void BottomLeftCorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _windowResizer.StartResizing(e, ResizeDirection.BottomLeft);
        private void BottomRightCorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _windowResizer.StartResizing(e, ResizeDirection.BottomRight);

        // Add this method to update RevisionNumber for selected sheets in the UI
        private void ShowSelectedRevisionOnSheets()
        {
            if (SelectedRevisionItem == null) return;
            foreach (var sheet in Sheets.Where(s => s.IsSelected))
            {
                sheet.RevisionNumber = SelectedRevisionItem.Revision.Description; // or use another property, e.g., RevisionNumber
            }
        }

        // Call this method when the ComboBox selection changes
        private void RevisionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedRevisionOnSheets();
        }

        // 1. Add a method to refresh the Sheets collection
        private void RefreshSheets()
        {
            Sheets.Clear();
            foreach (var s in new FilteredElementCollector(_doc)
                                .OfClass(typeof(ViewSheet))
                                .Cast<ViewSheet>()
                                .Where(s => !s.IsPlaceholder))
            {
                string revNum = "";
                var param = s.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION);
                if (param != null && param.HasValue)
                    revNum = param.AsString();

                Sheets.Add(new SheetInfo
                {
                    IsSelected = false,
                    SheetNumber = s.SheetNumber,
                    SheetName = s.Name,
                    SheetId = s.Id,
                    RevisionNumber = revNum
                });
            }
        }
    }

}
