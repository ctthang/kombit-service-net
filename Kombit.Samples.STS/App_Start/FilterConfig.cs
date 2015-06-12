#region

using System.Web.Mvc;

#endregion

namespace Kombit.Samples.STS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}