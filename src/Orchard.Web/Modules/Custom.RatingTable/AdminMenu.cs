using Orchard;
using Orchard.Environment.Extensions;
using Orchard.UI.Navigation;

namespace Custom.RatingTable
{
    [OrchardFeature("Custom.RatingTable")]
    public class AdminMenu : Component, INavigationProvider
    {
        public string MenuName { get { return "admin"; } }


        public void GetNavigation(NavigationBuilder builder){
            builder
                .Add(T("Rating Table dashboard"), "6", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu)
        {
            menu
                .LinkToFirstChild(true)

                .Add(subitem => subitem
                    .Caption(T("Edit Rating Tables"))
                    .Action("GetRatingTableList", "RatingTableAdmin", new { area = "Custom.RatingTable" })
                );

        }
    }
}