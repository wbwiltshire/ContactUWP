using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeagullConsulting.ContactUWP.Data.POCO;
using SeagullConsulting.ContactUWP.Data.Repository;
using Microsoft.Toolkit.Uwp.UI.Controls;

// Links to Windows Community Toolit (DataGrid): https://github.com/Microsoft/WindowsCommunityToolkit

namespace SeagullConsulting.ContactUWP.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContactsPage : Page, INotifyPropertyChanged
    {
        private ContactRepository repository = null;
        private ILogger logger = null;
        private AppSettingsConfiguration settings = null;
        private ICollection<Contact> entities = new List<Contact>();

        public event PropertyChangedEventHandler PropertyChanged;
        public ICollection<Contact> Contacts {
            get
            {
                return entities.ToList();
            }
            // Raise the PropertyChangedEvent, so the DataGrid will update
            // Note: DataGrid ItemsSource with x:Bind defaults to a Mode of "OneTime" Binding
            //       You must Change Mode to "OneWay" for data updates to work
            set {
                entities = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Contacts"));
            }
        }

        public ContactsPage()
        {
            this.InitializeComponent();

            //Initialize
            IConfigurationRoot config;
            var builder = new ConfigurationBuilder().
                       AddJsonFile("appsettings.json");
            config = builder.Build();
            settings = new AppSettingsConfiguration();
            ConfigurationBinder.Bind(config, settings);

            ILoggerFactory logFactory = new LoggerFactory()
                .AddDebug()
                .AddFile(config.GetSection("Logging"));
            logger = logFactory.CreateLogger(typeof(MainPage));

        }
        
        private async void ContactPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (DBConnection db = new DBConnection(settings.Database.ConnectionString, logger))
                {
                    repository = new ContactRepository(settings, logger, db);
                    Contacts = await repository.FindAllView();
                }
            }
            catch (Exception ex)
            {

                logger.LogError($"Error: {ex.Message}");
            }
        }
    }
}
