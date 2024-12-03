using Core.Data;
using Core.Domain.Meta;
using Core.Domain.User;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using Service.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementations.Meta
{
    public class MetaService : IMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly Repository<Context> _repository;

        public MetaService(HttpClient httpClient)
        {
            _repository = new Repository<Context>(new Context());
            _httpClient = httpClient;
        }

        #region Admin

        public int AddMetaApp(string appId, string appSecret)
        {
            var metaApp = new MetaApp
            {
                AppId = appId,
                AppSecret = appSecret,
                InsertedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            _repository.Save(metaApp);
            return metaApp.Id;
        }

        public int UpdateMetaApp(int id, string accessToken, string appId, string appSecret)
        {
            var metaApp = GetMetaAppById(id);
            if (metaApp != null)
            {
                metaApp.AppId = appId;
                metaApp.AppSecret = appSecret;
                metaApp.UpdateDate = DateTime.UtcNow;

                _repository.Update(metaApp);
                return metaApp.Id;
            }
            return 0;
        }

        public int IsActiveMetaApp(int id)
        {
            var metaApp = GetMetaAppById(id);
            if (metaApp != null)
            {
                metaApp.IsActive = !metaApp.IsActive;
                metaApp.UpdateDate = DateTime.UtcNow;

                _repository.Update(metaApp);
                return metaApp.Id;
            }
            return 0;
        }

        public int IsDeletedMetaApp(int id)
        {
            var metaApp = GetMetaAppById(id);
            if (metaApp != null)
            {
                metaApp.IsDeleted = !metaApp.IsDeleted;
                metaApp.UpdateDate = DateTime.UtcNow;

                _repository.Update(metaApp);
                return metaApp.Id;
            }
            return 0;
        }

        public MetaApp GetMetaAppById(int id)
        {
            return _repository.GetById<MetaApp>(id);
        }

        public MetaApp GetMetaApp()
        {
            var data = _repository.FilterAsQueryable<MetaApp>(p => p.IsActive && !p.IsDeleted).IncludeMetaApp().FirstOrDefault();
            return data;
        }

        public int AddLongAccessToken(int metaAppId, int userId, string accessToken, string tokenType, int expiresIn)
        {
            var metaApp = GetMetaAppById(metaAppId);
            if (metaApp != null)
            {
                var metaLongAccess = new MetaLongAccess
                {
                    MetaAppId = metaAppId,
                    AccessToken = accessToken,
                    TokenType = tokenType,
                    ExpiresIn = expiresIn,
                    UserId = userId,
                    InsertedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _repository.Save(metaLongAccess);
                return metaLongAccess.Id;
            }
            return 0;
        }

        public MetaLongAccess GetLongAccessToken(int userId)
        {
            var data = _repository.FilterAsQueryable<MetaLongAccess>(p => p.IsActive && !p.IsDeleted && p.User.Id.Equals(userId)).IncludeMetaLongAccess().FirstOrDefault();
            return data;
        }

        public Core.Domain.User.User GetUserById(int id)
        {
            return _repository.GetById<Core.Domain.User.User>(id);
        }

        #endregion
    }

    public static class MetaAccessExtensions
    {
        public static IQueryable<MetaApp> IncludeMetaApp(this IQueryable<MetaApp> query)
        {
            return query
                .Include(ma => ma.MetaLongAccess);
        }

        public static IQueryable<MetaLongAccess> IncludeMetaLongAccess(this IQueryable<MetaLongAccess> query)
        {
            return query
                .Include(ma => ma.MetaApp)
                .Include(ma => ma.User);
        }
    }
}
