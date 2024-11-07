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

        public int AddAccessToken(int userId, string accessToken, string appId, string appSecret)
        {
            var user = GetUserById(userId);
            if (user != null)
            {
                var metaAccess = new MetaAccess
                {
                    UserId = userId,
                    AccessToken = accessToken,
                    AppId = appId,
                    AppSecret = appSecret,
                    InsertedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _repository.Save(metaAccess);
                return metaAccess.Id;
            }
            return 0;
        }

        public int UpdateAccessToken(int id, string accessToken, string appId, string appSecret)
        {
            var metaAccess = GetMetaAccessById(id);
            if (metaAccess != null)
            {
                metaAccess.AccessToken = accessToken;
                metaAccess.AppId = appId;
                metaAccess.AppSecret = appSecret;
                metaAccess.UpdateDate = DateTime.UtcNow;

                _repository.Update(metaAccess);
                return metaAccess.Id;
            }
            return 0;
        }

        public int IsActiveAccessToken(int id)
        {
            var metaAccess = GetMetaAccessById(id);
            if (metaAccess != null)
            {
                metaAccess.IsActive = !metaAccess.IsActive;
                metaAccess.UpdateDate = DateTime.UtcNow;

                _repository.Update(metaAccess);
                return metaAccess.Id;
            }
            return 0;
        }

        public int IsDeletedAccessToken(int id)
        {
            var metaAccess = GetMetaAccessById(id);
            if (metaAccess != null)
            {
                metaAccess.IsDeleted = !metaAccess.IsDeleted;
                metaAccess.UpdateDate = DateTime.UtcNow;

                _repository.Update(metaAccess);
                return metaAccess.Id;
            }
            return 0;
        }

        public MetaAccess GetMetaAccessById(int id)
        {
            return _repository.GetById<MetaAccess>(id);
        }

        public MetaAccess GetAccessToken(int userId)
        {
            var data = _repository.FilterAsQueryable<MetaAccess>(p => p.IsActive && !p.IsDeleted && p.User.Id.Equals(userId)).IncludeMetaAccess().FirstOrDefault();
            return data;
        }

        public int AddLongAccessToken(int metaAccessId, string accessToken, string tokenType, int expiresIn)
        {
            var metaAccess = GetMetaAccessById(metaAccessId);
            if (metaAccess != null)
            {
                var metaLongAccess = new MetaLongAccess
                {
                    MetaAccessId = metaAccessId,
                    AccessToken = accessToken,
                    TokenType = tokenType,
                    ExpiresIn = expiresIn,
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
            var data = _repository.FilterAsQueryable<MetaLongAccess>(p => p.IsActive && !p.IsDeleted && p.MetaAccess.User.Id.Equals(userId)).IncludeMetaLongAccess().FirstOrDefault();
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
        public static IQueryable<MetaAccess> IncludeMetaAccess(this IQueryable<MetaAccess> query)
        {
            return query
                .Include(ma => ma.User)
                .Include(ma => ma.MetaLongAccess);
        }

        public static IQueryable<MetaLongAccess> IncludeMetaLongAccess(this IQueryable<MetaLongAccess> query)
        {
            return query
                .Include(ma => ma.MetaAccess)
                .Include(ma => ma.MetaAccess.User);
        }
    }
}
