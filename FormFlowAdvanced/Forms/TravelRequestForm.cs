using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;

namespace FormFlowAdvanced.Forms
{
    [Serializable]
    public class TravelRequestForm
    {
        public ModeOptions ModeOfTravel { get; set; }
        public TravelOptions Options { get; set; }
        public string DepartureCity { get; set; }
        public string DestinationCity { get; set; }
        public DateTime? TravelDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string DepartureCity2 { get; set; }
        public string DestinationCity2 { get; set; }
        public DateTime? TravelDate2 { get; set; }
        public bool IsHotelRequired { get; set; }

        public enum TravelOptions
        {
            Return = 1,
            OneWay,
            TwoWay
        }

        public enum ModeOptions
        {
            [Terms("Plane", "Air", "Flight")]
            Flight = 1,
            [Terms("Rail", "Train")]
            Train,
            Taxi
        };

        public static IForm<TravelRequestForm> BuildForm()
        {
            OnCompletionAsyncDelegate<TravelRequestForm> wrapUpRequest = async (context, state) =>
            {
                string wrapUpMessage = "Your Travel Request from " + state.DepartureCity + " to "+ state.DepartureCity + "is saved";

                var msg = context.MakeMessage();
                msg.Text = wrapUpMessage;

                await context.PostAsync(msg);

            };

            return new FormBuilder<TravelRequestForm>()
                    .Message("Welecome to Travel Bot. Please choose an option")
                    .Field(nameof(ModeOfTravel))
                    .Field(nameof(Options))
                    .Field(nameof(DepartureCity))
                    .Field(nameof(DestinationCity))
                    .Field(nameof(TravelDate))
                    // .Field(new FieldReflector<TravelRequestForm>(nameof(TravelDate))
                    //.SetNext(SetNextAfterTravelDate))
                    .Field(nameof(ReturnDate), state => (state.Options == TravelOptions.Return))
                    .Field(nameof(DepartureCity2), state => (state.Options == TravelOptions.TwoWay))
                    .Field(nameof(DestinationCity2), state => (state.Options == TravelOptions.TwoWay))
                    .Field(nameof(TravelDate2), state => (state.Options == TravelOptions.TwoWay))
                    .Field(nameof(IsHotelRequired))

                    .Confirm("Do you confirm your selection ? {*}")
                    .OnCompletion(wrapUpRequest)
                    .Build();
        }


        private static NextStep SetNextAfterTravelDate(object value, TravelRequestForm state)
        {
            if (state.Options != TravelOptions.OneWay)
            {
                return new NextStep(new[] { nameof(ReturnDate) });
            }
            else
            {
                return new NextStep(new[] { nameof(IsHotelRequired) });
            }
        }

        private static NextStep EvaluateReturnDateStep(object value, TravelRequestForm state)
        {
            return new NextStep();
        }


    }
}