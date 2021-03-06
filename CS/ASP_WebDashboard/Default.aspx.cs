﻿using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASP_WebDashboard {
    public partial class Default : System.Web.UI.Page {

        const string extractFileName = "\"ExtractDS_\"yyyyMMddHHmmssfff\".dat\"";
        protected void Page_Load(object sender, EventArgs e) {
            DataSourceInMemoryStorage dataSourceStorage = new DataSourceInMemoryStorage();
            dataSourceStorage.RegisterDataSource("extractDataSource", CreateExtractDataSource().SaveToXml());
            ASPxDashboard1.SetDataSourceStorage(dataSourceStorage);
        }

        protected void ASPxDashboard1_ConfigureDataConnection(object sender, ConfigureDataConnectionWebEventArgs e) {
            ExtractDataSourceConnectionParameters extractCP = e.ConnectionParameters as ExtractDataSourceConnectionParameters;
            if (extractCP != null) {
                extractCP.FileName = GetExtractFileName();
            }
        }
        protected void ASPxDashboard1_CustomParameters(object sender, CustomParametersWebEventArgs e) {
            e.Parameters.Add(new DashboardParameter("ExtractFileName", typeof(string), GetExtractFileName()));
        }

        private string GetExtractFileName() {
            var path = Server.MapPath("~/App_Data/ExtractDataSource/");
            var file = Directory.GetFiles(path).Select(fn => new FileInfo(fn)).OrderByDescending(f => f.CreationTime).FirstOrDefault();
            if (file != null)
                return file.FullName;
            else
                return AddExtractDataSource();
        }

        private static DashboardExtractDataSource CreateExtractDataSource() {
            DashboardSqlDataSource nwindDataSource = new DashboardSqlDataSource("Northwind Invoices", "nwindConnection");
            SelectQuery invoicesQuery = SelectQueryFluentBuilder
                .AddTable("Invoices")
                .SelectColumns("City", "Country", "Salesperson", "OrderDate", "Shippers.CompanyName", "ProductName", "UnitPrice", "Quantity", "Discount", "ExtendedPrice", "Freight")
                .Build("Invoices");
            nwindDataSource.Queries.Add(invoicesQuery);
            nwindDataSource.ConnectionOptions.CommandTimeout = 600;

            DashboardExtractDataSource extractDataSource = new DashboardExtractDataSource("Invoices Extract Data Source");

            extractDataSource.ExtractSourceOptions.DataSource = nwindDataSource;
            extractDataSource.ExtractSourceOptions.DataMember = "Invoices";
            return extractDataSource;
        }

        [WebMethod]
        public static string AddExtractDataSource() {
            string fileName = DateTime.Now.ToString(extractFileName);
            string path = HostingEnvironment.MapPath("~/App_Data/ExtractDataSource/");
            string tempPath = path + "Temp\\";
            Directory.CreateDirectory(tempPath);
            using (var ds = CreateExtractDataSource()) {
                ds.FileName = tempPath + fileName;
                ds.UpdateExtractFile();
            }
            File.Move(tempPath + fileName, path + fileName);
            if (!Directory.EnumerateFiles(tempPath).Any()) {
                Directory.Delete(tempPath);
            }
            return path + fileName;
        }


    }
}