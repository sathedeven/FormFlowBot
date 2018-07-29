using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;
using FormFlowAdvanced.Forms;
using Microsoft.Bot.Builder.FormFlow;

namespace FormFlowAdvanced.Dialogs
{
    [Serializable]
    [LuisModel("14348e46-478c-446b-8bcf-725af9c4f1dc", "beed5c3792424628996d5d847c11eae3")]
    public class TravelLuisDialog : LuisDialog<object>
    {
        [LuisIntent("PlanTravel")]
        public async Task TravelHandler(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Welcome to my travel bot.");
            var form = new FormDialog<TravelRequestForm>(
                new TravelRequestForm(),
                TravelRequestForm.BuildForm,
                FormOptions.PromptInStart,
                result.Entities);
            context.Call<TravelRequestForm>(form, TravelRequestFormComplete);
        }

        private async Task TravelRequestFormComplete(IDialogContext context, IAwaitable<TravelRequestForm> result)
        {
            TravelRequestForm form = null;
            try
            {
                form = await result;
                
            }
            catch (OperationCanceledException)
            {
            }
            if (form == null)
            {
                await context.PostAsync("You cancelled the form.");
            }
            else
            {
                //call the TravelRequestForm service to complete the form fill
                var message = $"Thanks! for using our Bot to submit Form Services.";
                await context.PostAsync(message);
            }
            context.Wait(this.MessageReceived);
        }

    }
}