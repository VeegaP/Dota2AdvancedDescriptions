﻿using Dota2AdvancedDescriptions.Helpers;
using Dota2AdvancedDescriptions.Properties;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dota2AdvancedDescriptions.ViewModels
{
    public partial class MainWindowViewModel
    {
        private DelegateCommand<object> resetResourcesCommand;

        public DelegateCommand<object> ResetResourcesCommand
        {
            get
            {
                return this.resetResourcesCommand ?? (this.resetResourcesCommand = new DelegateCommand<object>((x) =>
                {
                    isPublishingResources = true;
                    CreateAdvancedDescriptionCommand.RaiseCanExecuteChanged();
                    ResetResourcesCommand.RaiseCanExecuteChanged();
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += delegate
                    {
                        _resourcesEditor.RevertResources(ResourcesFilePath);
                    };
                    worker.RunWorkerCompleted += delegate
                    {
                        isPublishingResources = false;
                        CreateAdvancedDescriptionCommand.RaiseCanExecuteChanged();
                        ResetResourcesCommand.RaiseCanExecuteChanged();
                    };
                    worker.RunWorkerAsync();
                }, x => { return !isPublishingResources && File.Exists(_resourcesEditor.GetBackupFile(ResourcesFilePath)); }));
            }
        }

        private bool isPublishingResources = false;
        private DelegateCommand<object> createAdvancedDescriptionCommand;

        public DelegateCommand<object> CreateAdvancedDescriptionCommand
        {
            get
            {
                return this.createAdvancedDescriptionCommand ?? (this.createAdvancedDescriptionCommand = new DelegateCommand<object>((x) =>
                {
                    if (!_htmlParser.ParseCompleted) return;

                    isPublishingResources = true;
                    CreateAdvancedDescriptionCommand.RaiseCanExecuteChanged();
                    ResetResourcesCommand.RaiseCanExecuteChanged();
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += delegate
                    {
                        //Revert required
                        if (File.ReadAllText(ResourcesFilePath).IndexOf(Resources.CreditsText) >= 0)
                        {
                            _resourcesParser.ParseResources(_resourcesEditor.GetBackupFile(ResourcesFilePath));
                            _resourcesEditor.PrepareResources(_resourcesEditor.GetBackupFile(ResourcesFilePath), _htmlParser.ParsedData, _resourcesParser.ParsedResources);
                        } else
                        {
                            _resourcesParser.ParseResources(ResourcesFilePath);
                            _resourcesEditor.PrepareResources(ResourcesFilePath, _htmlParser.ParsedData, _resourcesParser.ParsedResources);
                        }
                        
                        _resourcesEditor.PublishResources(ResourcesFilePath);
                        
                    };
                    worker.RunWorkerCompleted += delegate
                    {
                        isPublishingResources = false;
                        CreateAdvancedDescriptionCommand.RaiseCanExecuteChanged();
                        ResetResourcesCommand.RaiseCanExecuteChanged();
                    };
                    worker.RunWorkerAsync();
                }, x => {
                    return File.Exists(ResourcesFilePath) && _htmlParser != null && _htmlParser.ParseCompleted && !isPublishingResources;
                }));
            }
        }
    }
}