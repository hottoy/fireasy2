﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Localization.Configuration;
using System.Globalization;
using Fireasy.Common.Extensions;
#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif


namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 字符串本地化管理器的工厂。
    /// </summary>
    public static class StringLocalizerFactory
    {
#if NETSTANDARD
        internal static IServiceCollection AddStringLocalizer(this IServiceCollection services)
        {
            var section = ConfigurationUnity.GetSection<StringLocalizerConfigurationSection>();
            if (section == null)
            {
                return services;
            }

            var setting = section.GetDefault();

            if (setting == null)
            {
                services.AddSingleton(typeof(IStringLocalizerManager), WithCulture(DefaultStringLocalizerManager.Instance, section));
            }
            else
            {
                if (setting is ExtendConfigurationSetting extend)
                {
                    setting = extend.Base;
                }

                services.AddSingleton(typeof(IStringLocalizerManager), WithCulture(((StringLocalizerConfigurationSetting)setting).LocalizerType.New<IStringLocalizerManager>(), section));
            }

            return services;
        }
#endif

        /// <summary>
        /// 根据应用程序配置，创建字符串本地化管理器。
        /// </summary>
        /// <param name="configName">应用程序配置项的名称。</param>
        /// <returns><paramref name="configName"/>缺省时，如果应用程序未配置，则为 <see cref="DefaultStringLocalizerManager"/>，否则为配置项对应的 <see cref="IStringLocalizerManager"/> 实例。</returns>
        public static IStringLocalizerManager CreateManager(string configName = null)
        {
            IStringLocalizerManager manager;
            IConfigurationSettingItem setting = null;
            var section = ConfigurationUnity.GetSection<StringLocalizerConfigurationSection>();
            if (section != null && section.Factory != null)
            {
                manager = ConfigurationUnity.Cached<IStringLocalizerManager>($"LocalizerManager_{configName}", () => section.Factory.CreateInstance(configName) as IStringLocalizerManager);
                if (manager != null)
                {
                    return WithCulture(manager, section);
                }
            }

            if (string.IsNullOrEmpty(configName))
            {
                if (section == null || (setting = section.GetDefault()) == null)
                {
                    return WithCulture(DefaultStringLocalizerManager.Instance, section);
                }
            }
            else if (section != null)
            {
                setting = section.GetSetting(configName);
            }

            if (setting == null)
            {
                return null;
            }

            return WithCulture(ConfigurationUnity.Cached<IStringLocalizerManager>($"LocalizerManager_{configName}", () => ConfigurationUnity.CreateInstance<StringLocalizerConfigurationSetting, IStringLocalizerManager>(setting, s => s.LocalizerType)), section);
        }

        private static IStringLocalizerManager WithCulture(IStringLocalizerManager manager, StringLocalizerConfigurationSection section)
        {
            if (section != null && !string.IsNullOrEmpty(section.Culture))
            {
                try
                {
                    manager.CultureInfo = CultureInfo.GetCultureInfo(section.Culture);
                }
                catch (CultureNotFoundException)
                { 
                }
            }

            return manager;
        }
    }
}
