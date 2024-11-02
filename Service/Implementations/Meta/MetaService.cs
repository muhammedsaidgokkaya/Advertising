using Core.Data;
using Core.Domain.Meta;
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

        public async Task<Core.Domain.Meta.MetaUser> GetMetaUser(string accessToken)
        {
            var url = $"https://graph.facebook.com/v13.0/me?access_token={accessToken}";
            var user = await _httpClient.GetFromJsonAsync<Core.Domain.Meta.MetaUser>(url);
            return user;
        }

        public IQueryable<MetaAccess> GetAccessToken(int userId)
        {
            var data = _repository.FilterAsQueryable<MetaAccess>(p => p.IsActive && !p.IsDeleted && p.User.Id.Equals(userId)).IncludeAll();
            return data;
        }

        public async Task<string> GetLongLivedAccessTokenAsync(string appId, string appSecret, string shortLivedToken)
        {
            var url = $"https://graph.facebook.com/v21.0/oauth/access_token" +
                      $"?grant_type=fb_exchange_token&client_id={appId}&client_secret={appSecret}&fb_exchange_token={shortLivedToken}";

            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public static class MetaAccessExtensions
    {
        public static IQueryable<MetaAccess> IncludeAll(this IQueryable<MetaAccess> query)
        {
            return query
                .Include(ma => ma.User)
                .Include(ma => ma.MetaLongAccess);
        }
    }
}
