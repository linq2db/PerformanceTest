using System;
using System.ServiceModel;
using System.ServiceModel.Description;

using LinqToDB;
using LinqToDB.Remote;
using LinqToDB.Remote.Wcf;

namespace Tests.L2DB
{
	using DataModel;

	public class LoWcfContext : WcfDataContext
	{
		public LoWcfContext(bool trackChanges) : base(
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
		}

		static int IP = 23654;

		public static ServiceHost Host;

		public static void OpenHost()
		{
			if (Host != null)
				return;

			ServiceHost host = new ServiceHost(new WcfLinqService(new LinqService { AllowUpdates = true }, true), new Uri("net.tcp://localhost:" + ++IP));

			host.Description.Behaviors.Add(new ServiceMetadataBehavior());
			host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
			host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
			host.AddServiceEndpoint(
				typeof(IWcfLinqService),
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

		public ITable<Narrow>     Narrows     => this.GetTable<Narrow>();
		public ITable<NarrowLong> NarrowLongs => this.GetTable<NarrowLong>();
		public ITable<WideLong>   WideLongs   => this.GetTable<WideLong>();
	}
}
