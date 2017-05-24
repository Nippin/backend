using System;
using Akka.Actor;
using static Backend.PageActor;
using System.Threading.Tasks;

namespace Backend
{
    /// <summary>
    /// * Represents simple operation - check VATIN - on page ppuslugi.mf.gov.pl.
    /// * Controls lifecycle of browser instance: creates a new browser instance on start, close them on exit
    /// </summary>
    public sealed class PageActor : FSM<States, (IActorRef Requestor, string Vatin, DateTime Requested)>, IWithUnboundedStash
    {
        public enum States
        {
            /// <summary>
            /// Initial state of the Actor. 
            /// 
            /// Actor need to initialize connection with Browser to continue work.
            /// </summary>
            Initialized,

            /// <summary>
            /// Linked browser (managed by the agent) is ready for use.
            /// Actor is ready for <see cref="Working"/>
            /// </summary>
            Operational,

            /// <summary>
            /// Someone requested to check VATIN number, operation in progress.
            /// 
            /// When finished, Actor will go back to <see cref="Operational"/> state.
            /// </summary>
            Working
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

        public sealed class PageLocatingResult<TPage>
            where TPage : IPage
        {
            public bool Success { get; private set; }
            public TPage Page { get; set; }

            public PageLocatingResult(bool success, TPage page)
            {
                Success = success;
                Page = page;
            }
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

            base.StartWith(States.Initialized, (null, null, DateTime.Now));

            When(States.Initialized, @event =>
            {
                switch (@event.FsmEvent)
                {
                    case BrowserInitialized msg when msg.Success:
                        browser
                            .GoToUrl("https://ppuslugi.mf.gov.pl/?link=VAT")
                            .ContinueWith(it => browser.Expect<CheckVatinQueryPage>())
                            .Unwrap()
                            .ContinueWith(it =>
                            {
                                var pageFound = it.Status == TaskStatus.RanToCompletion;
                                var page = pageFound ? it.Result : null;
                                return new PageLocatingResult<CheckVatinQueryPage>(pageFound, page);
                            })
                            .PipeTo(Self);

                        return Stay();

                    case PageLocatingResult<CheckVatinQueryPage> msg when msg.Success:
                        // Let's restore all message fro mStash - it is safe place for client requests
                        // which were postponed (if any exists) antil now
                        Stash.UnstashAll();

                        // now it's tim to go to Operational state where it'll be waiting for requests.
                        return GoTo(States.Operational);

                    case PageLocatingResult<CheckVatinQueryPage> msg1 when !msg1.Success:
                    case BrowserInitialized msg2 when !msg2.Success:
                        Self.Tell(PoisonPill.Instance);
                        return Stay();

                    default:
                        return null;
                }
            });

            When(States.Operational, @event =>
            {
                switch (@event.FsmEvent)
                {
                    case CheckVatinAsk msg:
                        // Operational state + CheckVatinAsk starts check VATIN.
                        // Let's remember who requested the check to return them later checking result.
                        browser
                            .Expect<CheckVatinQueryPage>()
                            .ContinueWith(it =>
                            {
                                var pageFound = it.Status == TaskStatus.RanToCompletion;
                                var page = pageFound ? it.Result : null;
                                return new PageLocatingResult<CheckVatinQueryPage>(pageFound, page);
                            })
                            .PipeTo(Self);

                        return GoTo(States.Working).Using((Sender, msg.Vatin, msg.Now));
                    default:
                        return null;
                }
            });

            When(States.Working, @event =>
            {
                switch (@event.FsmEvent)
                {
                    case PageLocatingResult<CheckVatinQueryPage> msg when msg.Success:
                        msg.Page.Vatin(StateData.Vatin);
                        msg.Page.Submit();

                        browser.Expect<CheckVatinResultPage>()
                            .ContinueWith(it =>
                            {
                                var pageFound = it.Status == TaskStatus.RanToCompletion;
                                var page = pageFound ? it.Result : null;
                                return new PageLocatingResult<CheckVatinResultPage>(pageFound, page);
                            })
                            .PipeTo(Self);

                        return Stay();
                    
                    case PageLocatingResult<CheckVatinResultPage> msg when msg.Success:
                        var screenshot = msg.Page.Print();
                        StateData.Requestor.Tell(new CheckVatinReply(true, screenshot));
                        msg.Page.Clear();
                        return GoTo(States.Operational);

                    case PageLocatingResult<CheckVatinResultPage> msg when !msg.Success:
                        return GoTo(States.Operational);

                    case PageLocatingResult<CheckVatinQueryPage> msg when !msg.Success:
                        return GoTo(States.Operational);

                    default:
                        return null;
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

            Initialize();
        }

        protected override void PreStart()
        {
            browser
                .Initialize()
                .ContinueWith(t => new BrowserInitialized(t.Status == TaskStatus.RanToCompletion))
                .PipeTo(Self);

            base.PreStart();
        }

        protected override void PostStop()
        {
            browser.Dispose();

            base.PostStop();
        }
    }
}
