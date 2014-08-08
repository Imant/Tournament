using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Custom.RatingTable.Models;
using Custom.RatingTable.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Custom.RatingTable.Controllers
{
    [OrchardFeature("Custom.RatingTable")]
    [Admin]
    public class RatingTableAdminController:Controller, IUpdateModel
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;

        public Localizer T { get; set; }

        public RatingTableAdminController(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
            _authorizer = orchardServices.Authorizer;
            _contentManager = orchardServices.ContentManager;
            _transactionManager = orchardServices.TransactionManager;

            T = NullLocalizer.Instance;
        }

        public ActionResult GetRatingTableList(){
            var ratingTables = _contentManager
                .Query(VersionOptions.Latest, "RatingTable")
                .Join<RatingTablePartRecord>()
                .Join<CommonPartRecord>()
                .OrderByDescending(record => record.ModifiedUtc)
                .WithQueryHints(new QueryHints().ExpandRecords(new string[] {"AutoroutePartRecord", "TitlePartRecord"})).List();

            return new ShapeResult(this, _orchardServices.New.RatingTables(RatingTables: ratingTables));
        }

        public ActionResult GetRatingTableEditor(int id = 0){
            return GetRatingTableShapeResult(GetItem(id));
        }

        [HttpPost, ActionName("GetRatingTableEditor")]
        public ActionResult GetRatingTableEditorPost(int id = 0)
        {
            var item = GetItem(id);
            if (item == null) return new HttpNotFoundResult();

            if (id == 0) _contentManager.Create(item);

            var editorShape = _contentManager.UpdateEditor(item, this);

            if (!ModelState.IsValid)
            {
                _transactionManager.Cancel();

                return GetRatingTableShapeResult(item);
            }

            _orchardServices.Notifier.Information(T("{0}, the Rating Table item was successfully saved.", _orchardServices.WorkContext.CurrentUser.UserName));

            return RedirectToAction("GetRatingTableEditor", new {id});
        }

        public ActionResult AddRatingRecord(int id = 0){
            var ratingTableContentItem = GetItem(id);

            var ratingTablePartItem = ratingTableContentItem.As<RatingTablePart>();
            ratingTablePartItem.RatingRecords.Add(new RatingRecord());

            //_transactionManager.Cancel();
            //return GetRatingTableShapeResult(ratingTableContentItem);

            return RedirectToAction("GetRatingTableEditor", new {id});
        }

        [HttpPost, ActionName("AddRatingRecord")]
        public ActionResult AddRatingRecordPost(int id = 0){
            return GetRatingTableEditorPost(id);
            //return RedirectToAction("GetRatingTableEditor", new {id});// GetRatingTableShapeResult(ratingTableContentItem);
        }

        public ActionResult DeleteRatingRecord(int id = 0, int ratingRecordId = 0)
        {
            var ratingTableContentItem = GetItem(id);

            var ratingTablePartItem = ratingTableContentItem.As<RatingTablePart>();
            ratingTablePartItem.RatingRecords.Remove(ratingTablePartItem.RatingRecords.FirstOrDefault(x => x.Id == ratingRecordId));

            //_transactionManager.Cancel();
            //return GetRatingTableShapeResult(ratingTableContentItem);

            return RedirectToAction("GetRatingTableEditor", new { id });
        }

        [HttpPost, ActionName("DeleteRatingRecord")]
        public ActionResult DeleteRatingRecordPost(int id = 0)
        {
            return GetRatingTableEditorPost(id);
        }

        public ActionResult SynchronizeWithRemoteSite(int id = 0)
        {
            //var ratingRecords = _ratingTableDataService.GetData();

            //var ratingTableContentItem = GetItem(id);
            //var ratingTablePartItem = ratingTableContentItem.As<RatingTablePart>();
            //ratingTablePartItem.RatingRecords.Clear();

            //foreach (var ratingRecord in ratingRecords)
            //{
            //    ratingTablePartItem.RatingRecords.Add(ratingRecord);
            //}

            //_tournamentDataService.GetPlayers();

            return RedirectToAction("GetRatingTableEditor", new { id });
        }

        private ShapeResult GetRatingTableShapeResult(ContentItem item)
        {
            var itemEditorShape = _contentManager.BuildEditor(item);
            var editorShape = _orchardServices.New.RatingTableDashboard(EditorShape: itemEditorShape);

            return new ShapeResult(this, editorShape);
        }

        private ContentItem GetItem(int id)
        {
            if (id == 0) return _contentManager.New("RatingTable");
            else return _contentManager.Get(id);
        }
        
        #region IUpdateModel members
        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
        #endregion
    }
}