﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using AppRopio.Base.Core.Models.Bundle;
using AppRopio.Base.Core.Models.Navigation;
using AppRopio.Base.Core.Services.Router;
using AppRopio.Base.Core.ViewModels;
using AppRopio.Navigation.Menu.Core.ViewModels.Items;
using AppRopio.Navigation.Menu.Core.ViewModels.Services;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

namespace AppRopio.Navigation.Menu.Core.ViewModels
{
    public class MenuViewModel : BaseViewModel, IMenuViewModel
    {
        #region Commands

        private ICommand _selectionChangedCommand;
        public ICommand SelectionChangedCommand
        {
            get
            {
                return _selectionChangedCommand ?? (_selectionChangedCommand = new MvxCommand<IMenuItemVM>(OnItemSelected));
            }
        }

        #endregion

        #region Properties

        private IMvxViewModel _headerVm;
        public IMvxViewModel HeaderVm
        {
            get
            {
                return _headerVm;
            }
            set
            {
                _headerVm = value;
                RaisePropertyChanged(() => HeaderVm);
            }
        }

        private ObservableCollection<IMenuItemVM> _items;
        public ObservableCollection<IMenuItemVM> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                RaisePropertyChanged(() => Items);
            }
        }

        private IMvxViewModel _footerVm;
        public IMvxViewModel FooterVm
        {
            get
            {
                return _footerVm;
            }
            set
            {
                _footerVm = value;
                RaisePropertyChanged(() => FooterVm);
            }
        }

        public Type DefaultViewModel
        {
            get
            {
                var defaultType = MenuService.DefaultViewModelType();
                MenuSettings.FirstLaunch = false;
                return defaultType;
            }
        }

        #endregion

        #region Services

        private IMenuVmService _menuService;
        protected IMenuVmService MenuService { get { return _menuService ?? (_menuService = Mvx.Resolve<IMenuVmService>()); } }

        protected IRouterService RouterService { get { return Mvx.Resolve<IRouterService>(); } }

        #endregion

        #region Constructor

        public MenuViewModel()
        {
            VmNavigationType = Base.Core.Models.Navigation.NavigationType.None;

            Items = new ObservableCollection<IMenuItemVM>();
        }

        #endregion

        #region Private

        private void LoadContent()
        {
            LoadHeaderVm();

            LoadMenuItems();

            LoadFooterVm();
        }

        private async void LoadHeaderVm()
        {
            try
            {
                HeaderVm = MenuService.LoadHeaderVmIfExist();

                await HeaderVm?.Initialize();
            }
            catch (UnauthorizedAccessException ex)
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", ex.Message);
            }
            catch
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", "Can't load header vm");
            }
        }

        private void LoadMenuItems()
        {
            try
            {
                Items = new ObservableCollection<IMenuItemVM>(MenuService.LoadItemsFromConfigJSON());
            }
            catch (UnauthorizedAccessException ex)
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", ex.Message);
            }
            catch
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", "Can't load menu items");
            }
        }

        private async void LoadFooterVm()
        {
            try
            {
                FooterVm = MenuService.LoadFooterVmIfExist();

                await FooterVm?.Initialize();
            }
            catch (UnauthorizedAccessException ex)
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", ex.Message);
            }
            catch
            {
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "MenuViewModel: ", "Can't load footer vm");
            }
        }

        #endregion

        #region Protected

        protected void OnItemSelected(IMenuItemVM item)
        {
            AnalyticsNotifyingService.NotifyEventIsHandled("menu", "menu_item_selected", item.Type);

            if (!RouterService.NavigatedTo(item.Type, new BaseBundle(NavigationType.ClearAndPush)))
                MvxTrace.Trace(MvvmCross.Platform.Platform.MvxTraceLevel.Error, "Menu", $"Can't navigate to ViewModel of type {item.Type}");
        }

        #endregion

        #region Public

        public override Task Initialize()
        {
            return Task.Run(() => LoadContent());
        }

        #endregion
    }
}
