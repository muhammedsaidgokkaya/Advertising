﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Google
{
    public class GoogleAccessToken : BaseEntity
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
        public int GoogleAppId { get; set; }
        public virtual GoogleApp GoogleApp { get; set; }
        public int UserId { get; set; }
        public virtual Core.Domain.User.User User { get; set; }
    }
}