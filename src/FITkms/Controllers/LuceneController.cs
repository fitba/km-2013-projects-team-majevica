using Lucene.Net.Store;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FITkms.Models;
using FITkms.Lucene;
using Lucene.Net;
using FITkms.ViewModels;

namespace FITkms.Lucene
{
    public class LuceneController : Controller
    {
       
        public ActionResult Index (string searchTerm, string searchField, bool? searchDefault, int? limit)
        {
            // create default Lucene search index directory
            if (!System.IO.Directory.Exists(LuceneSearch._luceneDir)) System.IO.Directory.CreateDirectory(LuceneSearch._luceneDir);

            // perform Lucene search
            List<Articles> _searchResults;
            if (searchDefault == true)
                _searchResults = (string.IsNullOrEmpty(searchField)
                                   ? LuceneSearch.SearchDefault(searchTerm)
                                   : LuceneSearch.SearchDefault(searchTerm, searchField)).ToList();
            else
                _searchResults = (string.IsNullOrEmpty(searchField)
                                   ? LuceneSearch.Search(searchTerm)
                                   : LuceneSearch.Search(searchTerm, searchField)).ToList();
            if (string.IsNullOrEmpty(searchTerm) && !_searchResults.Any())
                _searchResults = LuceneSearch.GetAllIndexRecords().ToList();


            // setup and return view model
            var search_field_list = new
                List<SelectedList> {
				                     	new SelectedList {Text = "(All Fields)", Value = ""},
				                     	new SelectedList {Text = "Article ID", Value = "ArticleID"},
				                     	new SelectedList {Text = "Title", Value = "Title"},
				                     	new SelectedList {Text = "Content", Value = "Content"}
				                     };

            // limit display number of database records
            var limitDb = limit == null ? 3 : Convert.ToInt32(limit);
            List<Articles> allSampleData;
            if (limitDb > 0)
            {
                allSampleData = ArticlesDataRepo.all().ToList().Take(limitDb).ToList();
                ViewBag.Limit = ArticlesDataRepo.all().Count - limitDb;
            }
            else allSampleData = ArticlesDataRepo.all();

            return View(new ArticleViewModel
            {
                AllArticles = allSampleData,
                SearchIndexData = _searchResults,
                
                SearchFieldList = search_field_list,
            });
        }



        public ActionResult Search(string searchTerm, string searchField, string searchDefault)
        {
            return RedirectToAction("Index", new { searchTerm, searchField, searchDefault });
        }

        public ActionResult CreateIndex()
        {
            LuceneSearch.AddUpdateLuceneIndex(ArticlesDataRepo.all());
            TempData["Result"] = "Search index was created successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddToIndex(Articles article)
        {
            LuceneSearch.AddUpdateLuceneIndex(article);
            TempData["Result"] = "Record was added to search index successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult ClearIndex()
        {
            if (LuceneSearch.ClearLuceneIndex())
                TempData["Result"] = "Search index was cleared successfully!";
            else
                TempData["ResultFail"] = "Index is locked and cannot be cleared, try again later or clear manually!";
            return RedirectToAction("Index");
        }

       

        public ActionResult ClearIndexRecord(int id)
        {
            LuceneSearch.ClearLuceneIndexRecord(id);
            TempData["Result"] = "Search index record was deleted successfully!";
            return RedirectToAction("Index");
        }

       
        public ActionResult OptimizeIndex()
        {
            LuceneSearch.Optimize();
            TempData["Result"] = "Search index was optimized successfully!";
            return RedirectToAction("Index");
        }
    }
}
