using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FITkms.Models;
using System.Web.Mvc;

namespace FITkms.ViewModels
{
    public class ArticleTagsViewModel
    {
        public Articles article { get; set; }
        public Tags tags { get; set; }
        //public ICollection<ArticleCategories> ArticleCategories { get; set; }
        
        
        
        //public SelectList Genres = new SelectList(repository.Genres, "Name", "Id");
    }
}