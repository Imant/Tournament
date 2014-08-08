using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Security.Permissions;
using Orchard.UI.Navigation;

namespace Custom.Tournament
{
    [OrchardFeature("Custom.Tournament")]
    public class TournamentAdminMenu : Component, INavigationProvider
    {
        public string MenuName { get { return "admin"; } }


        public void GetNavigation(NavigationBuilder builder)
        {
            builder
                .Add(T("Tournament dashboard"), "6", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu){
            menu
                .LinkToFirstChild(true)

                .Add(subitem => subitem
                                    .Caption(T("Edit Tournament"))
                                    .Action("TournamentDashboard", "TournamentAdmin", new { area = "Custom.Tournament" })
                                    .LinkToFirstChild(true)
                                    .Add(subsubitem => subsubitem
                                                           .Caption(T("Rating"))
                                                           .Action("TournamentDashboard", "TournamentAdmin", new { area = "Custom.Tournament" })
                                                           .LocalNav(true)
                                    )

                                    .Add(subsubitem => subsubitem
                                                           .Caption(T("Edit Tours"))
                                                           .Action("EditTours", "TournamentAdmin", new { area = "Custom.Tournament" })
                                                           .LocalNav(true)
                                    ));

        }
    }
}