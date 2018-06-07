using System;
using System.ServiceModel;
using System.ServiceModel.Description;

using LinqToDB;
using LinqToDB.ServiceModel;

namespace Tests.L2DB
{
	using DataModel;

	public class LoWcfContext : ServiceModelDataContext
	{
		public readonly bool NoTracking;

		public LoWcfContext(bool noTracking = true) : base(
			new NetTcpBinding(SecurityMode.None)
			{
				MaxReceivedMessageSize = 10000000,
				MaxBufferPoolSize      = 10000000,
				MaxBufferSize          = 10000000,
				CloseTimeout           = new TimeSpan(00, 01, 00),
				OpenTimeout            = new TimeSpan(00, 01, 00),
				ReceiveTimeout         = new TimeSpan(00, 10, 00),
				SendTimeout            = new TimeSpan(00, 10, 00),
			},
			new EndpointAddress("net.tcp://localhost:" + IP + "/LinqOverWCF"))
		{
			((NetTcpBinding)Binding).ReaderQuotas.MaxStringContentLength = 10000000;

			NoTracking = noTracking;
		}

		static int IP = 23654;

		public static ServiceHost Host;

		public static void OpenHost()
		{
			if (Host != null)
				return;

			ServiceHost host = new ServiceHost(new LinqService { AllowUpdates = true }, new Uri("net.tcp://localhost:" + ++IP));

			host.Description.Behaviors.Add(new ServiceMetadataBehavior());
			host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
			host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
			host.AddServiceEndpoint(
				typeof(ILinqService),
				new NetTcpBinding(SecurityMode.None)
				{
					MaxReceivedMessageSize = 10000000,
					MaxBufferPoolSize      = 10000000,
					MaxBufferSize          = 10000000,
					CloseTimeout           = new TimeSpan(00, 01, 00),
					OpenTimeout            = new TimeSpan(00, 01, 00),
					ReceiveTimeout         = new TimeSpan(00, 10, 00),
					SendTimeout            = new TimeSpan(00, 10, 00),
				},
				"LinqOverWCF");

			host.Open();

			Host = host;
		}

		public ITable<Narrow>        Narrows         => this.GetTable<Narrow>();
		public ITable<NarrowLong>    NarrowLongs     => this.GetTable<NarrowLong>();
		public ITable<WideLong>      WideLongs       => this.GetTable<WideLong>();

		public ITable<Setting>       Settings        => this.GetTable<Setting>();
		public ITable<TestRun>       TestRuns        => this.GetTable<TestRun>();
		public ITable<TestMethod>    TestMethods     => this.GetTable<TestMethod>();
		public ITable<TestStopwatch> TestStopwatches => this.GetTable<TestStopwatch>();
		public ITable<TestResult>    TestResults     => this.GetTable<TestResult>();
	}
}
