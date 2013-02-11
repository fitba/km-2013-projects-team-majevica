using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FITkms.Models;

namespace FITkms.Models
{
    public class ArticlesDataRepo
    {
        

        public static Articles Get(int id)
        {
            //return all.SingleOrDefault(x => x.ArticleID.Equals(id));
            return all().SingleOrDefault(x => x.ArticleID.Equals(id));
        }

        public static List<Articles> all(){

            using (kmsEntities db = new kmsEntities())
            {
                return db.Articles.ToList();
                //    var query = (from x in db.Articles
                //                 select new
                //                 {
                //                    x.ArticleID, 
                //                    x.Title, 
                //                    x.Content}).ToList();

                //    return query; 
                //}
            }
        }

        //public object GetCartByUser(int p)
        //{
        //    var query = (from s in context.UserCart
        //                 where s.UserID == p
        //                 orderby s.UserCartID descending
        //                 select new
        //                 {
        //                     s.UserCartID,
        //                     Product = s.Product.Title,
        //                     s.Amount,
        //                     Price = s.Product.Price,
        //                     TotalPrice = s.Amount * s.Product.Price,
        //                     Image = s.Product.SourceUrl
        //                 }).ToList();
        //    return query;
        //}

                                        
    }
}