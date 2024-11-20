using Core.Data;
using Core.Domain.Google;
using Core.Domain.Meta;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using Service.Implementations.Meta;
using Service.Interfaces.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementations.Google
{
    public class GoogleService : IGoogleService
    {
        private readonly HttpClient _httpClient;
        private readonly Repository<Context> _repository;

        public GoogleService(HttpClient httpClient)
        {
            _repository = new Repository<Context>(new Context());
            _httpClient = httpClient;
        }

        #region Admin

        public int AddGoogleApp(int userId, string redirectUrl, string appId, string appSecret)
        {
            var user = GetUserById(userId);
            if (user != null)
            {
                var googleApp = new GoogleApp
                {
                    UserId = userId,
                    AppId = appId,
                    AppSecret = appSecret,
                    RedirectUrl = redirectUrl,
                    InsertedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _repository.Save(googleApp);
                return googleApp.Id;
            }
            return 0;
        }

        public int UpdateGoogleApp(int id, string redirectUrl, string appId, string appSecret)
        {
            var googleApp = GetGoogleAppById(id);
            if (googleApp != null)
            {
                googleApp.RedirectUrl = redirectUrl;
                googleApp.AppId = appId;
                googleApp.AppSecret = appSecret;
                googleApp.UpdateDate = DateTime.UtcNow;

                _repository.Update(googleApp);
                return googleApp.Id;
            }
            return 0;
        }

        public int IsActiveGoogleApp(int id)
        {
            var googleApp = GetGoogleAppById(id);
            if (googleApp != null)
            {
                googleApp.IsActive = !googleApp.IsActive;
                googleApp.UpdateDate = DateTime.UtcNow;

                _repository.Update(googleApp);
                return googleApp.Id;
            }
            return 0;
        }

        public int IsDeletedGoogleApp(int id)
        {
            var googleApp = GetGoogleAppById(id);
            if (googleApp != null)
            {
                googleApp.IsDeleted = !googleApp.IsDeleted;
                googleApp.UpdateDate = DateTime.UtcNow;

                _repository.Update(googleApp);
                return googleApp.Id;
            }
            return 0;
        }

        public int AddGoogleAuthorizationCode(int googleAppId, int userId, string authorizationCode)
        {
            var googleApp = GetGoogleAppById(googleAppId);
            if (googleApp != null)
            {
                var googleAuthorizationCode = new GoogleAuthorizationCode
                {
                    AuthorizationCode = authorizationCode,
                    GoogleAppId = googleAppId,
                    UserId = userId,
                    InsertedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _repository.Save(googleAuthorizationCode);
                return googleAuthorizationCode.Id;
            }
            return 0;
        }

        public int AddGoogleAccessToken(int googleAppId, int userId, string accessToken, string refreshToken, int expiresIn, string scope, string tokenType)
        {
            var googleApp = GetGoogleAppById(googleAppId);
            if (googleApp != null)
            {
                var googleAccessToken = new GoogleAccessToken
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = expiresIn,
                    Scope = scope,
                    TokenType = tokenType,
                    GoogleAppId = googleAppId,
                    UserId = userId,
                    InsertedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _repository.Save(googleAccessToken);
                return googleAccessToken.Id;
            }
            return 0;
        }

        public GoogleApp GetGoogleAppById(int id)
        {
            return _repository.GetById<GoogleApp>(id);
        }

        public GoogleApp GetGoogleApp(int userId)
        {
            var data = _repository.FilterAsQueryable<GoogleApp>(p => p.IsActive && !p.IsDeleted && p.User.Id.Equals(userId)).IncludeGoogleApp().FirstOrDefault();
            return data;
        }

        public GoogleAuthorizationCode GetGoogleAuthorizationCode(int userId)
        {
            var data = _repository.FilterAsQueryable<GoogleAuthorizationCode>(p => p.IsActive && !p.IsDeleted && p.User.Id.Equals(userId)).IncludeGoogleAuthorizationCode().FirstOrDefault();
            return data;
        }

        public GoogleAccessToken GetGoogleAccessToken(int userId)
        {
            var data = _repository.FilterAsQueryable<GoogleAccessToken>(p => p.IsActive && !p.IsDeleted && p.User.Id.Equals(userId)).IncludeGoogleAccessToken().FirstOrDefault();
            return data;
        }

        public Core.Domain.User.User GetUserById(int id)
        {
            return _repository.GetById<Core.Domain.User.User>(id);
        }

        #endregion
    }

    public static class GoogleAppExtensions
    {
        public static IQueryable<GoogleApp> IncludeGoogleApp(this IQueryable<GoogleApp> query)
        {
            return query
                .Include(ma => ma.User)
                .Include(ma => ma.GoogleAuthorizationCode)
                .Include(ma => ma.GoogleAccessToken);
        }

        public static IQueryable<GoogleAuthorizationCode> IncludeGoogleAuthorizationCode(this IQueryable<GoogleAuthorizationCode> query)
        {
            return query
                .Include(ma => ma.GoogleApp)
                .Include(ma => ma.User)
                .Include(ma => ma.GoogleApp.GoogleAccessToken);
        }

        public static IQueryable<GoogleAccessToken> IncludeGoogleAccessToken(this IQueryable<GoogleAccessToken> query)
        {
            return query
                .Include(ma => ma.GoogleApp)
                .Include(ma => ma.User)
                .Include(ma => ma.GoogleApp.GoogleAuthorizationCode);
        }
    }
}
