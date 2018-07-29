using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace FormFlowAdvanced.Dialogs
{
    /// <summary>
    /// A Dialog to rerieve a user token for a configured OAuth connection
    /// This Dialog will first attempt to rerieve the user token from the Azure Bot Service
    /// If the Azure Bot Service does not already have a token, the GetTokenDialog will send
    /// the user an OAuthCard.
    /// The GetTokenDialog will then wait for either the user to come back, or for the user to send
    /// a validation code. The Dialog will attempt to exchange whatever response is sent for the 
    /// user token. If successful, the dialog will return the token and otherwise will retry the
    /// specified number of times.
    /// </summary>
    [Serializable]
    public class GetTokenDialog : IDialog<GetTokenResponse>
    {
        private string _connectionName;
        private string _buttonLabel;
        private string _signInMessage;
        private int _retries;
        private string _retryMessage;

        public GetTokenDialog(string connectionName, string signInMessage, string buttonLabel, int retries = 0, string retryMessage = null)
        {
            _connectionName = connectionName;
            _signInMessage = signInMessage;
            _buttonLabel = buttonLabel;
            _retries = retries;
            _retryMessage = retryMessage;
        }

        public async Task StartAsync(IDialogContext context)
        {
            // First ask Bot Service if it already has a token for this user
            var token = await context.GetUserTokenAsync(_connectionName);
            if (token != null)
            {
                context.Done(new GetTokenResponse() { Token = token.Token });
            }
            else
            {
                // If Bot Service does not have a token, send an OAuth card to sign in
                await SendOAuthCardAsync(context, (Activity)context.Activity);
            }
        }

        private async Task SendOAuthCardAsync(IDialogContext context, Activity activity)
        {
            if (activity.ChannelId == ChannelIds.Msteams)
            {

                var reply = activity.CreateReply();

                var client = activity.GetOAuthClient();

                var link = await client.OAuthApi.GetSignInLinkAsync(activity, _connectionName);

                reply.Attachments = new List<Attachment>() {

                    new Attachment()

                    {

                        ContentType = SigninCard.ContentType,

                        Content = new SigninCard()

                        {

                            Text = _signInMessage,

                            Buttons = new CardAction[]

                            {

                                new CardAction() { Title = _buttonLabel, Value = link, Type = ActionTypes.OpenUrl }

                            },

                        }

                    }

                };

                await context.PostAsync(reply);

            }

            else

            {

                var reply = await activity.CreateOAuthReplyAsync(_connectionName, _signInMessage, _buttonLabel);

                await context.PostAsync(reply);

            }

            context.Wait(WaitForToken);

        }

        //private async Task SendOAuthCardAsync(IDialogContext context, Activity activity)
        //{
        //    var reply = await activity.CreateOAuthReplyAsync(_connectionName, _signInMessage, _buttonLabel);
        //    await context.PostAsync(reply);
        //    context.Wait(WaitForToken);
        //}

        private async Task WaitForToken(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var tokenResponse = activity.ReadTokenResponseContent();
            string verificationCode = null;
            if (tokenResponse != null)
            {
                context.Done(new GetTokenResponse() { Token = tokenResponse.Token });
                return;
            }
            else if (activity.IsTeamsVerificationInvoke())
            {
                JObject value = activity.Value as JObject;
                if (value != null)
                {
                    verificationCode = (string)(value["state"]);
                }
            }
            else if (!string.IsNullOrEmpty(activity.Text))
            {
                verificationCode = activity.Text;
            }

            tokenResponse = await context.GetUserTokenAsync(_connectionName, verificationCode);
            if (tokenResponse != null)
            {
                context.Done(new GetTokenResponse() { Token = tokenResponse.Token });
                return;
            }

            // decide whether to retry or not
            if (_retries > 0)
            {
                _retries--;
                await context.PostAsync(_retryMessage);
                await SendOAuthCardAsync(context, activity);
            }
            else
            {
                context.Done(new GetTokenResponse() { NonTokenResponse = activity.Text });
                return;
            }
        }
    }

    /// <summary>
    /// Result object from the GetTokenDialog
    /// If the GetToken action is successful in retrieving a user token, the GetTokenDialog will be populated with the Token property
    /// If the GetToken action is unsuccessful in retrieving a user token, the GetTokenDialog will be populated with the NonTokenResponse property
    /// </summary>
    public class GetTokenResponse
    {
        /// <summary>
        /// The user token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The text that the user typed when the GetTokenDialog is unable to retrieve a user token
        /// </summary>
        public string NonTokenResponse { get; set; }
    }
}