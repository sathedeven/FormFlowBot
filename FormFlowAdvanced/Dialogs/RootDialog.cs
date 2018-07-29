using System;
using System.Configuration;
using System.Threading.Tasks;
using FormFowBasic.Auth;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FormFlowAdvanced.Forms;
using Microsoft.Bot.Builder.FormFlow;

namespace FormFlowAdvanced.Dialogs
{

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private static string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];
        //public Task StartAsync(IDialogContext context)
        //{
        //    context.Wait(MessageReceivedAsync);

        //   // return Task.CompletedTask;
        //}

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            context.Call(CreateGetTokenDialog(), ListMe);


            //context.Wait(MessageReceivedAsync);
        }

        private async Task ListMe(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
        {
            var token = await tokenResponse;
            var client = new SimpleGraphClient(token.Token);

            var me = await client.GetMe();
            //var manager = await client.GetManager();

            await context.PostAsync($"Welcome {me.DisplayName}.");

            var form = new FormDialog<TravelRequestForm>(
               new TravelRequestForm(),
               TravelRequestForm.BuildForm,
               FormOptions.PromptInStart
                );
            context.Call<TravelRequestForm>(form, TravelRequestFormComplete);

        }

        private async Task TravelRequestFormComplete(IDialogContext context, IAwaitable<TravelRequestForm> result)
        {
            TravelRequestForm form = null;
            try
            {
                form = await result;
                //context.UserData.

            }
            catch (OperationCanceledException ex)
            {
            }
        }

            private GetTokenDialog CreateGetTokenDialog()
        {
            return new GetTokenDialog(
                ConnectionName,
                $"Please sign in to {ConnectionName} to proceed.",
                "Sign In",
                2,
                "Hmm. Something went wrong, let's try again.");
        }
    }
}