using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mabiavalon.Docking
{
    public class Finder
    {
        internal static LocationReport Find(DockControl dockControl)
        {
            if (dockControl == null) throw new ArgumentNullException("dockControl");

            var locationReportBuilder = new LocationReportBuilder(dockControl);

            foreach (var loadedInstance in Layout.GetLoadedInstances())
            {
                locationReportBuilder.CurrentLayout = loadedInstance;

                loadedInstance.Query().Visit(
                    locationReportBuilder,
                    BranchVisitor,
                    DockControlVisitor
                    );

                if (locationReportBuilder.IsFound)
                    break;
            }

            if (!locationReportBuilder.IsFound)
                throw new LocationReportException("Instance not within any layout.");

            return locationReportBuilder.ToLocationReport();
        }

        private static void BranchVisitor(LocationReportBuilder locationReportBuilder, BranchAccessor branchAccessor)
        {
            if (Equals(branchAccessor.FirstDockControl, locationReportBuilder.TargetDockControl))
                locationReportBuilder.MarkFound(branchAccessor.Branch, false);
            else if (Equals(branchAccessor.SecondDockControl, locationReportBuilder.TargetDockControl))
                locationReportBuilder.MarkFound(branchAccessor.Branch, true);
            else
            {
                branchAccessor.Visit(BranchItem.First, ba => BranchVisitor(locationReportBuilder, ba));
                if (locationReportBuilder.IsFound) return;
                branchAccessor.Visit(BranchItem.Second, ba => BranchVisitor(locationReportBuilder, ba));
            }
        }

        private static void DockControlVisitor(LocationReportBuilder locationReportBuilder, DockControl dockControl)
        {
            if (Equals(dockControl, locationReportBuilder.TargetDockControl))
                locationReportBuilder.MarkFound();
        }
    }
}
