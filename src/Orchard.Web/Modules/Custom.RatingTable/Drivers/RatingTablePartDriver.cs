using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Custom.RatingTable.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Custom.RatingTable.Drivers
{
    [OrchardFeature("Custom.RatingTable")]
    public class RatingTablePartDriver : ContentPartDriver<RatingTablePart>
    {
        protected override string Prefix
        {
            get { return "Custom.RatingTable.RatingTablePart"; }
        }

        protected override DriverResult Display(RatingTablePart part, string displayType, dynamic shapeHelper) 
        {
            if (displayType == "SummaryAdmin") 
            {
                return ContentShape("Parts_RatingTable_SummaryAdmin",
                                    () => 
                                    {
                                        var upperCaseDisplayType = displayType.ToUpperInvariant();
                                        return shapeHelper.Parts_RatingTable_SummaryAdmin(DisplayType: displayType, UpperDisplayType: upperCaseDisplayType);
                                    });
            }

            return ContentShape("Parts_RatingTable",
                                () => 
                                {
                                    var upperCaseDisplayType = displayType.ToUpperInvariant();
                                    return shapeHelper.Parts_RatingTable(DisplayType: displayType, UpperDisplayType: upperCaseDisplayType);
                                });
        }

        protected override DriverResult Editor(RatingTablePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_RatingTable_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/RatingTable",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(RatingTablePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

    }
}