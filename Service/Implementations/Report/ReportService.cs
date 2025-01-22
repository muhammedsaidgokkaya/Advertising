using Core.Data;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using Service.Implementations.User;
using Service.Interfaces.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementations.Report
{
    public class ReportService : IReportService
    {
        private readonly Repository<Context> _repository;

        public ReportService()
        {
            _repository = new Repository<Context>(new Context());
        }

        public int AddReport(string name, string account, string accountId, string typeId, string content, int reportType, int organizationId, DateTime? startDate, DateTime? endDate)
        {
            var report = new Core.Domain.Report.Report
            {
                Name = name,
                Account = account,
                AccountId = accountId,
                TypeId = typeId,
                Content = content,
                ReportType = reportType,
                OrganizationId = organizationId,
                InsertedDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                IsDeleted = false
            };

            _repository.Save(report);
            return report.Id;
        }

        public int IsDeletedReport(int id)
        {
            var report = GetReportById(id);
            if (report != null)
            {
                report.IsDeleted = !report.IsDeleted;
                report.UpdateDate = DateTime.UtcNow;

                _repository.Update(report);
                return report.Id;
            }
            return 0;
        }

        public Core.Domain.Report.Report GetReportById(int id)
        {
            return _repository.GetById<Core.Domain.Report.Report>(id);
        }

        public Core.Domain.Report.Report GetReport(int id, int organizationId, int reportType)
        {
            var data = _repository
                .FilterAsQueryable<Core.Domain.Report.Report>(p => 
                    p.Id.Equals(id) && 
                    p.Organization.Id.Equals(organizationId) && 
                    p.ReportType == reportType)
                .IncludeReport()
                .FirstOrDefault();
            return data;
        }

        public IEnumerable<Core.Domain.Report.Report> GetReports(int organizationId, string accountId, int reportType, string startDate, string endDate)
        {
            DateTime? startDateTime = null;
            DateTime? endDateTime = null;

            if (DateTime.TryParse(startDate, out var parsedStartDate))
            {
                startDateTime = DateTime.SpecifyKind(parsedStartDate, DateTimeKind.Utc);
            }

            if (DateTime.TryParse(endDate, out var parsedEndDate))
            {
                endDateTime = DateTime.SpecifyKind(parsedEndDate, DateTimeKind.Utc);
            }

            var data = _repository
                .FilterAsQueryable<Core.Domain.Report.Report>(
                    p => !p.IsDeleted
                         && p.IsActive
                         && p.AccountId == accountId
                         && p.ReportType == reportType
                         && p.Organization.Id.Equals(organizationId)
                         && (!startDateTime.HasValue || p.InsertedDate >= startDateTime)
                         && (!endDateTime.HasValue || p.InsertedDate <= endDateTime))
                .IncludeReport();
            return data;
        }
    }

    public static class ReportExtensions
    {
        public static IQueryable<Core.Domain.Report.Report> IncludeReport(this IQueryable<Core.Domain.Report.Report> query)
        {
            return query
                .Include(ma => ma.Organization);
        }
    }
}
