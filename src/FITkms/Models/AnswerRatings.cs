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
    
    public partial class AnswerRatings
    {
        public int UserID { get; set; }
        public int AnswerID { get; set; }
        public Nullable<int> RatingPoints { get; set; }
    
        public virtual Answers Answers { get; set; }
        public virtual Users Users { get; set; }
    }
}
