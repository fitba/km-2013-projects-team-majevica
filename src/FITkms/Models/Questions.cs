//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FITkms.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Questions
    {
        public Questions()
        {
            this.Answers = new HashSet<Answers>();
        }
    
        public int QuestionID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> ArticleID { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
    
        public virtual ICollection<Answers> Answers { get; set; }
        public virtual Articles Articles { get; set; }
        public virtual Users Users { get; set; }
    }
}
