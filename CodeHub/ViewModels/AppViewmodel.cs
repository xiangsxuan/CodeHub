﻿using CodeHub.Helpers;
using CodeHub.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.System.Profile;
using Windows.UI.Popups;

namespace CodeHub.ViewModels
{
    public class AppViewmodel : ViewModelBase
    {
        #region properties
        public bool _isLoggedin;
        public bool IsLoggedin
        {
            get => _isLoggedin;
            set => Set(() => IsLoggedin, ref _isLoggedin, value);
        }

        public bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(() => IsLoading, ref _isLoading, value);
        }

        public User _user;
        public User User
        {
            get => _user;
            set => Set(() => User, ref _user, value);
        }

        public bool _IsNotificationsUnread;
        public bool IsNotificationsUnread
        {
            get => _IsNotificationsUnread;
            set => Set(() => IsNotificationsUnread, ref _IsNotificationsUnread, value);
        }

        private int _NumberOfAllNotifications;
        public int NumberOfAllNotifications
        {
            get => _NumberOfAllNotifications;
            set => Set(() => NumberOfAllNotifications, ref _NumberOfAllNotifications, value);
        }

        private int _NumberOfParticipatingNotifications;
        public int NumberOfParticipatingNotifications
        {
            get => _NumberOfParticipatingNotifications;
            set => Set(() => NumberOfParticipatingNotifications, ref _NumberOfParticipatingNotifications, value);
        }

        private int _NumberOfUnreadNotifications;
        public int NumberOfUnreadNotifications
        {
            get => _NumberOfUnreadNotifications;
            set => Set(() => NumberOfUnreadNotifications, ref _NumberOfUnreadNotifications, value);
        }

        private bool _isDesktopAdsVisible;
        public bool IsDesktopAdsVisible
        {
            get => _isDesktopAdsVisible;
            set => Set(() => IsDesktopAdsVisible, ref _isDesktopAdsVisible, value);
        }

        private bool _isMobileAdsVisible;
        public bool IsMobileAdsVisible
        {
            get => _isMobileAdsVisible;
            set => Set(() => IsMobileAdsVisible, ref _isMobileAdsVisible, value);
        }

        public string WhatsNewText
              => "Hi all! \n v2.5.0\n\n\x2022 CodeHub is back with major bug fixes. Lots of new features and improvemnents to come soon.\n\x2022  \n\n NOTE: Please update Windows to Build 17763 or above to get latest CodeHub updates.";

        private string _AllString = $" ({NotificationsViewmodel.AllNotifications?.Count ?? 0})";
        public string AllString
        {
            get => _AllString;
            set => Set(() => AllString, ref _AllString, value);
        }
        public string _ParticipatingString = $" ({NotificationsViewmodel.ParticipatingNotifications?.Count ?? 0})";
        public string ParticipatingString
        {
            get => _ParticipatingString;
            set => Set(() => ParticipatingString, ref _ParticipatingString, value);
        }
        public string _UnreadString = $" ({UnreadNotifications?.Count ?? 0})";
        public string UnreadString
        {
            get => _UnreadString;
            protected set { Set(() => UnreadString, ref _UnreadString, value); }
        }

        public static ObservableCollection<Notification> UnreadNotifications { get; set; }
        #endregion

        private const string donateFirstAddOnId = "9pd0r1dxkt8j";
        private const string donateSecondAddOnId = "9msvqcz4pbws";
        private const string donateThirdAddOnId = "9n571g3nr2cs";
        private const string donateFourthAddOnId = "9nsmgzx3p43x";
        private const string donateFifthAddOnId = "9phrhpvhscdv";
        private const string donateSixthAddOnId = "9nnqdq0kq21j";

        public AppViewmodel()
        {
            UnreadNotifications = UnreadNotifications ?? new ObservableCollection<Notification>();
        }

        public async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(e.Link));
            }
            catch (UriFormatException)
            {
                MessageDialog dialog = new MessageDialog("Incorrect URI Format");
                await dialog.ShowAsync();
            }
        }

        public async Task Navigate(Type pageType)
        {
            await SimpleIoc.Default.GetInstance<IAsyncNavigationService>().NavigateAsync(pageType, User);
        }

        public async Task Navigate<TType, TViewModel>(TType pageType, TViewModel viewModel)
            where TType : Windows.UI.Xaml.Controls.Page
            where TViewModel : AppViewmodel
        {
            await Navigate(typeof(TType), viewModel);
        }

        public async Task Navigate<TViewModel>(Type pageType, TViewModel viewModel)
            where TViewModel : AppViewmodel
        {
            await SimpleIoc.Default.GetInstance<IAsyncNavigationService>().NavigateAsync(pageType, viewModel);
        }

        public async Task GoBack()
        {
            await SimpleIoc.Default.GetInstance<IAsyncNavigationService>().GoBackAsync();
        }

        public void UpdateAllNotificationIndicator(int count)
        {
            NumberOfAllNotifications = count;
            AllString = $" ({NumberOfAllNotifications})";
        }

        public void UpdateParticipatingNotificationIndicator(int count)
        {
            NumberOfParticipatingNotifications = count;
            ParticipatingString = $" ({NumberOfParticipatingNotifications})";
        }

        public void UpdateUnreadNotificationIndicator(int count)
        {
            IsNotificationsUnread = count > 0;
            NumberOfUnreadNotifications = count;
            UnreadString = $" ({NumberOfUnreadNotifications})";
        }

        public async Task<bool> HasAlreadyDonated()
        {
            try
            {
                if (SettingsService.Get<bool>(SettingsKeys.HasUserDonated))
                {
                    return true;
                }
                else
                {
                    StoreContext WindowsStore = StoreContext.GetDefault();

                    string[] productKinds = { "Durable" };
                    var filterList = new List<string>(productKinds);

                    var queryResult = await WindowsStore.GetUserCollectionAsync(filterList);

                    if (queryResult.ExtendedError != null)
                    {
                        return false;
                    }

                    foreach (KeyValuePair<string, StoreProduct> item in queryResult.Products)
                    {
                        if (item.Value != null)
                        {
                            if (item.Value.IsInUserCollection)
                            {
                                SettingsService.Save(SettingsKeys.HasUserDonated, true, true);
                                return true;
                            }
                        }
                        return false;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task ConfigureAdsVisibility()
        {
            if (await HasAlreadyDonated())
            {
                GlobalHelper.HasAlreadyDonated = true;
                ToggleAdsVisiblity();
            }
            else
            {
                SettingsService.Save(SettingsKeys.IsAdsEnabled, true);

                if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {
                    IsMobileAdsVisible = true;
                    IsDesktopAdsVisible = false;
                }
                else
                {
                    IsDesktopAdsVisible = true;
                    IsMobileAdsVisible = false;
                }
            }
        }

        public void ToggleAdsVisiblity()
        {
            if (SettingsService.Get<bool>(SettingsKeys.IsAdsEnabled))
            {
                if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {
                    IsMobileAdsVisible = true;
                    IsDesktopAdsVisible = false;
                }
                else
                {
                    IsDesktopAdsVisible = true;
                    IsMobileAdsVisible = false;
                }
            }
            else
            {
                IsMobileAdsVisible = IsDesktopAdsVisible = false;
            }
        }
        public async void Error(Exception ex)
        {
            await new MessageDialog(ex.ToString(), ex.Message).ShowAsync();
        }
        public async void Error(string message, string title = "Unknown Error")
        {
            await new MessageDialog(message, title).ShowAsync();
        }
    }
}
