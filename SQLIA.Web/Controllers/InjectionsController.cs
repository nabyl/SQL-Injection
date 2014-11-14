using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SQLIA.Model;
using System.IO;
using SQLIA.Scanner;

namespace SQLIA.Web.Controllers
{
    public class InjectionsController : Controller
    {
        private SQLLiteralsEntities db = new SQLLiteralsEntities();

        // GET: Injections
        public ActionResult Scan()
        {
            return View();
        }


        [HttpPost]
        public ActionResult RunScan(SQLIA.Web.Models.UploadForScanView inputFile)
        {
            var reader = new StreamReader(inputFile.File.InputStream);
            int count = 0;

            try
            {

                if (inputFile.File != null)
                {
                    var scan = new Scan();
                    scan.Date = DateTime.Now;

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        //need to modify to split between the last ',', rather then the first
                        string script = line.Substring(0, line.LastIndexOf(','));
                        string last = line.Substring(line.LastIndexOf(',') + 1);

                        Literal literalFound = new Literal();

                        // basic details
                        var scanResult = new ScanEntry
                        {
                            Content = script,
                            ActualPossiblity = (last == "0" ? false : true)
                        };

                        // run scannnig class
                        SQLValidator scanner = new SQLValidator(script);
                        scanner.CheckInjections();

                        if (scanner.AttackTypes.Count() > 0)
                        {
                            scanResult.Description = "";

                            //if injection code found - add details to the scan result object
                            scanResult.InjectionAttackPossible = true;
                            foreach (var message in scanner.Messages.Distinct())
                            {
                                scanResult.Description += message + "<br />";
                            }

                            foreach (var attackTypeId in scanner.AttackTypes.Distinct())
                            {
                                scanResult.ScanEntryPossibleAttackTypes.Add(new ScanEntryPossibleAttackType
                                {
                                    AttackTypeID = attackTypeId
                                });
                            }
                        }
                        else
                        {
                            scanResult.Description = "";
                            foreach (var message in scanner.Messages.Distinct())
                            {
                                scanResult.Description += message + "<br />";
                            }
                            scanResult.InjectionAttackPossible = false;

                        }

                        scan.ScanEntries.Add(scanResult);

                    }

                    scan.CaculateStatistics(); // statistics calculation
                    db.Scans.Add(scan);
                    db.SaveChanges();

                    return RedirectToAction("ScanResult", new { id = scan.ScanID });
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting  
                        // the current instance as InnerException  
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
            return RedirectToAction("Scan");
        }

        public ActionResult ScanResult(int id)
        {
            var scan = db.Scans.Find(id);
            return View(scan);
        }



        //list all previous scans
        [ChildActionOnly]
        public PartialViewResult ListScanHistory()
        {
            var scans = db.Scans;
            return PartialView("_PreviousScans", scans);
        }


        public FilePathResult DownloadSample()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "download/";
            string fileName = "data.txt";
            return File(path + fileName, "text", "sample.txt");
        }

    }
}