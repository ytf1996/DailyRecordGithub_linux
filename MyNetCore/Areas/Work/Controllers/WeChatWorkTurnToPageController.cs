using Microsoft.AspNetCore.Mvc;

namespace MyNetCore.Areas.Work.Controllers
{
    public class WeChatWorkTurnToPageController : WeChatWorkAuthController
    {
        // GET: Work/WeChatWorkTurnToPage
        public ActionResult Index()
        {
            string pageName = Request.Query["pageName"];

            switch (pageName)
            {
                case "zhuye":
                    return Redirect("/Management/Home/Index");
                case "jishiben":
                    return Redirect("/Management/Note/List");
                case "liaotianshi":
                    return Redirect("/Management/ChatRoom/List");
                case "hebingpdf":
                    return Redirect("/Home/PDFHelper");
                case "yinger":
                    return Redirect("/Management/BabyInfoDaliy/List");
                default:
                    return Redirect("/Management/Home/Index");
            }
        }
    }
}