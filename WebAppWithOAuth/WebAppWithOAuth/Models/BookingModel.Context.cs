﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebAppWithOAuth.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MeetingRoomManagerEntities : DbContext
    {
        public MeetingRoomManagerEntities()
            : base("name=MeetingRoomManagerEntities")
        {
            Database.Connection.ConnectionString = Database.Connection.ConnectionString.Replace("xxxxx", "Leesin#12");
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BookingNew> BookingNews { get; set; }
    }
}
