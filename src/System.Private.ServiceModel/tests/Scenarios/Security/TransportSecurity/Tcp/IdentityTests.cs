﻿using System;
using System.Collections;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using Xunit;

using Infrastructure.Common;

public static class IdentityTests
{
    [Fact]
    [OuterLoop]
    // The product code will check the Dns identity from the server and throw if it does not match what is specified in DnsEndpointIdentity
    public static void VerifyServiceIdentityMatchDnsEndpointIdentity()
    {
        string testString = "Hello";

        NetTcpBinding binding = new NetTcpBinding();
        binding.Security.Mode = SecurityMode.Transport;
        binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

        EndpointAddress endpointAddress = new EndpointAddress(new Uri(Endpoints.Tcp_VerifyDNS_Address), new DnsEndpointIdentity(Endpoints.Tcp_VerifyDNS_HostName));
        ChannelFactory<IWcfService> factory = new ChannelFactory<IWcfService>(binding, endpointAddress);
        IWcfService serviceProxy = factory.CreateChannel();

        try
        {
            var result = serviceProxy.Echo(testString);
            Assert.Equal(testString, result);

            factory.Close();
        }
        finally
        {
            ScenarioTestHelpers.CloseCommunicationObjects(factory);
        }
    }

    [Fact]
    [OuterLoop]
    // Verify product throws MessageSecurityException when the Dns identity from the server does not match the expectation
    public static void ServiceIdentityNotMatch_Throw_MessageSecurityException()
    {
        string testString = "Hello";

        NetTcpBinding binding = new NetTcpBinding();
        binding.Security.Mode = SecurityMode.Transport;
        binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

        EndpointAddress endpointAddress = new EndpointAddress(new Uri(Endpoints.Tcp_VerifyDNS_Address), new DnsEndpointIdentity("wrongone"));
        ChannelFactory<IWcfService> factory = new ChannelFactory<IWcfService>(binding, endpointAddress);
        IWcfService serviceProxy = factory.CreateChannel();

        try
        {
            var exception = Assert.Throws<MessageSecurityException>(() =>
            {
                var result = serviceProxy.Echo(testString);
                Assert.Equal(testString, result);
            });

            Assert.Contains(Endpoints.Tcp_VerifyDNS_HostName, exception.Message);
        }
        finally
        {
            ScenarioTestHelpers.CloseCommunicationObjects(factory);
        }
    }

}
