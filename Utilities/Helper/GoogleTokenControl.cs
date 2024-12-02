using Core.Domain.User;
using Microsoft.Extensions.Logging;
using Service.Implementations.Google;
using Service.Implementations.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Utilities.GoogleData;

namespace Utilities.Helper
{
    public class GoogleTokenControl
    {
        private readonly UserService _userService;
        private readonly GoogleService _googleService;

        public GoogleTokenControl(GoogleService googleService)
        {
            _userService = new UserService();
            _googleService = googleService;
        }

        public string GetControl(int userId)
        {
            var control = _googleService.GetGoogleAccessToken(userId);
            GoogleData googleData = new GoogleData();
            if (control == null)
            {
                var model = _googleService.GetGoogleApp(userId);
                var accessToken = _googleService.GetGoogleAccessTokenControl(userId);
                var refreshToken = googleData.RefreshAccessTokenAdmin(model.AppId, model.AppSecret, accessToken.RefreshToken);
                var newAccessToken = _googleService.UpdateGoogleAccessToken(accessToken.Id, refreshToken.AccessToken, refreshToken.ExpiresIn);
                control = _googleService.GetGoogleAccessToken(userId);
            }
            return control.AccessToken;
        }
    }
}
