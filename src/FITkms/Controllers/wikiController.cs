using BootstrapMvcSample.Controllers;
using FITkms.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using System.Net;
using FITkms.ViewModels;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace FITkms.Lucene
{
    public class wikiController : Controller
    {
        kmsEntities db = new kmsEntities();

        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult IndexPartial()
        {
           return PartialView(db.Articles.OrderByDescending(x => x.ArticleID).ToList().Take(3));

        }

        public ActionResult article(int id = 0)
        {
            Articles art = db.Articles.Find(id);

            ViewBag.artcat = (from c in db.ArticleCategories
                              from a in c.Articles.Where(ar => ar.ArticleID == id)
                              select c.Name).SingleOrDefault();

            ViewBag.data = (from t in db.Tags
                            from a in t.Articles.Where(x => x.ArticleID == id)
                            select t).ToList();

            if (art == null)
            {
                return HttpNotFound();
            }

            return View(art);
        }

        public ActionResult Create()
        {

            ViewBag.ArticleCategoryID = new SelectList(db.ArticleCategories, "ArticleCategoryID", "Name");
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(ArticleTagsViewModel vm)
        {

            #region publicip
            //Visitor IP address
            //string stringIpAddress;
            //stringIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if (stringIpAddress == null) 
            //{
            //    stringIpAddress = Request.ServerVariables["REMOTE_ADDR"];
            //}
            #endregion
            #region lanip
            //LAN IP address
            //Get the Host Name
            string stringHostName = Dns.GetHostName();
            //Get The Ip Host Entry
            IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
            //Get The Ip Address From The Ip Host Entry Address List
            IPAddress[] arrIpAddress = ipHostEntries.AddressList;
            #endregion

            if (ModelState.IsValid)
            {
                vm.article.DateCreated = DateTime.Now;
                vm.article.UserIPAddress = arrIpAddress[arrIpAddress.Length - 1].ToString();
                db.Articles.Add(vm.article);
                InsertTag(vm);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            ViewBag.ArticleCategoryID = new SelectList(db.ArticleCategories, "ArticleCategoryID", "Name", vm.article.ArticleCategoryID);
            return View(vm.article);
        }

        private void InsertTag(ArticleTagsViewModel vm)
        {
            string[] tags;
            tags = vm.tags.Name.Split(',');

            foreach (var item in tags)
            {
                if (getTagByName(item) == null)
                {
                    Tags temp = new Tags();
                    temp.Name = item.Trim();
                    db.Tags.Add(temp);
                    db.SaveChanges();
                    vm.article.Tags.Add(temp);
                }
                else
                {
                    Tags temp = getTagByName(item);
                    vm.article.Tags.Add(temp);
                }
            }
        }

        private Tags getTagByName(string name)
        {
            return db.Tags.Where(x => x.Name == name).SingleOrDefault();
        }

        public ActionResult Edit(int id)
        {
            Articles art = db.Articles.Find(id);
            ViewBag.ArticleCategoryID = new SelectList(db.ArticleCategories, "ArticleCategoryID", "Name", art.ArticleCategoryID);
            if (art == null)
            {
                return HttpNotFound();
            }
            return View(art);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(Articles article)
        {
            if (ModelState.IsValid)
            {

                article.DateModified = DateTime.Now;
                db.Entry(article).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ArticleCategoryID = new SelectList(db.ArticleCategories, "ArticleCategoryID", "Name", article.ArticleCategoryID);
            return View(article);
        }

        public ActionResult Delete(int id)
        {
            Articles article = db.Articles.Find(id);
            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Articles article = db.Articles.Find(id);

            List<Tags> tag = (from t in db.Tags
                              from a in t.Articles.Where(x => x.ArticleID == id)
                              select t).ToList();

            foreach (var item in tag)
            {
                db.Tags.Remove(item);
            }

            db.Articles.Remove(article);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Categories()
        {
            return View(db.ArticleCategories.ToList());
        }

        public ActionResult CreateArticleCategory()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateArticleCategory(ArticleCategories arCat)
        {
            if (ModelState.IsValid)
            {
                db.ArticleCategories.Add(arCat);
                db.SaveChanges();
                return RedirectToAction("Categories");
            }
            return View(arCat);
        }

        public ActionResult articlesbycategories(int id = 0)
        {
            ViewBag.CatName = (from a in db.ArticleCategories
                               where a.ArticleCategoryID == id
                               select a.Name).SingleOrDefault();

            return View(db.Articles.Where(x => x.ArticleCategoryID == id).OrderByDescending(x => x.ArticleID).ToList());
        }

        [ChildActionOnly]
        public ActionResult CategoriesMenu()
        {
            var cat = db.ArticleCategories.ToList();

            return PartialView(cat);
        }

        [ChildActionOnly]
        public ActionResult AllTagsPartial()
        {
            var tag = db.Tags.ToList().Take(25);

            return PartialView(tag);
        }


        public ActionResult articlesbytags(int id = 0)
        {
            ViewBag.tagName = (from a in db.Tags
                               where a.TagID == id
                               select a.Name).SingleOrDefault();

            var articles = (from a in db.Articles
                            from t in a.Tags.Where(x => x.TagID == id)
                            select a).ToList();

            return View(articles);
        }

        //Lucene
        #region Lucene



        #endregion
    }
}
