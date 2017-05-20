using System;
using Akka.Actor;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using static Backend.PageActor;
using System.Threading.Tasks;

namespace Backend
{
    /// <summary>
    /// * Represents simple operation - check VATIN - on page ppuslugi.mf.gov.pl.
    /// * Controls lifecycle of browser instance: creates a new browser instance on start, close them on exit
    /// </summary>
    public sealed class PageActor : FSM<States, dynamic>, IWithUnboundedStash
    {
        public enum States
        {
            /// <summary>
            /// Initial state of the Actor.
            /// </summary>
            Initialized,

            /// <summary>
            /// Linked browser (managed by the agent) is ready for use.
            /// Actor is ready for <see cref="VatinPageRequested"/>
            /// </summary>
            Operational,

            /// <summary>
            /// Someone requested to keed to check VATIN number.
            /// </summary>
            VatinPageRequested
        }


        public IStash Stash { get; set; }

        /// <summary>
        /// Request about validation of a subject with provided VATIN.
        /// The message initiate work with service ppuslugi. Every other CheckVatinAsk
        /// will be stashed as long as the Actor will be in Operational state.
        /// </summary>
        public sealed class CheckVatinAsk
        {
            public CheckVatinAsk(string vatin, DateTime now)
            {
                this.Vatin = vatin;
                this.Now = now;
            }
            public string Vatin { get; }
            public DateTime Now { get; }
        }

        /// <summary>
        /// Checking VATIN has finished, current status is visible in browser.
        /// 
        /// Returned 'CheckVatinReply' contains some data recognized from the page.
        /// Important is recognition about if operation has finished with success or not.
        /// </summary>
        public sealed class CheckVatinReply
        {
            public CheckVatinReply(bool done, string screenshot)
            {
                Done = done;
                Screenshot = screenshot;
            }
            public bool Done { get; set; }
            public string Screenshot { get; set; }
        }

        public sealed class BrowserInitialized
        {
            public bool Success { get; private set; }
            public BrowserInitialized(bool success)
            {
                Success = success;
            }
        }

        public sealed class GoToCheckkVatinPageFinished
        {
            public GoToCheckkVatinPageFinished(bool success)
            {
                Success = success;
            }
            public bool Success { get; private set; }
        }

        private IBrowser browser;

        public PageActor(Func<IBrowser> browserFactory)
        {
            browser = browserFactory();

            base.StartWith(States.Initialized, null);

            When(States.Initialized, @event =>
            {
                if (@event.FsmEvent is BrowserInitialized msg && msg.Success)
                {
                    return GoTo(States.Operational);
                }
                else
                {
                    Self.Tell(PoisonPill.Instance);
                    return Stay();
                }
            });

            When(States.Operational, @event =>
            {
                switch (@event.FsmEvent)
                {
                    case CheckVatinAsk msg:
                        // Operational state + CheckVatinAsk starts check VATIN.
                        // Let's remember who requested the check to return them later checking result.
                        return GoTo(States.VatinPageRequested).Using(Sender);
                    default:
                        return Stay();
                }
            });

            WhenUnhandled(@event =>
            {
                switch (@event.FsmEvent)
                {
                    // if new request is coming, lets stash it to postponed processing.
                    // Will unstash all messages when state is Operational
                    case CheckVatinAsk msg:
                        Stash.Stash();
                        return Stay();
                    default:
                        return Stay();
                }
            });

            OnTransition((tfrom, tto) =>
            {
                if (tfrom == States.Operational && tto == States.VatinPageRequested)
                {
                }
            });

            Initialize();
        }

        protected override void PreStart()
        {
            browser
                .Initialize()
                .ContinueWith(t => new BrowserInitialized(t.Status == TaskStatus.RanToCompletion))
                .PipeTo(Self);

            //browser
            //    .GoToUrl("https://ppuslugi.mf.gov.pl/?link=VAT")
            //    .ContinueWith(t => new GoToCheckkVatinPageFinished(t.IsCompleted && !t.IsFaulted))
            //    .PipeTo(Self);

            base.PreStart();
        }

        protected override void PostStop()
        {
            browser.Dispose();

            base.PostStop();
        }
    }
}
