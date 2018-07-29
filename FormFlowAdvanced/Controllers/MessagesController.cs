﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using FormFowBasic.Forms;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Linq;
using FormFlowAdvanced.Forms;
using FormFlowAdvanced.Dialogs;
using System.Web.Http.Filters;

namespace FormFowBasic
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>


        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                //await Conversation.SendAsync(activity, () => new TravelLuisDialog());
                //await Conversation.SendAsync(activity, MakeRootDialog);
                await Conversation.SendAsync(activity,() => new RootDialog());


            }
            
            else
            {
               await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                if (message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                {
                    var reply = message.CreateReply("Bonjour");

                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));

                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

            }

            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
               
            }
    
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

           
        }

        internal static IDialog<TravelRequestForm> MakeRootDialog()
        {


            return Chain.From(() => FormDialog.FromForm(TravelRequestForm.BuildForm))
                .Do(async (context, survey) =>
                {
                    try
                    {
                        var completed = await survey;
                       await context.PostAsync("Thanks for your request !");
                    }
                    catch (FormCanceledException<SurveyForm> e)
                    {
                        string reply;
                        if (e.InnerException == null)
                        {
                            reply = "Vous n’avez pas complété l’enquête !";
                        }
                        else
                        {
                            reply = "Erreur. Essayez plus tard !.";
                        }
                        await context.PostAsync(reply);
                    }
                });
        }


    }

    public class TravelExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is NotImplementedException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented);
            }
        }
    }
}