using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ricaun.Revit.UI;
using System;

namespace RevisionControl
{
    [AppLoader]
    public class App : IExternalApplication
    {
        private RibbonPanel ribbonPanel;

        public Result OnStartup(UIControlledApplication application)
        {
            const string tabName = "RK Tools";
            const string panelName = "Tools";

            try { application.CreateRibbonTab(tabName); } catch { }
            ribbonPanel = application.CreateOrSelectPanel(tabName, panelName);

            // Create the buttons
            ribbonPanel.CreatePushButton<Command>("Versiooni\nNr.")
                .SetLargeImage("Resources/RevisionControl.tiff")
                .SetToolTip("Set revision nr.")
                .SetContextualHelp("https://raulkalev.github.io/rktools/");


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            ribbonPanel?.Remove();
            return Result.Succeeded;
        }
    }
}
