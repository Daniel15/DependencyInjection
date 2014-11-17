﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Framework.DependencyInjection
{
    public class ServiceManifestFacts
    {
        private class ServiceManifest : IServiceManifest
        {
            public ServiceManifest([NotNull] IEnumerable<Type> services)
            {
                Services = services;
            }

            public IEnumerable<Type> Services { get; private set; }
        }

        [Fact]
        public void ImportAddsServices()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddSingleton<IFakeSingletonService, FakeService>();
            var instance = new FakeService();
            fallbackServices.AddInstance<IFakeServiceInstance>(instance);
            fallbackServices.AddTransient<IFakeService, FakeService>();

            fallbackServices.AddInstance<IServiceManifest>(new ServiceManifest(
                new Type[] { typeof(IFakeServiceInstance), typeof(IFakeService), typeof(IFakeSingletonService) }));

            var services = new ServiceCollection();
            services.Import(fallbackServices.BuildServiceProvider());

            // Act
            var provider = services.BuildServiceProvider();
            var singleton = provider.GetRequiredService<IFakeSingletonService>();
            var transient = provider.GetRequiredService<IFakeService>();

            // Assert
            Assert.Equal(singleton, provider.GetRequiredService<IFakeSingletonService>());
            Assert.NotEqual(transient, provider.GetRequiredService<IFakeService>());
            Assert.Equal(instance, provider.GetRequiredService<IFakeServiceInstance>());
        }

        [Fact]
        public void CanHideImportedServices()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            var fallbackInstance = new FakeService();
            fallbackServices.AddInstance<IFakeService>(fallbackInstance);

            var services = new ServiceCollection();
            var realInstance = new FakeService();
            fallbackServices.AddInstance<IServiceManifest>(new ServiceManifest(
                new Type[] { typeof(IFakeService) }));
            services.AddInstance<IFakeService>(realInstance);

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.Equal(realInstance, provider.GetRequiredService<IFakeService>());
        }

        [Fact]
        public void ImportIgnoresAndDoesNotExplodeWithNoManifest()
        {
            // Arrange
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddSingleton<IFakeSingletonService, FakeService>();
            var instance = new FakeService();
            fallbackServices.AddInstance<IFakeServiceInstance>(instance);
            fallbackServices.AddTransient<IFakeService, FakeService>();

            var services = new ServiceCollection();
            services.Import(fallbackServices.BuildServiceProvider());

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.Null(provider.GetService<IFakeSingletonService>());
            Assert.Null(provider.GetService<IFakeService>());
            Assert.Null(provider.GetService<IFakeServiceInstance>());
        }

    }
}