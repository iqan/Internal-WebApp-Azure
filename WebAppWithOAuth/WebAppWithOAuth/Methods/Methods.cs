﻿using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using WebAppWithOAuth.Models;

namespace WebAppWithOAuth.Methods
{
    public static class Methods
    {
        public static string ExportToExcel(DataTable dt, string dest, DateTime fromDate, DateTime toDate)
        {

            /*Set up work book, work sheets, and excel application*/
            try
            {

                var file = new FileInfo(dest);
                using (var xp = new ExcelPackage(file))
                {
                    string tableName = "Forecast_" + DateTime.Today.ToString("dd-MM-yyyy");
                    ExcelWorksheet ws = xp.Workbook.Worksheets.Add(tableName);

                    //Headers
                    ws.Cells["A1"].Value = "Month";
                    ws.Cells["B1"].Value = "Project#";
                    ws.Cells["C1"].Value = "Project Name";
                    ws.Cells["D1"].Value = "Resource Name";
                    ws.Cells["E1"].Value = "Billing Period";
                    ws.Cells["F1"].Value = "Rate";
                    ws.Cells["G1"].Value = "Leaves";
                    ws.Cells["H1"].Value = "Billing days";
                    ws.Cells["I1"].Value = "Billing (Total)";

                    using (var range = ws.Cells["A1:I1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSeaGreen);
                        range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.ShrinkToFit = false;
                    }

                    //var resources = dt.AsEnumerable().Select(r => r.Field<int>("Resource Name")).ToList();
                    List<Resource> resources = new List<Resource>();

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["Resource Name"].ToString() != "")
                        {
                            Resource res = new Resource();
                            res.ProjectId = long.Parse((string)row[0]);
                            res.ProjectName = (string)row[1];
                            res.ResourceName = (string)row[2];
                            res.BillingPeriod = "na";
                            res.Rate = int.Parse((string)row[8]);
                            res.Leaves = 0;
                            res.BillingDays = 20;
                            res.TotalBilling = res.Rate * res.BillingDays;
                            res.EndDate = DateTime.Parse((string)row[7]);
                            res.StartDate = DateTime.Parse((string)row[6]);
                            if (ws.Dimension.End.Column == 13)
                                if ((string)row[13] != "")
                                    res.LikelyEntensionTill = DateTime.Parse((string)row[13]);

                            res.Extension = false;
                            res.OverMonth = 0;

                            res.Extension = (res.LikelyEntensionTill >= DateTime.Today) ? true : false;
                            resources.Add(res);
                        }
                    }

                    int i = 2;

                    for (DateTime index = new DateTime(fromDate.Year, fromDate.Month, 1); index < toDate; index = index.AddMonths(1))
                    {
                        foreach (var res in resources)
                        {
                            var mn = new DateTimeFormatInfo();
                            int count = 0;
                            int days = 0;

                            DateRange range = new DateRange(res.StartDate, res.EndDate);

                            DateTime[] bps = GetBillingPeriodGeneral(index);
                            DateTime tempEndDate = bps[1];

                            for (DateTime index2 = index; index2 < index.AddMonths(1); index2 = index2.AddDays(1))
                            {
                                //if (index2.Month < 4)
                                //    bps = GetBillingPeriodGeneral(index2.AddYears(-1));
                                //else if (index2 == GetFinancialYearStartDate(index2))
                                //    bps = GetBillingPeriodGeneral(index2);

                                if (index2 == res.EndDate.AddDays(1) && res.Extension)
                                {
                                    res.StartDate = res.EndDate.AddDays(1);
                                    res.EndDate = res.LikelyEntensionTill;
                                    range = new DateRange(res.StartDate, res.EndDate);
                                    tempEndDate = GetBillingPeriodGeneral(index2)[1];
                                    res.OverMonth = 1;
                                    count = 0;
                                }
                                if (res.OverMonth == 1)
                                {
                                    using (var r = ws.Cells[i, 1, i, 9])
                                    {
                                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        r.Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                                    }
                                }
                                else
                                {
                                    using (var r = ws.Cells[i, 1, i, 9])
                                    {
                                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        r.Style.Fill.BackgroundColor.SetColor(Color.White);
                                    }
                                }

                                if (range.Includes(index2))
                                {
                                    if (res.StartDate >= bps[0])
                                        days = BillingDays(res.StartDate, bps[1]);
                                    else if (res.EndDate <= bps[1])
                                    {
                                        days = BillingDays(bps[0], res.EndDate);
                                        tempEndDate = res.EndDate;
                                    }
                                    else
                                        days = BillingDays(bps[0], bps[1]);

                                    if (index2 == tempEndDate && count == 0)
                                    {
                                        ws.Cells[i, 1].Value = mn.GetAbbreviatedMonthName(index.Month) + "-" +
                                                               index.ToString("yy");
                                        ws.Cells[i, 2].Value = res.ProjectId;
                                        ws.Cells[i, 3].Value = res.ProjectName;
                                        ws.Cells[i, 4].Value = res.ResourceName;
                                        ws.Cells[i, 5].Value = "From " + bps[0].ToString("MMM") + " " + bps[0].Day +
                                                               " till " + bps[1].ToString("MMM") + " " + bps[1].Day;
                                        ws.Cells[i, 6].Value = res.Rate;
                                        ws.Cells[i, 7].Value = res.Leaves;
                                        ws.Cells[i, 8].Value = days;
                                        ws.Cells[i, 9].Value = days * res.Rate;
                                        i++;
                                        count++;
                                    }
                                }
                            }
                        }
                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    xp.Save();
                }
                string ret = "File Exported successfully.";
                return ret;
            }
            catch (Exception ex)
            {
                string ret = "Error While Exporting.. Error = "+ ex.Message;
                return ret;
            }
        }

        public static DataTable ExcelSheetToDataTable(string path, string sName)
        {
            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(path))
                {
                    pck.Load(stream);
                }
                var ws = pck.Workbook.Worksheets.First();

                if (sName != string.Empty)
                    ws = pck.Workbook.Worksheets[sName];

                DataTable tbl = new DataTable();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    tbl.Columns.Add(firstRowCell.Text);
                }
                for (int rowNum = 2; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    DataRow row = tbl.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                }
                return tbl;
            }
        }

        #region Billing Period logic

        public static DateTime[] GetBillingPeriodGeneral(DateTime index)
        {
            List<DateTime> billingPS = new List<DateTime>();
            List<DateTime> billingES = new List<DateTime>();

            DateTime tempData = new DateTime();

            DateTime financialYearStartDate = new DateTime(index.Year, 4, 1);
            DateTime financialYearEndDate = new DateTime(index.Year + 1, 3, 31);
            if (index.Month < 4)
            {
                financialYearStartDate = new DateTime(index.Year - 1, 4, 1);
                financialYearEndDate = new DateTime(index.Year, 3, 31);
            }


            List<int> w = new List<int>() { 4, 5, 4, 4, 5, 4, 4, 5, 4, 4, 5, 4, 4, 5, 4, 4 };

            tempData = GetFinancialYearStartDate(financialYearStartDate);
            billingPS.Add(tempData);
            bool change = false;
            int count = 0;

            for (DateTime i = tempData; i <= financialYearEndDate; )
            {
                if (change)
                {
                    if (i.AddDays(1).DayOfWeek == DayOfWeek.Saturday)
                        i = i.AddDays(3);
                    else if (i.AddDays(1).DayOfWeek == DayOfWeek.Sunday)
                        i = i.AddDays(2);
                    else
                        i = i.AddDays(1);
                    billingPS.Add(i);
                    change = false;
                }
                else
                {
                    if (w[count] == 4)
                        i = i.AddDays((w[count] * 5) + 5);
                    else
                        i = i.AddDays((w[count] * 5) + 7);
                    billingES.Add(i);
                    change = true;
                    count++;
                }
            }

            DateTime[] temp = new DateTime[2];
            if (index.Month >= 4)
            {
                temp[0] = billingPS[index.Month - 4];
                temp[1] = billingES[index.Month - 4];
            }
            else
            {
                temp[0] = billingPS[index.Month + 8];
                temp[1] = billingES[index.Month + 8];
            }

            return temp;
        }

        public static DateTime GetFinancialYearStartDate(DateTime index)
        {

            DateTime tempData = new DateTime();

            DateTime financialYearStartDate = new DateTime(index.Year, 4, 1);
            DateTime financialYearEndDate = new DateTime(index.Year + 1, 3, 31);

            switch (financialYearStartDate.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    tempData = financialYearStartDate;
                    break;
                case DayOfWeek.Tuesday:
                    tempData = financialYearStartDate.AddDays(-1);
                    break;
                case DayOfWeek.Wednesday:
                    tempData = financialYearStartDate.AddDays(-2);
                    break;
                case DayOfWeek.Thursday:
                    tempData = financialYearStartDate.AddDays(-3);
                    break;
                case DayOfWeek.Friday:
                    tempData = financialYearStartDate.AddDays(-4);
                    break;
                case DayOfWeek.Saturday:
                    tempData = financialYearStartDate.AddDays(-5);
                    break;
                case DayOfWeek.Sunday:
                    tempData = financialYearStartDate.AddDays(-6);
                    break;
            }
            return tempData;
        }

        #endregion

        #region Billing days count

        public static int BillingDays(DateTime startDate, DateTime endDate)
        {
            int count = 0;
            for (DateTime index = startDate; index <= endDate; index = index.AddDays(1))
            {
                if (index.DayOfWeek != DayOfWeek.Sunday && index.DayOfWeek != DayOfWeek.Saturday)
                {
                    count++;
                }
            }
            return count;
        }

        public static int BillingDaysWithDateExclusion(DateTime startDate, DateTime endDate, Boolean excludeWeekends,
            List<DateTime> excludeDates)
        {
            int count = 0;
            for (DateTime index = startDate; index < endDate; index = index.AddDays(1))
            {
                if (excludeWeekends && index.DayOfWeek != DayOfWeek.Sunday && index.DayOfWeek != DayOfWeek.Saturday)
                {
                    bool excluded = false;
                    ;
                    for (int i = 0; i < excludeDates.Count; i++)
                    {
                        if (index.Date.CompareTo(excludeDates[i].Date) == 0)
                        {
                            excluded = true;
                            break;
                        }
                    }

                    if (!excluded)
                        count++;
                }
            }
            return count;
        }

        #endregion
    }

    public interface IRange<T>
    {
        T Start { get; }
        T End { get; }
        bool Includes(T value);
        bool Includes(IRange<T> range);
    }
    public class DateRange : IRange<DateTime>
    {
        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public bool Includes(DateTime value)
        {
            return (Start <= value) && (value <= End);
        }

        public bool Includes(IRange<DateTime> range)
        {
            return (Start <= range.Start) && (range.End <= End);
        }

        //usage
        //DateRange range = new DateRange(startDate, endDate);
        //range.Includes(date);
    }
} 