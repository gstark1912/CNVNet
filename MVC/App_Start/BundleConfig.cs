using System.Web.Optimization;

namespace MVC.Security
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/Plugins").Include(
                                         "~/Scripts/Plugins/*.js",
                                         "~/Scripts/Plugins/jqgrid/*.js",
                                         "~/Scripts/Plugins/jqgrid/i18n/grid.locale-es.js"
                                         ));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                                         "~/Scripts/jquery-{version}.js"
                                         ));

            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                                          "~/Scripts/jquery-ui-{version}.custom.js"
                                         ));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/mobydick").Include("~/Scripts/mobydick-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                                        "~/Content/css/jquery-ui-1.10.3.custom.css",
                                        "~/Content/bootstrap/_ui-mobydick.css",
                                        "~/Content/css/ui.dynatree.css",
                                        "~/Content/css/ui.jqgrid.css"
                                        ));

            BundleTable.EnableOptimizations = false;
        }
    }
}
