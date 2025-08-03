using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevisionControl
{
    public enum RevisionAction
    {
        Add,
        Remove
    }

    public class ApplyRevisionExternalEventHandler : IExternalEventHandler
    {
        public Document Document { get; set; }
        public List<ElementId> SheetIds { get; set; }
        public ElementId RevisionId { get; set; }
        public string RevisionDescription { get; set; }
        public Action<string> ShowInfoAction { get; set; }
        public RevisionAction Action { get; set; } = RevisionAction.Add; // Default to Add

        public void Execute(UIApplication app)
        {
            try
            {
                if (ShowInfoAction == null)
                {
                    TaskDialog.Show("Revision Control", "ShowInfoAction is null! No popup can be shown.");
                    return;
                }

                if (Document == null || SheetIds == null || RevisionId == null)
                    throw new InvalidOperationException("Handler data is not set (Document, SheetIds, or RevisionId is null).");

                int count = 0;
                using (Transaction t = new Transaction(Document, (Action == RevisionAction.Add ? "Apply" : "Remove") + " Revision to Sheets"))
                {
                    t.Start();
                    foreach (var sheetId in SheetIds)
                    {
                        var viewSheet = Document.GetElement(sheetId) as ViewSheet;
                        if (viewSheet == null)
                        {
                            TaskDialog.Show("Revision Control", $"SheetId {sheetId.IntegerValue} is not a ViewSheet.");
                            continue;
                        }
                        var currRevIds = viewSheet.GetAdditionalRevisionIds().ToList();
                        if (Action == RevisionAction.Add)
                        {
                            if (!currRevIds.Contains(RevisionId))
                            {
                                currRevIds.Add(RevisionId);
                                viewSheet.SetAdditionalRevisionIds(currRevIds);
                                count++;
                            }
                        }
                        else // Remove
                        {
                            if (currRevIds.Contains(RevisionId))
                            {
                                currRevIds.Remove(RevisionId);
                                viewSheet.SetAdditionalRevisionIds(currRevIds);
                                count++;
                            }
                        }
                    }
                    t.Commit();
                }

                ShowInfoAction?.Invoke($"Revision '{RevisionDescription}' {(Action == RevisionAction.Add ? "applied to" : "removed from")} {count} sheet(s).");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Revision Control", $"Error: {ex.Message}\n{ex.StackTrace}");
                ShowInfoAction?.Invoke($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public string GetName() => "Apply/Remove Revision External Event";
    }
}
