using MVC.Security.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Security.Controllers
{
    public class AccessLogsController : Controller
    {
        // GET: AccessLogs
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewUsersAccess()
        {
            return View();
        }
        public ActionResult GetUsersAccess(string sidx, string sord, int page, int rows)
        {
            List<AspNetUsersAccess> result;
            int totalcount;
            using (var dbContext = new ApplicationDbContext())
            {
                IQueryable<AspNetUsersAccess> query = dbContext.AspNetUsersAccess;
                query = sord == "desc" ? query.OrderByDescending(q => q.Date) : query.OrderBy(q => q.Date);
                totalcount = query.Count();
                query = query.Skip((page - 1) * rows).Take(rows);
                result = query.ToList();
            }

            int total = totalcount;

            int totalPages = (int)Math.Ceiling((decimal)total / (decimal)rows);
            var data = new
            {
                total = totalPages,
                page = page,
                records = total,
                rows = from c in result
                       select new
                       {
                           cell = new string[] 
                                   {                                       
                                        c.Id.ToString(),
                                        c.Route.ToString(),
                                        c.Date.ToString() ,
                                        c.UserName
                                   }
                       }
            };
            return Json(data);
        }

        public ActionResult ViewUsersNoAccess()
        {
            return View();
        }

        public ActionResult GetUsersNoAccess(string sidx, string sord, int page, int rows)
        {
            List<AspNetUsersNoAccess> result;
            int totalcount;
            using (var dbContext = new ApplicationDbContext())
            {
                IQueryable<AspNetUsersNoAccess> query = dbContext.AspNetUsersNoAccess;
                query = sord == "desc" ? query.OrderByDescending(q => q.Date) : query.OrderBy(q => q.Date);
                totalcount = query.Count();
                query = query.Skip((page - 1) * rows).Take(rows);
                result = query.ToList();
            }

            int total = totalcount;

            int totalPages = (int)Math.Ceiling((decimal)total / (decimal)rows);
            var data = new
            {
                total = totalPages,
                page = page,
                records = total,
                rows = from c in result
                       select new
                       {
                           cell = new string[] 
                                   {                                       
                                        c.Id.ToString(),
                                        c.Route.ToString(),
                                        c.Date.ToString() ,
                                        c.UserName
                                   }
                       }
            };
            return Json(data);
        }
    }
}