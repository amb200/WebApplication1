﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebApplication1.Data;

#nullable disable

namespace WebApplication1.Migrations
{
    [DbContext(typeof(PostgreSQLDbContext))]
    [Migration("20230616090443_initialPg")]
    partial class initialPg
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebApplication1.Entities.Issue", b =>
                {
                    b.Property<int>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("EventId"));

                    b.Property<string>("JsonField")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MetricType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("MetricValue")
                        .HasColumnType("double precision");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("EventId");

                    b.ToTable("Models");
                });
#pragma warning restore 612, 618
        }
    }
}
