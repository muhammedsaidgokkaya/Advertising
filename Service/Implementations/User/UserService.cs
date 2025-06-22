using Core.Data;
using Core.Domain.App;
using Core.Domain.Meta;
using Core.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Implementations;
using Service.Interfaces.User;
using Service.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Service.Implementations.User
{
    public class UserService : IUserService
    {
        private readonly Repository<Context> _repository;

        public UserService()
        {
            _repository = new Repository<Context>(new Context());
        }

        #region Organization
        public int AddOrganization(string name, string hashCode, int userCount = 5, int accountCount = 2, string accountType = "", string address = "", string zipCode = "", string taskNumber = "", string phone = "")
        {
            var organization = new Organization
            {
                Name = name,
                OrganizationHashCode = hashCode,
                UserCount = userCount,
                AccountCount = accountCount,
                Address = address,
                ZipCode = zipCode,
                TaskNumber = taskNumber,
                Phone = phone,
                AccountType = accountType,
                InsertedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            _repository.Save(organization);
            return organization.Id;
        }

        public int UpdateOrganization(int id, string name, string address, string zipCode, string taskNumber)
        {
            var organization = GetOrganizationById(id);
            if (organization != null)
            {
                organization.Name = name;
                organization.Address = address;
                organization.ZipCode = zipCode;
                organization.TaskNumber = taskNumber;
                organization.UpdateDate = DateTime.UtcNow;

                _repository.Update(organization);
                return organization.Id;
            }
            return 0;
        }

        public int UpdateAccountOrganization(int id, string googleSearchConsole, string googleAnalytics, string metaAccount)
        {
            var organization = GetOrganizationById(id);
            if (organization != null)
            {
                organization.GoogleSearchConsole = googleSearchConsole;
                organization.GoogleAnalytics = googleAnalytics;
                organization.MetaAccount = metaAccount;
                organization.UpdateDate = DateTime.UtcNow;

                _repository.Update(organization);
                return organization.Id;
            }
            return 0;
		}

		public int UpdateMetaAccountOrganization(int id, string metaAccount)
		{
			var organization = GetOrganizationById(id);
			if (organization != null)
			{
				organization.MetaAccount = metaAccount;
				organization.UpdateDate = DateTime.UtcNow;

				_repository.Update(organization);
				return organization.Id;
			}
			return 0;
		}

		public int UpdateAnalyticsAccountOrganization(int id, string googleAnalytics)
		{
			var organization = GetOrganizationById(id);
			if (organization != null)
			{
				organization.GoogleAnalytics = googleAnalytics;
				organization.UpdateDate = DateTime.UtcNow;

				_repository.Update(organization);
				return organization.Id;
			}
			return 0;
		}

		public int UpdateAdsAccountOrganization(int id, string googleAds)
		{
			var organization = GetOrganizationById(id);
			if (organization != null)
			{
				organization.GoogleAccount = googleAds;
				organization.UpdateDate = DateTime.UtcNow;

				_repository.Update(organization);
				return organization.Id;
			}
			return 0;
		}

		public int UpdateSearchConsoleAccountOrganization(int id, string googleSearchConsole)
		{
			var organization = GetOrganizationById(id);
			if (organization != null)
			{
				organization.GoogleSearchConsole = googleSearchConsole;
				organization.UpdateDate = DateTime.UtcNow;

				_repository.Update(organization);
				return organization.Id;
			}
			return 0;
		}

		public int IsActiveOrganization(int id)
        {
            var organization = GetOrganizationById(id);
            if (organization != null)
            {
                organization.IsActive = !organization.IsActive;
                organization.UpdateDate = DateTime.UtcNow;

                _repository.Update(organization);
                return organization.Id;
            }
            return 0;
        }

        public int IsDeletedOrganization(int id)
        {
            var organization = GetOrganizationById(id);
            if (organization != null)
            {
                organization.IsDeleted = !organization.IsDeleted;
                organization.UpdateDate = DateTime.UtcNow;

                _repository.Update(organization);
                return organization.Id;
            }
            return 0;
        }

        public Organization GetOrganizationById(int id)
        {
            return _repository.GetById<Organization>(id);
        }

        public IEnumerable<Organization> GetOrganization()
        {
            var data = _repository.Filter<Organization>(p => p.IsActive && !p.IsDeleted);
            return data;
        }

		public Organization GetOrganizationHashCode(string code)
		{
			var data = _repository.Filter<Organization>(p => p.IsActive && !p.IsDeleted && p.OrganizationHashCode == code);
			return data.SingleOrDefault();
		}

		public string GetOrganizationMeta(int id)
		{
            var data = _repository.Filter<Organization>(p => p.IsActive && !p.IsDeleted && p.Id.Equals(id)).SingleOrDefault();
			return data.MetaAccount;
		}

		public string GetOrganizationGoogleAnalytics(int id)
		{
            var data = _repository.Filter<Organization>(p => p.IsActive && !p.IsDeleted && p.Id.Equals(id)).SingleOrDefault();
			return data.GoogleAnalytics;
		}

		public string GetOrganizationGoogleSearchConsole(int id)
		{
            var data = _repository.Filter<Organization>(p => p.IsActive && !p.IsDeleted && p.Id.Equals(id)).SingleOrDefault();
			return data.GoogleSearchConsole;
		}
		#endregion

		#region User
		public int AddUser(int organizationId, string firstName, string lastName, string mail, string phone, string title, DateTime? dateOfBirth, string gender, string address, string userName, string password)
        {
            var organization = GetOrganizationById(organizationId);
            if (organization != null)
            {
                var currentUser = GetUser(organizationId).Count() - 1;

                if (currentUser >= organization.UserCount)
                {
                    return 0;
                }

                var user = new Core.Domain.User.User
                {
                    OrganizationId = organizationId,
                    FirstName = firstName,
                    LastName = lastName,
                    Mail = mail,
                    Phone = phone,
                    Title = title,
                    DateOfBirth = dateOfBirth,
                    Gender = gender,
                    Address = address,
                    UserName = userName,
                    Password = password,
                    ActivityStatus = "offline",
                    InsertedDate = DateTime.UtcNow,
					LastActivity = DateTime.UtcNow,
					IsActive = true,
                    IsDeleted = false
                };

                _repository.Save(user);
                return user.Id;
            }
            return 0;
        }

        public int UpdateUserName(int id, string userName)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.UserName = userName;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public int UpdateUser(int id, string name, string userName, string password)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.UserName = userName;
                user.Password = password;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public int UpdatePassword(int id, string password)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.Password = password;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public int UpdateAdminUser(int id, string firstName, string lastName, string mail, string phone, string title, DateTime? dateOfBirth, string gender, string address)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.Mail = mail;
                user.Phone = phone;
                user.Title = title;
                user.DateOfBirth = dateOfBirth;
                user.Gender = gender;
                user.Address = address;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public int UpdateOnlyUser(int id, string firstName, string lastName, string mail, string phone, DateTime? dateOfBirth, string gender)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.Mail = mail;
                user.Phone = phone;
                user.DateOfBirth = dateOfBirth;
                user.Gender = gender;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

		public int UpdateLastActivity(int id)
		{
			var user = GetUserById(id);
			if (user != null)
			{
				user.LastActivity = DateTime.UtcNow;

				_repository.Update(user);
				return user.Id;
			}
			return 0;
		}

		public int IsActiveUser(int id)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public int IsDeletedUser(int id)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.IsDeleted = !user.IsDeleted;
                user.UpdateDate = DateTime.UtcNow;

                _repository.Update(user);
                return user.Id;
            }
            return 0;
        }

        public Core.Domain.User.User GetUserById(int id)
        {
            return _repository.GetById<Core.Domain.User.User>(id);
        }

        public IEnumerable<Core.Domain.User.User> GetUser(int organizationId)
        {
            var data = _repository.FilterAsQueryable<Core.Domain.User.User>(p => !p.IsDeleted && p.Organization.Id.Equals(organizationId)).IncludeUser();
            return data;
        }

        public Core.Domain.User.User GetPaymentUser(int organizationId)
        {
            var data = _repository
                .FilterAsQueryable<Core.Domain.User.User>(p =>
                    !p.IsDeleted &&
                    p.Organization.Id == organizationId &&
                    p.UserRole.Any(ur => ur.RoleId == 1))
                .IncludeUser();

            return data.SingleOrDefault();
        }

        public IEnumerable<Core.Domain.User.User> GetUsers(int organizationId, int userId)
        {
            var data = _repository
                .FilterAsQueryable<Core.Domain.User.User>(
                    p => !p.IsDeleted
                         && p.Organization.Id.Equals(organizationId)
                         && !p.Id.Equals(userId))
                .IncludeUser();
            return data;
        }

        public Core.Domain.User.User GetUserLogin(string mail, string password)
        {
            var data = _repository.FilterAsQueryable<Core.Domain.User.User>(p => p.IsActive && !p.IsDeleted)
                .IncludeUser()
                .FirstOrDefault(u => u.Mail == mail && u.Password == password); ;
            return data;
        }

		public IEnumerable<string> GetUserTitles(int organizationId, int userId)
		{
            var titles = _repository
                .FilterAsQueryable<Core.Domain.User.User>(p =>
                    !p.IsDeleted &&
                    p.Organization.Id.Equals(organizationId) &&
                    !p.Id.Equals(userId))
                .Select(p => p.Title)
                .Distinct()
                .ToList();

			return titles;
		}

		public IEnumerable<Core.Domain.User.User> GetDepartmentUsers(int organizationId, int userId, List<string> department)
		{
			var data = _repository
				.FilterAsQueryable<Core.Domain.User.User>(
					p => !p.IsDeleted
						 && p.Organization.Id.Equals(organizationId)
						 && !p.Id.Equals(userId)
				         && department.Contains(p.Title))
				.IncludeUser();
			return data;
		}

		public IEnumerable<Core.Domain.User.User> GetUserCheckMail(string mail)
		{
			var data = _repository
				.FilterAsQueryable<Core.Domain.User.User>(
					p => !p.IsDeleted && p.Mail == mail)
				.IncludeUser();
			return data;
		}

		public IEnumerable<Core.Domain.User.User> GetUserCheckMail(string mail, int userId)
		{
			var data = _repository
				.FilterAsQueryable<Core.Domain.User.User>(
					p => !p.IsDeleted && p.Mail == mail && p.Id != userId
				)
				.IncludeUser();

			return data;
		}
		#endregion

		#region Role
		public IEnumerable<Role> GetRole()
        {
            var data = _repository.FilterAsQueryable<Role>(x => true);
            return data;
        }

        public Role GetRoleById(int id)
        {
            return _repository.GetById<Role>(id);
        }
        #endregion

        #region UserRole
        public int AddUserRole(int userId, int roleId)
        {
            var user = GetUserById(userId);
            if (user != null)
            {
                var userRole = new Core.Domain.User.UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                };

                _repository.Save(userRole);
                return 1;
            }
            return 0;
        }

        public int RemoveUserRolesByUserId(int userId)
        {
            var userRoles = _repository.FilterAsQueryable<UserRole>(ur => ur.UserId == userId).ToList();

            if (userRoles.Any())
            {
                foreach (var userRole in userRoles)
                {
                    _repository.Delete(userRole);
                }

                return 1;
            }

            return 0;
        }

        public IEnumerable<UserRole> GetUserRole(int userId)
        {
            var data = _repository.FilterAsQueryable<UserRole>(p => p.UserId.Equals(userId))
                .IncludeUserRole();
            return data;
        }
        #endregion

        #region Plan
        public int AddOrUpdatePlan(int organizationId, float amount, int planId, bool isYearly, bool isPayment)
        {
            var card = GetPlan(organizationId);
            if (card == null)
            {
                var plan = new Plan
                {
                    Amount = amount,
                    PlanId = planId,
                    IsYearly = isYearly,
                    IsPayment = isPayment,
                    OrganizationId = organizationId,
                    InsertedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _repository.Save(plan);
                return plan.Id;
            }
            else
            {
                var updatePlan = GetPlanById(card.Id);
                if (updatePlan != null)
                {
                    updatePlan.Amount = amount;
                    updatePlan.PlanId = planId;
                    updatePlan.IsYearly = isYearly;
                    updatePlan.IsPayment = isPayment;
                    updatePlan.UpdateDate = DateTime.UtcNow;

                    _repository.Update(updatePlan);
                    return updatePlan.Id;
                }
            }

            return 0;
        }

        public int UpdateNextPaymentDatePlan(int id, bool isYearly)
        {
            DateTime now = DateTime.UtcNow;
            DateTime next;
            if (isYearly)
            {
                next = now.AddYears(1);
            }
            else
            {
                next = now.AddMonths(1);
            }

            var updatePlan = GetPlanById(id);
            if (updatePlan != null)
            {
                updatePlan.NextPaymentDate = next;
                updatePlan.UpdateDate = DateTime.UtcNow;

                _repository.Update(updatePlan);
                return updatePlan.Id;
            }

            return 0;
        }

        public int UpdateCronJobNextPaymentDatePlan(int id, bool isYearly, DateTime? date)
        {
            DateTime next;
            if (isYearly)
            {
                next = date.Value.AddYears(1);
            }
            else
            {
                next = date.Value.AddMonths(1);
            }

            var updatePlan = GetPlanById(id);
            if (updatePlan != null)
            {
                updatePlan.NextPaymentDate = next;
                updatePlan.UpdateDate = DateTime.UtcNow;

                _repository.Update(updatePlan);
                return updatePlan.Id;
            }

            return 0;
        }

        public int DeletePlan(int organizationId)
        {
            var card = GetPlan(organizationId);
            if (card != null)
            {
                var updatePlan = GetPlanById(card.Id);
                if (updatePlan != null)
                {
                    updatePlan.IsDeleted = true;
                    updatePlan.UpdateDate = DateTime.UtcNow;

                    _repository.Update(updatePlan);
                    return updatePlan.Id;
                }
            }

            return 0;
        }

        public int UpdateIsPaymentSuccessPlan(int organizationId)
        {
            var card = GetPlan(organizationId);
            if (card != null)
            {
                var updatePlan = GetPlanById(card.Id);
                if (updatePlan != null)
                {
                    updatePlan.IsPayment = true;
                    updatePlan.UpdateDate = DateTime.UtcNow;

                    _repository.Update(updatePlan);
                    return updatePlan.Id;
                }
            }

            return 0;
        }

        public int UpdateIsPaymentFailPlan(int organizationId)
        {
            var card = GetPlan(organizationId);
            if (card != null)
            {
                var updatePlan = GetPlanById(card.Id);
                if (updatePlan != null)
                {
                    updatePlan.IsPayment = false;
                    updatePlan.UpdateDate = DateTime.UtcNow;

                    _repository.Update(updatePlan);
                    return updatePlan.Id;
                }
            }

            return 0;
        }

        public Plan GetPlanById(int id)
        {
            return _repository.GetById<Plan>(id);
        }

        public Plan GetPlan(int organizationId)
        {
            var data = _repository.Filter<Plan>(p => p.IsActive && !p.IsDeleted && p.OrganizationId.Equals(organizationId));
            return data.SingleOrDefault();
        }

        public IEnumerable<Plan> GetExpiredOrDuePlansNotMatchedWithPayments()
        {
            var today = DateTime.UtcNow.Date;

            var payments = _repository.FilterAsQueryable<PaymentSuccess>(p => p.IsActive && !p.IsDeleted);

            var plans = _repository.FilterAsQueryable<Plan>(p => p.IsActive && !p.IsDeleted && p.NextPaymentDate <= today);

            if (payments.ToList() == null || plans.ToList() == null)
            {
                return plans.ToList();
            }

            var unmatchedPlans = plans.Where(plan =>
                !payments.Any(payment =>
                    payment.OrganizationId == plan.OrganizationId &&
                    payment.PaymentDate.Value.Date == plan.NextPaymentDate.Value.Date
                )
            );

            return unmatchedPlans.ToList();
        }
        #endregion

        #region Payment
        public int AddOrUpdateCard(int organizationId, string cardUserKey, string cardToken, string cardAlias)
        {
            var card = GetCard(organizationId);
            if (card == null)
            {
                var payment = new Payment
                {
                    CardUserKey = cardUserKey,
                    CardToken = cardToken,
                    CardAlias = cardAlias,
                    OrganizationId = organizationId,
                    InsertedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _repository.Save(payment);
                return payment.Id;
            }
            else
            {
                var updatePayment = GetPaymentById(card.Id);
                if (updatePayment != null)
                {
                    updatePayment.CardUserKey = cardUserKey;
                    updatePayment.CardToken = cardToken;
                    updatePayment.CardAlias = cardAlias;
                    updatePayment.UpdateDate = DateTime.UtcNow;

                    _repository.Update(updatePayment);
                    return updatePayment.Id;
                }
            }

            return 0;
        }

        public Payment GetPaymentById(int id)
        {
            return _repository.GetById<Payment>(id);
        }

        public Payment GetCard(int organizationId)
        {
            var data = _repository.Filter<Payment>(p => p.IsActive && !p.IsDeleted && p.OrganizationId.Equals(organizationId));
            return data.SingleOrDefault();
        }
        #endregion

        #region Subscription
        public Subscription GetSubscription(int planId, bool isYearly)
        {
            var data = _repository.Filter<Subscription>(p => p.PlanId == planId && p.IsYearly == isYearly);
            return data.SingleOrDefault();
        }
        #endregion

        #region PaymentSuccess
        public int AddPaymentSuccess(int organizationId, DateTime? date)
        {
            var payment = new PaymentSuccess
            {
                PaymentDate = date,
                OrganizationId = organizationId,
                InsertedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            _repository.Save(payment);
            return payment.Id;
        }

        public IEnumerable<PaymentSuccess> GetPaymentSuccess()
        {
            var data = _repository.FilterAsQueryable<PaymentSuccess>(p => p.IsActive && !p.IsDeleted);
            return data;
        }
        #endregion

        #region PaymentFail
        public int AddPaymentFail(int organizationId, string message)
        {
            var payment = new PaymentFail
            {
                Message = message,
                OrganizationId = organizationId,
                InsertedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            _repository.Save(payment);
            return payment.Id;
        }

        public int DeletePaymentFail(int organizationId)
        {
            var existingFails = _repository.Filter<PaymentFail>(
                p => p.OrganizationId == organizationId && !p.IsDeleted
            ).ToList();

            if (existingFails != null)
            {
                foreach (var fail in existingFails)
                {
                    fail.IsDeleted = true;
                    fail.UpdateDate = DateTime.UtcNow;

                    _repository.Update(fail);
                    return fail.Id;
                }
            }

            return 0;
        }

        public PaymentFail GetPaymentFail(int organizationId)
        {
            var data = _repository.Filter<PaymentFail>(p => p.IsActive && !p.IsDeleted && p.OrganizationId.Equals(organizationId));
            return data.SingleOrDefault();
        }
        #endregion
    }

    public static class UserExtensions
    {
        public static IQueryable<Core.Domain.User.User> IncludeUser(this IQueryable<Core.Domain.User.User> query)
        {
            return query
                .Include(ma => ma.UserRole)
                .Include(ma => ma.Organization);
        }

        public static IQueryable<UserRole> IncludeUserRole(this IQueryable<UserRole> query)
        {
            return query
                .Include(ma => ma.User)
                .Include(ma => ma.Role);
        }

        public static IQueryable<Role> IncludeRole(this IQueryable<Role> query)
        {
            return query
                .Include(ma => ma.UserRole);
        }
    }
}
