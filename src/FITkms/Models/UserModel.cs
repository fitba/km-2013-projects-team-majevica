using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FITkms.Models;

namespace FITkms.Models
{
    public class UserModel
    {

        public Users GetLogin(string username, string password)
        {
            kmsEntities db = new kmsEntities();
            return db.Users.Where(x => x.Username == username && x.Password == password).SingleOrDefault();
        }
    }
}