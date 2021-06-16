﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Roogle.RoogleSpider.Db;

namespace Roogle.RoogleSpider.Migrations
{
    [DbContext(typeof(RoogleSpiderDbContext))]
    partial class RoogleSpiderDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.6");

            modelBuilder.Entity("Roogle.RoogleSpider.Db.Link", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("FromPage")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("LastSeenTime")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("ToPage")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("FromPage", "ToPage")
                        .IsUnique();

                    b.ToTable("Links");
                });

            modelBuilder.Entity("Roogle.RoogleSpider.Db.Page", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Contents")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("ContentsChanged")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastCrawled")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("PageHash")
                        .HasColumnType("int");

                    b.Property<int>("PageRank")
                        .HasColumnType("int");

                    b.Property<DateTime>("PageRankUpdatedTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("StatusCode")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("UpdatedTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("Url")
                        .IsUnique();

                    b.ToTable("Pages");
                });

            modelBuilder.Entity("Roogle.RoogleSpider.Db.SearchIndexEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("Page")
                        .HasColumnType("char(36)");

                    b.Property<string>("Word")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("Word", "Page")
                        .IsUnique();

                    b.ToTable("SearchIndex");
                });
#pragma warning restore 612, 618
        }
    }
}
