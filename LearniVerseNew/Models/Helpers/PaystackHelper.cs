using PayStack.Net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.Helpers
{
    public class PaystackHelper
    {
        private readonly PayStackApi _api;

        public PaystackHelper()
        {
            var secretKey = ConfigurationManager.AppSettings["PayStackSecret"];
            _api = new PayStackApi(secretKey);
        }

        public TransactionInitializeResponse InitializeTransaction(string email, int amountInKobo, string callbackUrl)
        {
            // Initialize a transaction for ZAR
            var request = new TransactionInitializeRequest
            {
                Email = email,
                AmountInKobo = amountInKobo,
                Currency = "ZAR",
                CallbackUrl = callbackUrl
            };

            return _api.Transactions.Initialize(request);
        }

        public TransactionVerifyResponse VerifyTransaction(string reference)
        {
            return _api.Transactions.Verify(reference);
        }
    }
}