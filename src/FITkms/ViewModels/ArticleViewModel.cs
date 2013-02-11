using FITkms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FITkms.ViewModels
{
    public class ArticleViewModel
    {
        public int Limit { get; set; }
        public bool SearchDefault { get; set; }
        public Articles Articles { get; set; }
        public IEnumerable<Articles> AllArticles { get; set; }
        public IEnumerable<Articles> SearchIndexData { get; set; }
        public IList<SelectedList> SearchFieldList { get; set; }
        public string SearchTerm { get; set; }
        public string SearchField { get; set; }
    }
}