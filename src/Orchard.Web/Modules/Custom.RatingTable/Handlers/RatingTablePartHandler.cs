using System.Web.Routing;
using Custom.RatingTable.Models;
using Custom.RatingTable.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Extensions;

namespace Custom.RatingTable.Handlers
{
    [OrchardFeature("Custom.RatingTable")]
    public class RatingTablePartHandler : ContentHandler
    {
        public RatingTablePartHandler(
            IRepository<RatingTablePartRecord> ratingTablePartRepository, IRatingService ratingService)
        {
            Filters.Add(StorageFilter.For(ratingTablePartRepository));

            //OnUpdating<RatingTablePart>(Updating);

            //OnActivated<RatingTablePart>((context, part) => part.RatingsField.Loader(() => ratingService.GetRatingsForPart(part.Record.Id)));
        }

        private void Updating(UpdateContentContext context, RatingTablePart part) 
        {
            
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "RatingTable") return;

            context.Metadata.EditorRouteValues = new RouteValueDictionary
                                                    {
                                                        {"area", "Custom.RatingTable"},
                                                        {"controller", "RatingTableAdmin"},
                                                        {"action", "GetRatingTableEditor"},
                                                        {"id", context.ContentItem.Id}
                                                    };
        }
    }
}