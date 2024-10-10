using LearniVerseNew.Models.ApplicationModels.Store_Models;
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

        // Initialize a transaction for cart checkout
        public TransactionInitializeResponse InitializeTransactionForCheckout(string email, decimal cartTotal, string callbackUrl)
        {
            // Convert cart total to kobo (if using NGN or ZAR, multiply by 100)
            int amountInKobo = (int)(cartTotal * 100);

            // Initialize the Paystack transaction
            var request = new TransactionInitializeRequest
            {
                Email = email,
                AmountInKobo = amountInKobo,
                Currency = "ZAR", // Change currency if needed
                CallbackUrl = callbackUrl
            };

            // Call Paystack API to initialize the transaction
            return _api.Transactions.Initialize(request);
        }

        public TransactionVerifyResponse VerifyTransaction(string reference)
        {
            return _api.Transactions.Verify(reference);
        }

        //subscriptions
        public TransactionInitializeResponse InitializeSubscriptionTransaction(string email, string planCode, string callbackUrl)
        {
            var request = new TransactionInitializeRequest
            {
                Email = email,
                AmountInKobo = 10, // This amount will be overridden by the plan code
                Currency = "ZAR",
                CallbackUrl = callbackUrl,
                Plan = planCode // Pass the plan code to subscribe the customer
            };

            return _api.Transactions.Initialize(request);
        }

       

        

    }
}