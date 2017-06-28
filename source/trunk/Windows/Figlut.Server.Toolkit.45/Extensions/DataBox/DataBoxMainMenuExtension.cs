﻿namespace Figlut.Server.Toolkit.Extensions.DataBox
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Figlut.Server.Toolkit.Extensions.DataBox.Events.MainMenu;
    using Figlut.Server.Toolkit.Extensions.DataBox;
    using Figlut.Server.Toolkit.Extensions.ExtensionManaged;
    using Figlut.Server.Toolkit.Extensions.DataBox.Managers;

    #endregion //Using Directives

    public abstract class DataBoxMainMenuExtension
    {
        #region Constructors

        public DataBoxMainMenuExtension()
        {
            _extensionManagedMainMenu = new ExtensionManagedMainMenu();
            AddExtensionManagedMenuItems();
            SubscribeToAddedMenuItemsEvents();
        }

        #endregion //Constructors

        #region Fields

        private ExtensionManagedMainMenu _extensionManagedMainMenu;
        private DataBoxManager _dataBoxManager;

        #endregion //Fields

        #region Properties

        public ExtensionManagedMainMenu ExtensionManagedMainMenu
        {
            get { return _extensionManagedMainMenu; }
        }

        public DataBoxManager DataBoxManager
        {
            get { return _dataBoxManager; }
        }

        #endregion //Properties

        #region Methods

        public void SetDataBoxManager(DataBoxManager dataBoxManager)
        {
            _dataBoxManager = dataBoxManager;
        }

        public abstract void AddExtensionManagedMenuItems();

        public abstract void SubscribeToAddedMenuItemsEvents();

        #endregion //Methods
    }
}