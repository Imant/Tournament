using Custom.Tournament.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Custom.Tournament.Drivers
{
    [OrchardFeature("Custom.Tournament")]
    public class RatingTablePartDriver : ContentPartDriver<RatingTablePart>
    {
        protected override string Prefix
        {
            get { return "Custom.Tournament.RatingTablePart"; }
        }

        protected override DriverResult Display(RatingTablePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_RatingTable", () => shapeHelper.Parts_RatingTable());
        }
    }
}