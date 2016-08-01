using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WebAppWithOAuth.Models;

namespace WebAppWithOAuth.Controllers
{
    public class ForecastController : Controller
    {
        // GET: Forecast
        public ActionResult Index()
        {
            TempData["AvailForecast"] = "true";
            return View();
        }

        public ActionResult _Import()
        {
            return View();
        }

        private static string path = string.Empty;
        private static string newPath = string.Empty;
        [HttpPost]
        public ActionResult _Import(HttpPostedFileBase inputFile)
        {
            // Verify that the user selected a file
            if (inputFile != null && inputFile.ContentLength > 0)
            {
                path = string.Empty;
                newPath = string.Empty;

                // clear files
                DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/Content/uploads"));

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                // extract only the filename
                var fileName = Path.GetFileName(inputFile.FileName);
                // store the file inside ~/App_Data/uploads folder
                path = Path.Combine(Server.MapPath("~/Content/uploads"), fileName);
                newPath = Path.Combine(Server.MapPath("~/Content/uploads"), "SOW-PO-Forecast.xlsx");
                inputFile.SaveAs(path);
                try
                {
                    using (var pck = new OfficeOpenXml.ExcelPackage())
                    {
                        using (var stream = System.IO.File.OpenRead(path))
                        {
                            pck.Load(stream);
                        }
                        list = new List<SelectListItem>();
                        list.Add(new SelectListItem { Text = "Select worksheet", Value = "Select", Selected = true });
                        foreach (var x in pck.Workbook.Worksheets)
                        {
                            list.Add(new SelectListItem { Text = x.Name, Value = x.Name, Selected = false });
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ImportError"] = "Error while reading file! Err = " + ex.Message;
                }
            }
            // redirect back to the index action to show the form once again
            return RedirectToAction("Index");
        }
        static List<SelectListItem> list = new List<SelectListItem>();
        public ActionResult _Export()
        {
            Resource ftm = new Resource();
            
            ftm.listworksheets = list;
            return View(ftm);
        }

        [HttpPost]
        public FileResult _Export(Resource rsc)
        {
            string status = string.Empty;
            try
            {
                DataTable dt = Methods.Methods.ExcelSheetToDataTable(path, rsc.Worksheet);
                status = Methods.Methods.ExportToExcel(dt,newPath,rsc.StartDate,rsc.EndDate);
                if (status.Contains("success"))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(newPath);
                    string fileName = "SOW-PO-Forecast.xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ExportError"] = "Error while reading file! Err = " + ex.Message;
                return null;
            }
            TempData["ExportError"] = "Error while reading file! Err = " + status;
            return null;
        }
    }
}