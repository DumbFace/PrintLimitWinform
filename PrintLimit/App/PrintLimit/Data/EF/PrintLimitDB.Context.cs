﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PrintLimit.Data.EF
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Print_LimitEntities : DbContext
    {
        public Print_LimitEntities()
            : base("name=Print_LimitEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<DM_ChiTietNhanVienMayIn> DM_ChiTietNhanVienMayIn { get; set; }
        public virtual DbSet<DM_MayIn> DM_MayIn { get; set; }
        public virtual DbSet<DM_NhanVien> DM_NhanVien { get; set; }
        public virtual DbSet<DM_NhomNhanVien> DM_NhomNhanVien { get; set; }
        public virtual DbSet<DM_NhomTaiKhoan> DM_NhomTaiKhoan { get; set; }
        public virtual DbSet<GiaHan> GiaHans { get; set; }
        public virtual DbSet<HT_ChiTietPhanQuyen> HT_ChiTietPhanQuyen { get; set; }
        public virtual DbSet<HT_ChucNang> HT_ChucNang { get; set; }
        public virtual DbSet<HT_Menu> HT_Menu { get; set; }
        public virtual DbSet<NV_BanIn> NV_BanIn { get; set; }
        public virtual DbSet<NV_PrintTam> NV_PrintTam { get; set; }
    }
}
