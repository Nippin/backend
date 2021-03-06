﻿using System;
using Akka.Actor;
using static Backend.PageActor;
using System.Threading.Tasks;
using System.Threading;
using Nippin;
using Akka.Event;
using System.Reactive.Disposables;

namespace Backend
{
    /// <summary>
    /// * Represents simple operation - check VATIN - on page ppuslugi.mf.gov.pl.
    /// * Controls lifecycle of browser instance: creates a new browser instance on start, close them on exit
    /// </summary>
    public sealed class PageActor : FSM<States, (IActorRef Requestor, string Vatin, DateTime Requested)>, IWithUnboundedStash
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);

        // Contains all disposable elements to allow dispose them with end of lifetime of the current instance.
        private readonly CompositeDisposable disposer = new CompositeDisposable();

        public enum States
        {
            /// <summary>
            /// Initial state of the Actor. 
            /// 
            /// Actor need to initialize connection with Browser to continue work. 
            /// Alternative is suicide because Browser is essential for PageActor.
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
            Working,
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
            public enum VatinPayerStatus
            {
                Unknown = 0,
                IsTaxPayer = 1
            }

            public CheckVatinReply(bool done, string screenshot, VatinPayerStatus status)
            {
                Done = done;
                Screenshot = screenshot;
                Status = status;
            }
            public bool Done { get; private set; }
            public string Screenshot { get; private set; }
            public VatinPayerStatus Status { get; private set; }
        }

        // Heartbeat message means that it is time to touch browser to keep them live.
        public sealed class BrowserNeedBeTouched { }

        /// <summary>
        /// Indicates the underlying browser is ready to work.
        /// </summary>
        public sealed class BrowserInitialized { }

        /// <summary>
        /// Artificial message sent to push message processing
        /// </summary>
        public sealed class ConsumeNext { }

        public sealed class GoToCheckkVatinPageFinished
        {
            public GoToCheckkVatinPageFinished(bool success)
            {
                Success = success;
            }
            public bool Success { get; private set; }
        }

        /// <summary>
        /// Web browser managed byt the current actor instance.
        /// </summary>
        private IBrowser browser;

        public PageActor(Func<IBrowser> browserFactory)
        {
            browser = browserFactory().DisposeWith(disposer);

            base.StartWith(States.Initialized, (null, null, DateTime.Now));

            When(States.Initialized, @event =>
            {
                switch (@event.FsmEvent)
                {
                    case BrowserInitialized msg:
                        browser
                            .GoToUrl("https://ppuslugi.mf.gov.pl/?link=VAT")
                            .ContinueWith(it => browser.Expect<CheckVatinQueryPage>(new CancellationTokenSource(30.Seconds()).Token))
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
                        // Let's restore all message from Stash - it is safe place for client requests
                        // which were postponed (if any exists) antil now
                        Stash.UnstashAll();

                        // now it's tim to go to Operational state where it'll be waiting for requests.
                        return GoTo(States.Operational);

                    case PageLocatingResult<CheckVatinQueryPage> msg1 when !msg1.Success:
                        // We can't open CheckVatinQueryPage so the browser is useless.
                        // The simplest scenario is to kill the actor and 
                        // assume the instance will be replaced with a new instance.
                        // so the first - suicide
                        throw new PageUnavailableException();

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
                            .Expect<CheckVatinQueryPage>(new CancellationTokenSource(30.Seconds()).Token)
                            .ContinueWith(it =>
                            {
                                var pageFound = it.Status == TaskStatus.RanToCompletion;
                                var page = pageFound ? it.Result : null;
                                log.Info($"Page <CheckVatinQueryPage> found: {pageFound}");
                                if (it.IsFaulted)
                                {
                                    log.Info($"Page <CheckVatinQueryPage> exception: {it.Exception.Flatten()}");
                                }
                                return new PageLocatingResult<CheckVatinQueryPage>(pageFound, page);
                            })
                            .PipeTo(Self);

                        return GoTo(States.Working).Using((Sender, msg.Vatin, msg.Now));

                    case ConsumeNext msg:
                        // Let's restore all message from Stash - it is safe place for client requests
                        // which were postponed (if any exists) antil now
                        Stash.UnstashAll();

                        // now it's tim to go to Operational state where it'll be waiting for requests.
                        return GoTo(States.Operational);

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

                        log.Info("Expect <CheckVatinResultPage> ...");
                        browser.Expect<CheckVatinResultPage>(new CancellationTokenSource(30.Seconds()).Token)
                            .ContinueWith(it =>
                            {
                                var pageFound = it.Status == TaskStatus.RanToCompletion;
                                var page = pageFound ? it.Result : null;
                                log.Info($"Page <CheckVatinResultPage> found: {pageFound}");
                                if (it.IsFaulted)
                                {
                                    log.Info($"Page <CheckVatinResultPage> exception: {it.Exception.Flatten()}");
                                }
                                return new PageLocatingResult<CheckVatinResultPage>(pageFound, page);
                            })
                            .PipeTo(Self);

                        return Stay();

                    case PageLocatingResult<CheckVatinResultPage> msg when msg.Success:
                        var screenshot = msg.Page.Print();
                        StateData.Requestor.Tell(new CheckVatinReply(true, screenshot, msg.Page.Status));
                        msg.Page.Clear();

                        // the actor will reconsume all queued messages if is Operational
                        // and browser is already initialized
                        // so let help them to follolow that pattern
                        Self.Tell(new ConsumeNext());
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
                    case BrowserNeedBeTouched msg:
                        browser.Touch();
                        return Stay();
                    default:
                        return Stay();
                }
            });

            OnTransition((from, to) =>
            {
                log.Info($"Transition {this.Self} from {from} to {to}");
            });

            Initialize();
        }

        protected override void PreStart()
        {
            browser.Initialize();

            Self.Tell(new BrowserInitialized());

            Context.System.Scheduler.ScheduleTellRepeatedly(10.Minutes(), 10.Minutes(), Self, new BrowserNeedBeTouched(), ActorRefs.Nobody);

            base.PreStart();
        }

        private void KeepBrowserLive()
        {
        }
        protected override void PostStop()
        {
            disposer.Dispose();

            base.PostStop();
        }
    }
}
