﻿namespace Figlut.Server
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FiglutServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.FiglutServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // FiglutServiceProcessInstaller
            // 
            this.FiglutServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.FiglutServiceProcessInstaller.Password = null;
            this.FiglutServiceProcessInstaller.Username = null;
            // 
            // FiglutServiceInstaller
            // 
            this.FiglutServiceInstaller.Description = "An out-of-the-box dynamic REST service for performing CRUD operations on database" +
                " servers.";
            this.FiglutServiceInstaller.DisplayName = "Figlut Web Service";
            this.FiglutServiceInstaller.ServiceName = "Figlut";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.FiglutServiceProcessInstaller,
            this.FiglutServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller FiglutServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller FiglutServiceInstaller;
    }
}