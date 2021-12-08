﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Sitko.Core.Apps.MudBlazorDemo.Data;

namespace Sitko.Core.Apps.Blazor.Migrations
{
    [DbContext(typeof(BarContext))]
    partial class BarContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.BarModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Bar")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("FooId")
                        .HasColumnType("uuid");

                    b.Property<string>("StorageItem")
                        .HasColumnType("jsonb")
                        .HasColumnName("StorageItem");

                    b.Property<string>("StorageItems")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("StorageItems");

                    b.Property<decimal>("Sum")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("FooId");

                    b.ToTable("Bars");
                });

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.FooModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("BarModelId")
                        .HasColumnType("uuid");

                    b.Property<string>("Foo")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BarModelId");

                    b.ToTable("Foos");
                });

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.BarModel", b =>
                {
                    b.HasOne("Sitko.Core.Apps.Blazor.Data.Entities.FooModel", "Foo")
                        .WithMany()
                        .HasForeignKey("FooId");

                    b.Navigation("Foo");
                });

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.FooModel", b =>
                {
                    b.HasOne("Sitko.Core.Apps.Blazor.Data.Entities.BarModel", null)
                        .WithMany("Foos")
                        .HasForeignKey("BarModelId");
                });

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.BarModel", b =>
                {
                    b.Navigation("Foos");
                });
#pragma warning restore 612, 618
        }
    }
}
