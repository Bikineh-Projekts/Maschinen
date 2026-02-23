using Microsoft.AspNetCore.Mvc;

namespace MaschinenDataein.Controllers
{
    public class TableSelectionController : Controller
    {
        public IActionResult SelectTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", tableName);
        }
    }
}
