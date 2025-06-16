using Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.User
{
    public class Payment : BaseEntity
    {
        private string _cardNumber;
        private string _cardHolder;
        private string _cvv;
        private string _expirationDate;

        public string CardNumber
        {
            get => _cardNumber == null ? null : CryptoHelper.Decrypt(_cardNumber);
            set => _cardNumber = string.IsNullOrEmpty(value) ? null : CryptoHelper.Encrypt(value);
        }

        public string CardHolder
        {
            get => _cardHolder == null ? null : CryptoHelper.Decrypt(_cardHolder);
            set => _cardHolder = string.IsNullOrEmpty(value) ? null : CryptoHelper.Encrypt(value);
        }

        public string Cvv
        {
            get => _cvv == null ? null : CryptoHelper.Decrypt(_cvv);
            set => _cvv = string.IsNullOrEmpty(value) ? null : CryptoHelper.Encrypt(value);
        }

        public string ExpirationDate
        {
            get => _expirationDate == null ? null : CryptoHelper.Decrypt(_expirationDate);
            set => _expirationDate = string.IsNullOrEmpty(value) ? null : CryptoHelper.Encrypt(value);
        }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        public string _CardNumber { get => _cardNumber; set => _cardNumber = value; }
        public string _CardHolder { get => _cardHolder; set => _cardHolder = value; }
        public string _Cvv { get => _cvv; set => _cvv = value; }
        public string _ExpirationDate { get => _expirationDate; set => _expirationDate = value; }
    }
}
